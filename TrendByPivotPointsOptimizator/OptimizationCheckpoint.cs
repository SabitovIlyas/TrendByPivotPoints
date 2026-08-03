using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>Состояние одной хромосомы в файле чек-поинта.</summary>
    public class CheckpointChromosome
    {
        public string TickerName { get; set; }
        public int TimeFrameIndex { get; set; }
        public int SideIndex { get; set; }
        public Dictionary<string, double> Genes { get; set; }

        /// <summary>Результат прогона; null — хромосома ещё не считалась.</summary>
        public ChromosomeMetrics Metrics { get; set; }
    }

    /// <summary>
    /// Снимок прогона оптимизатора: где остановились и с чем. Пишется после каждого
    /// поколения, чтобы после отключения питания продолжить с последнего поколения,
    /// а не с начала. Формат — текстовые строки «ключ:значение», как у файла настроек.
    /// </summary>
    public class OptimizationCheckpoint
    {
        public const int CurrentVersion = 1;

        /// <summary>Главный прогон бэктеста (перед форвардными периодами).</summary>
        public const string StageFinalBackward = "FinalBackward";

        /// <summary>Прогон одного форвардного периода.</summary>
        public const string StageForward = "Forward";

        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        /// <summary>Отпечаток настроек: с другими настройками продолжать нельзя.</summary>
        public string Fingerprint { get; set; }

        public int Seed { get; set; }
        public int RandomDraws { get; set; }
        public string Stage { get; set; } = StageFinalBackward;
        public int Period { get; set; }

        /// <summary>Сколько поколений текущего прогона уже завершено.</summary>
        public int Generation { get; set; }

        public double BestFitnessEver { get; set; } = double.MinValue;
        public int GenerationsWithoutImprovement { get; set; }

        /// <summary>Гены лучшей хромосомы главного прогона — затравка форвардных периодов.</summary>
        public Dictionary<string, double> BestGenes { get; set; }

        /// <summary>Итог главного прогона: в сводный отчёт он пишется последней строкой.</summary>
        public ForwardAnalysisResult FinalBackwardResult { get; set; }

        /// <summary>Уже посчитанные форвардные периоды — строки сводного отчёта.</summary>
        public List<ForwardAnalysisResult> CompletedResults { get; set; } =
            new List<ForwardAnalysisResult>();

        public List<CheckpointChromosome> Population { get; set; } =
            new List<CheckpointChromosome>();

        /// <summary>
        /// Отпечаток настроек и стратегии: всё, что меняет ход оптимизации. Если
        /// пользователь поправил настройки, старый чек-поинт к ним не подходит.
        /// </summary>
        public static string CalculateFingerprint(Settings settings,
            StrategyDefinition definition)
        {
            var builder = new StringBuilder();
            builder.Append(definition.Name).Append('|');
            builder.Append(string.Join(",", settings.Sides)).Append('|');
            builder.Append(string.Join(",",
                settings.TimeFrames.Select(t => t.ToString()))).Append('|');
            builder.Append(settings.PopulationSize).Append('|');
            builder.Append(settings.Generations).Append('|');
            builder.Append(settings.CrossoverRate.ToString("R", culture)).Append('|');
            builder.Append(settings.MutationRate.ToString("R", culture)).Append('|');
            builder.Append(settings.Patience).Append('|');
            builder.Append(settings.TournamentSize).Append('|');
            builder.Append(settings.MinDiversity.ToString("R", culture)).Append('|');
            builder.Append(settings.EliteFraction.ToString("R", culture)).Append('|');
            builder.Append(settings.BackwardDays).Append('|');
            builder.Append(settings.ForwardDays).Append('|');
            builder.Append(settings.ForwardPeriodsCount).Append('|');
            builder.Append(settings.ShiftWindowDays).Append('|');
            builder.Append(settings.Equity.ToString("R", culture)).Append('|');
            builder.Append(settings.RiskValuePrcnt.ToString("R", culture)).Append('|');
            builder.Append(settings.SecuritiesFile).Append('|');
            builder.Append(settings.TrimHistory).Append('|');

            //Настройки фитнес-функции меняют оценки, значит продолжать прогон,
            //начатый с другими, нельзя.
            builder.Append(settings.ExcludeBestDealsPrcnt.ToString("R", culture)).Append('|');
            builder.Append(settings.MinDealsCount).Append('|');
            builder.Append(settings.NeighbourhoodPoints).Append('|');
            builder.Append(settings.NeighbourhoodPercent.ToString("R", culture)).Append('|');
            builder.Append(settings.NeighbourhoodUseMedian).Append('|');

            foreach (var descriptor in definition.Parameters)
                builder.Append(descriptor.Name).Append('=')
                    .Append(descriptor.Min.ToString("R", culture)).Append(':')
                    .Append(descriptor.Max.ToString("R", culture)).Append(':')
                    .Append(descriptor.Step.ToString("R", culture)).Append(',');

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
                return string.Concat(hash.Take(16).Select(b => b.ToString("x2")));
            }
        }

        /// <summary>Складывает популяцию в снимок: гены и числа, без баров.</summary>
        public static List<CheckpointChromosome> FromPopulation(
            List<ChromosomeUniversal> population, Settings settings)
        {
            var result = new List<CheckpointChromosome>();
            foreach (var chromosome in population)
            {
                //Таймфрейм и сторону храним номером в списке настроек — так не нужно
                //разбирать их из текста при восстановлении.
                var timeFrameIndex = settings.TimeFrames.FindIndex(
                    t => t.ToString() == chromosome.TimeFrame.ToString());
                var sideIndex = settings.Sides.IndexOf(chromosome.Side);

                result.Add(new CheckpointChromosome()
                {
                    TickerName = chromosome.Ticker.Name,
                    TimeFrameIndex = Math.Max(0, timeFrameIndex),
                    SideIndex = Math.Max(0, sideIndex),
                    Genes = new Dictionary<string, double>(chromosome.Genes),
                    Metrics = double.IsNaN(chromosome.FitnessValue)
                        ? null : ChromosomeMetrics.From(chromosome),
                });
            }
            return result;
        }

        /// <summary>Разворачивает снимок обратно в популяцию.</summary>
        public List<ChromosomeUniversal> ToPopulation(Settings settings, List<Ticker> tickers)
        {
            var population = new List<ChromosomeUniversal>();
            foreach (var saved in Population)
            {
                var ticker = tickers.Find(t => t.Name == saved.TickerName) ?? tickers.First();
                var timeFrame = settings.TimeFrames[
                    Math.Min(saved.TimeFrameIndex, settings.TimeFrames.Count - 1)];
                var side = settings.Sides[
                    Math.Min(saved.SideIndex, settings.Sides.Count - 1)];

                var chromosome = new ChromosomeUniversal(ticker, timeFrame, side,
                    new Dictionary<string, double>(saved.Genes));
                saved.Metrics?.ApplyTo(chromosome);
                population.Add(chromosome);
            }
            return population;
        }

        public void Save(string fullFileName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Version:" + CurrentVersion);
            builder.AppendLine("Fingerprint:" + Fingerprint);
            builder.AppendLine("Seed:" + Seed.ToString(culture));
            builder.AppendLine("RandomDraws:" + RandomDraws.ToString(culture));
            builder.AppendLine("Stage:" + Stage);
            builder.AppendLine("Period:" + Period.ToString(culture));
            builder.AppendLine("Generation:" + Generation.ToString(culture));
            builder.AppendLine("BestFitnessEver:" + BestFitnessEver.ToString("R", culture));
            builder.AppendLine("GenerationsWithoutImprovement:" +
                GenerationsWithoutImprovement.ToString(culture));

            if (BestGenes != null)
                builder.AppendLine("BestGenes:" + GenesToString(BestGenes));

            if (FinalBackwardResult != null)
                builder.AppendLine("FinalBackwardResult:" + ResultToString(FinalBackwardResult));

            foreach (var result in CompletedResults)
                builder.AppendLine("Result:" + ResultToString(result));

            foreach (var chromosome in Population)
                builder.AppendLine("Chromosome:" + ChromosomeToString(chromosome));

            //Пишем через временный файл: обрыв питания посреди записи не должен
            //превращать чек-поинт в мусор.
            var tempFileName = fullFileName + ".tmp";
            File.WriteAllText(tempFileName, builder.ToString(), Encoding.UTF8);

            if (File.Exists(fullFileName))
                File.Delete(fullFileName);
            File.Move(tempFileName, fullFileName);
        }

        public static OptimizationCheckpoint Load(string fullFileName)
        {
            var checkpoint = new OptimizationCheckpoint();
            var version = 0;

            foreach (var line in File.ReadAllLines(fullFileName, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var separator = line.IndexOf(':');
                if (separator < 0)
                    continue;

                var key = line.Substring(0, separator);
                var value = line.Substring(separator + 1).Trim();

                switch (key)
                {
                    case "Version": version = int.Parse(value, culture); break;
                    case "Fingerprint": checkpoint.Fingerprint = value; break;
                    case "Seed": checkpoint.Seed = int.Parse(value, culture); break;
                    case "RandomDraws": checkpoint.RandomDraws = int.Parse(value, culture); break;
                    case "Stage": checkpoint.Stage = value; break;
                    case "Period": checkpoint.Period = int.Parse(value, culture); break;
                    case "Generation": checkpoint.Generation = int.Parse(value, culture); break;
                    case "BestFitnessEver":
                        checkpoint.BestFitnessEver = double.Parse(value, culture); break;
                    case "GenerationsWithoutImprovement":
                        checkpoint.GenerationsWithoutImprovement = int.Parse(value, culture); break;
                    case "BestGenes": checkpoint.BestGenes = ParseGenes(value); break;
                    case "FinalBackwardResult":
                        checkpoint.FinalBackwardResult = ParseResult(value); break;
                    case "Result": checkpoint.CompletedResults.Add(ParseResult(value)); break;
                    case "Chromosome":
                        checkpoint.Population.Add(ParseChromosome(value)); break;
                }
            }

            if (version != CurrentVersion)
                throw new FormatException($"Чек-поинт версии {version} не поддерживается " +
                    $"(нужна версия {CurrentVersion}).");

            return checkpoint;
        }

        private static string GenesToString(Dictionary<string, double> genes)
        {
            return string.Join(",", genes.OrderBy(g => g.Key)
                .Select(g => g.Key + "=" + g.Value.ToString("R", culture)));
        }

        private static Dictionary<string, double> ParseGenes(string text)
        {
            var genes = new Dictionary<string, double>();
            if (string.IsNullOrWhiteSpace(text))
                return genes;

            foreach (var pair in text.Split(','))
            {
                var parts = pair.Split('=');
                if (parts.Length != 2)
                    throw new FormatException("Не разобрать ген: " + pair);
                genes[parts[0]] = double.Parse(parts[1], culture);
            }
            return genes;
        }

        private static string ResultToString(ForwardAnalysisResult result)
        {
            return string.Join(";", new[]
            {
                result.BackwardFitness.ToString("R", culture),
                result.ForwardFitness.ToString("R", culture),
                result.BackwardStart.Ticks.ToString(culture),
                result.BackwardEnd.Ticks.ToString(culture),
                result.ForwardStart.Ticks.ToString(culture),
                result.ForwardEnd.Ticks.ToString(culture),
                result.BackwardProfit.ToString("R", culture),
                result.ForwardProfit.ToString("R", culture),
                result.BackwardProfitPrcnt.ToString("R", culture),
                result.ForwardProfitPrcnt.ToString("R", culture),
            });
        }

        private static ForwardAnalysisResult ParseResult(string text)
        {
            var parts = text.Split(';');
            if (parts.Length < 10)
                throw new FormatException("Не разобрать результат периода: " + text);

            return new ForwardAnalysisResult()
            {
                BackwardFitness = double.Parse(parts[0], culture),
                ForwardFitness = double.Parse(parts[1], culture),
                BackwardStart = new DateTime(long.Parse(parts[2], culture)),
                BackwardEnd = new DateTime(long.Parse(parts[3], culture)),
                ForwardStart = new DateTime(long.Parse(parts[4], culture)),
                ForwardEnd = new DateTime(long.Parse(parts[5], culture)),
                BackwardProfit = double.Parse(parts[6], culture),
                ForwardProfit = double.Parse(parts[7], culture),
                BackwardProfitPrcnt = double.Parse(parts[8], culture),
                ForwardProfitPrcnt = double.Parse(parts[9], culture),
            };
        }

        private static string ChromosomeToString(CheckpointChromosome chromosome)
        {
            return string.Join("|", new[]
            {
                chromosome.TickerName,
                chromosome.TimeFrameIndex.ToString(culture),
                chromosome.SideIndex.ToString(culture),
                GenesToString(chromosome.Genes),
                chromosome.Metrics == null ? string.Empty : chromosome.Metrics.ToLine(),
            });
        }

        private static CheckpointChromosome ParseChromosome(string text)
        {
            var parts = text.Split('|');
            if (parts.Length < 5)
                throw new FormatException("Не разобрать хромосому: " + text);

            return new CheckpointChromosome()
            {
                TickerName = parts[0],
                TimeFrameIndex = int.Parse(parts[1], culture),
                SideIndex = int.Parse(parts[2], culture),
                Genes = ParseGenes(parts[3]),
                Metrics = string.IsNullOrWhiteSpace(parts[4])
                    ? null : ChromosomeMetrics.Parse(parts[4]),
            };
        }
    }
}
