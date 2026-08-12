using Newtonsoft.Json;
using PeparatorDataForSpreadTradingSystems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TradingSystems;
using TrendByPivotPointsStarter;
using TSLab.DataSource;
using TSLab.Script;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class OptimizatorGeneticAlgorithmStarter
    {
        // Сид генератора случайных чисел. null — случайный запуск (боевой режим).
        // Задайте число, чтобы прогоны стали воспроизводимыми (отладка, регрессии).
        private int? seed = null;

        // Отчёты пишем в UTF-8 с меткой кодировки: без неё Excel на русской
        // Windows открывает файл как ANSI и русские заголовки превращаются в мусор.
        private static readonly System.Text.Encoding CsvEncoding =
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

        public void Start()
        {
            var logger = new ConsoleLogger();

            var startTime = DateTime.Now;
            logger.Log("Старт! {0}\r\n", startTime);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файл с настройками";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            var fullFileName = openFileDialog.FileName;

            var settings = CreateSettings(fullFileName);

            //Если в настройках указана стратегия — работаем через универсальный
            //оптимизатор; иначе — прежний путь Дончиана.
            var definition = CreateStrategyDefinitionWithOverrides(settings);
            if (definition != null)
            {
                StartUniversal(settings, definition, openFileDialog, logger, startTime);
                return;
            }

            openFileDialog.Title = "Выберите файл с инструментами";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;           

            fullFileName = openFileDialog.FileName;

            List<SecurityData> securitiesData = GetSecuritiesData(fullFileName);

            var loggerNull = new LoggerNull();

            List<Ticker> tickers = CreateTickers(securitiesData, fullFileName, settings, loggerNull);

            var converter = ConverterTextDataToBar.Create(fullFileName);
            var fileName = fullFileName.Split('\\').Last();
            var securityName = fileName.Split('.').First();
            var results = new List<ForwardAnalysisResult>();

            List<ChromosomeDonchianChannel> bestPopulation = null;
            List<ChromosomeDonchianChannel> bestPopulationLast = null;
            ChromosomeDonchianChannel bestChromosome = null;
            SurogateChromosome bestSurogateChromosome = null;

            openFileDialog.Title = "Выберите файл с инструментами";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fullFileName = openFileDialog.FileName;                
                bestSurogateChromosome = CreateBestChromosome(fullFileName);
            }

            var resultFileName = $"{tickers.First().Name}_{settings.Sides.First()}.csv";
            CreateTxtFile(resultFileName);
            try
            {
                var context = new ContextLab();
                var randomProvider = seed.HasValue
                    ? new RandomProvider(seed.Value)
                    : new RandomProvider();

                var optimizator = Optimizator.Create();
                var ga = new GeneticAlgorithmDonchianChannel(populationSize: 100, generations: 300,
                    crossoverRate: 0.85, mutationRate: 0.10, randomProvider, tickers, settings, context,
                    optimizator, loggerNull);

                logger.Log("Старт генетического алгоритма");
                logger.Log("Актуальная оптимизация!");
                ga.IsLastBackwardTesting = true;
                bestPopulationLast = ga.Run(period: 0, bestSurogateChromosome);

                foreach (var chromosome in bestPopulationLast)
                    chromosome.ForwardAnalysisResults.First().BackwardFitness =
                        chromosome.FitnessValue;

                var sumResults = 0d;
                foreach (var chromosome in bestPopulationLast)
                    sumResults += chromosome.ForwardAnalysisResults.First().BackwardFitness;

                var avgResults = sumResults / bestPopulationLast.Count;
                var tmpRes = new ForwardAnalysisResult() { BackwardFitness = avgResults, };

                if (bestPopulationLast.Count > 0)
                {
                    tmpRes.BackwardStart = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardStart;
                    tmpRes.BackwardEnd = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardEnd;
                    tmpRes.BackwardProfit = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardProfit;
                    tmpRes.BackwardProfitPrcnt = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardProfitPrcnt;
                }

                PrintToTxtFile(bestPopulationLast);
                bestChromosome = bestPopulationLast.First();

                ga.IsLastBackwardTesting = false;
                for (var period = 0; period < 10; period++)
                {
                    logger.Log("Период № {0}", period + 1);
                    bestPopulation = ga.Run(period, bestChromosome);

                    foreach (var chromosome in bestPopulation)
                        chromosome.ForwardAnalysisResults.First().BackwardFitness =
                            chromosome.FitnessValue;                    

                    foreach (var chromosome in bestPopulation)
                        chromosome.SetForwardBarsAsTickerBars();

                    foreach (var chromosome in bestPopulation)                    
                        chromosome.FitnessDonchianChannel.SetUpChromosomeFitnessValue(isCriteriaPassedNeedToCheck:
                            false);                    

                    foreach (var chromosome in bestPopulation)
                        chromosome.ForwardAnalysisResults.First().ForwardFitness =
                                chromosome.FitnessValue;

                    var sumResultsBackward = 0d;
                    var sumResultsForward = 0d;
                    foreach (var chromosome in bestPopulation)
                    {
                        sumResultsBackward += chromosome.ForwardAnalysisResults.First().BackwardFitness;
                        sumResultsForward += chromosome.ForwardAnalysisResults.First().ForwardFitness;
                    }

                    var avgResultsBackward = sumResultsBackward / bestPopulation.Count;
                    var avgResultsForward = sumResultsForward / bestPopulation.Count;

                    var tmp = new ForwardAnalysisResult()
                    {
                        BackwardFitness = avgResultsBackward,
                        ForwardFitness = avgResultsForward,                        
                    };

                    if (bestPopulation.Count > 0)
                    {
                        tmp.BackwardStart = bestPopulation.First().ForwardAnalysisResults.First().BackwardStart;
                        tmp.BackwardEnd = bestPopulation.First().ForwardAnalysisResults.First().BackwardEnd;
                        tmp.ForwardStart = bestPopulation.First().ForwardAnalysisResults.First().ForwardStart;
                        tmp.ForwardEnd = bestPopulation.First().ForwardAnalysisResults.First().ForwardEnd;
                        tmp.BackwardProfit = bestPopulation.First().ForwardAnalysisResults.First().BackwardProfit;
                        tmp.ForwardProfit = bestPopulation.First().ForwardAnalysisResults.First().ForwardProfit;
                        tmp.BackwardProfitPrcnt = bestPopulation.First().ForwardAnalysisResults.First().BackwardProfitPrcnt;
                        tmp.ForwardProfitPrcnt = bestPopulation.First().ForwardAnalysisResults.First().ForwardProfitPrcnt;
                    }

                    results.Add(tmp);
                    AppendToTxtFile(tmp, resultFileName);

                    var bestPopulationFile = $"{tickers.First().Name}_{settings.Sides.First()}_Period_{period}.csv";
                    CreateTxtFile(bestPopulationFile);
                    PrintToTxtFile(bestPopulation, bestPopulationFile);
                }
                results.Add(tmpRes);
                AppendToTxtFile(tmpRes, resultFileName);

                var stopTime = DateTime.Now;
                logger.Log("Стоп {0}", stopTime);

                var duration = stopTime-startTime;
                logger.Log("Время выполнения {0}", duration);
                logger.Log("Генетический алгоритм завершил работу.");
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }           

            Console.ReadLine();
        }

        /// <summary>Фабрика описаний стратегий по настройкам (имя + сторона торговли).</summary>
        public StrategyDefinition CreateStrategyDefinition(Settings settings)
        {
            var strategyName = settings.Strategy;
            if (string.IsNullOrEmpty(strategyName))
                return null;

            switch (strategyName.Trim().ToLowerInvariant())
            {
                case "meanreversion":
                    if (settings.Sides == null || settings.Sides.Count != 1)
                        throw new Exception("Стратегия MeanReversion торгует только одну " +
                            "сторону: укажите в настройках ровно один PositionSide.");
                    return new MeanReversionStrategyDefinition(settings.Sides.First());
                case "donchian":
                case "donchianuniversal":
                    return new DonchianStrategyDefinition();
                default:
                    throw new Exception("Неизвестная стратегия в файле настроек: " +
                        strategyName);
            }
        }

        private StrategyDefinition CreateStrategyDefinitionWithOverrides(Settings settings)
        {
            var definition = CreateStrategyDefinition(settings);
            definition?.ApplyRangeOverrides(settings);
            return definition;
        }

        //Оптимизация через универсальный генетический алгоритм: стратегия и все
        //параметры задаются файлом настроек, хардкода нет. Диалоги показываются
        //только если пути к файлам не заданы в настройках; вся работа —
        //в StartUniversalCore.
        private void StartUniversal(Settings settings, StrategyDefinition definition,
            OpenFileDialog openFileDialog, Logger logger, DateTime startTime)
        {
            var securitiesFileName = settings.SecuritiesFile;
            if (string.IsNullOrEmpty(securitiesFileName) || !File.Exists(securitiesFileName))
            {
                openFileDialog.Title = "Выберите файл с инструментами";
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                securitiesFileName = openFileDialog.FileName;
            }

            Dictionary<string, double> seedGenes = null;
            if (!string.IsNullOrEmpty(settings.SeedGenesFile) &&
                File.Exists(settings.SeedGenesFile))
            {
                seedGenes = LoadSeedGenes(settings.SeedGenesFile);
            }
            else
            {
                openFileDialog.Title = "Выберите файл с лучшей хромосомой (необязательно)";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    seedGenes = LoadSeedGenes(openFileDialog.FileName);
            }

            StartUniversalCore(settings, definition, securitiesFileName, seedGenes, logger,
                startTime);

            Console.ReadLine();
        }

        /// <summary>
        /// Полностью неинтерактивный запуск по файлу настроек: стратегия, диапазоны
        /// и пути к данным берутся из файла. Используется при запуске из
        /// Менеджера проектов (путь к файлу настроек — аргумент командной строки).
        /// </summary>
        public void StartFromSettingsFile(string settingsFileName)
        {
            var startTime = DateTime.Now;
            var settings = CreateSettings(settingsFileName);
            var logger = CreateLogger(settings, startTime);

            logger.Log("Старт! {0}\r\n", startTime);
            logger.Log("Файл настроек: {0}", settingsFileName);

            try
            {
                var definition = CreateStrategyDefinitionWithOverrides(settings);
                if (definition == null)
                    throw new Exception("В файле настроек не указана стратегия " +
                        "(строка Strategy:).");

                if (string.IsNullOrEmpty(settings.SecuritiesFile))
                    throw new Exception("В файле настроек не задан файл с описанием " +
                        "инструментов (строка SecuritiesFile:).");

                if (!File.Exists(settings.SecuritiesFile))
                    throw new Exception("Не найден файл с описанием инструментов: " +
                        settings.SecuritiesFile);

                LogSettings(settings, definition, logger);

                Dictionary<string, double> seedGenes = null;
                if (string.IsNullOrEmpty(settings.SeedGenesFile))
                    logger.Log("Затравочная хромосома не задана — стартовая популяция " +
                        "будет полностью случайной.");
                else if (!File.Exists(settings.SeedGenesFile))
                    throw new Exception("Не найден файл затравочной хромосомы: " +
                        settings.SeedGenesFile + ". Уберите строку SeedGenesFile из " +
                        "настроек, если затравка не нужна.");
                else
                {
                    seedGenes = LoadSeedGenes(settings.SeedGenesFile);
                    logger.Log("Затравочная хромосома: {0}", settings.SeedGenesFile);
                }

                StartUniversalCore(settings, definition, settings.SecuritiesFile, seedGenes,
                    logger, startTime);
            }
            catch (Exception ex)
            {
                logger.Log("\r\nОптимизатор остановлен из-за ошибки:\r\n{0}", ex.ToString());
            }
        }

        //Журнал ведём и в консоль, и в файл: прогон идёт часами, консоль к разбору
        //полётов уже не сохранить.
        private Logger CreateLogger(Settings settings, DateTime startTime)
        {
            var consoleLogger = new ConsoleLogger();

            var logFileName = settings.LogFile;
            if (string.IsNullOrEmpty(logFileName))
                logFileName = string.Format("Optimizator_{0:yyyy-MM-dd_HH-mm-ss}.log",
                    startTime);

            try
            {
                var fullLogFileName = Path.GetFullPath(logFileName);
                var directory = Path.GetDirectoryName(fullLogFileName);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                consoleLogger.Log("Журнал прогона: {0}", fullLogFileName);
                return new LoggerCombined(consoleLogger, new LoggerTxtFile(fullLogFileName));
            }
            catch (Exception ex)
            {
                consoleLogger.Log("Не удалось создать файл журнала «{0}»: {1}. " +
                    "Продолжаем без него.", logFileName, ex.Message);
                return consoleLogger;
            }
        }

        //Полный слепок настроек в журнале: по нему потом видно, чем именно был
        //запущен прогон.
        private void LogSettings(Settings settings, StrategyDefinition definition,
            Logger logger)
        {
            logger.Log("Стратегия: {0}", definition.Name);
            logger.Log("Стороны: {0}", string.Join(", ", settings.Sides));
            logger.Log("Таймфреймы: {0}", settings.TimeFrames.Count);
            logger.Log("Сид: {0}", settings.Seed.HasValue
                ? settings.Seed.Value.ToString() : "случайный");
            logger.Log("Популяция {0}, поколений {1}, кроссовер {2}, мутация {3}, " +
                "терпение {4}, турнир {5}, минимальное разнообразие {6}",
                settings.PopulationSize, settings.Generations, settings.CrossoverRate,
                settings.MutationRate, settings.Patience, settings.TournamentSize,
                settings.MinDiversity);
            logger.Log("Элита: {0:P0} популяции — {1} особей переходят в следующее " +
                "поколение без пересчёта", settings.EliteFraction,
                GeneticAlgorithmUniversal.GetEliteCount(settings));
            logger.Log("Потоков на расчёт хромосом: {0}", settings.Threads > 0
                ? settings.Threads.ToString()
                : $"по числу ядер ({Environment.ProcessorCount})");
            logger.Log("Фитнес-функция: фактор восстановления после исключения {0:P0} " +
                "лучших прибыльных сделок; порог числа сделок {1}",
                settings.ExcludeBestDealsPrcnt, settings.MinDealsCount);
            logger.Log("Штрафы: просадка глубже {0} % и доля выигрышных ниже {1} %, " +
                "жёсткость {2} (0 — штраф выключен)", settings.MaxDrawDownPrcnt,
                settings.MinWinRatePrcnt, settings.PenaltyPower);
            logger.Log("Окрестность: {0}", settings.NeighbourhoodPoints > 0
                ? string.Format("{0} точек в пределах {1:P0} диапазона каждого гена, " +
                    "оценка — {2}", settings.NeighbourhoodPoints,
                    settings.NeighbourhoodPercent,
                    settings.NeighbourhoodUseMedian ? "медиана" : "среднее")
                : "не используется, оценивается только сама хромосома");
            logger.Log("Сохранение состояния прогона: {0}", settings.SaveCheckpoint
                ? "после каждого поколения" : "выключено");
            logger.Log("Окна: бэктест {0} дней, форвард {1} дней, периодов {2}, " +
                "смещение {3} дней", settings.BackwardDays, settings.ForwardDays,
                settings.ForwardPeriodsCount, settings.ShiftWindowDays);
            logger.Log("Капитал {0}, риск на сделку {1} %", settings.Equity,
                settings.RiskValuePrcnt);
            logger.Log("Файл с описанием инструментов: {0}", settings.SecuritiesFile);

            logger.Log("Диапазоны поиска параметров:");
            foreach (var descriptor in definition.Parameters)
                logger.Log("    {0}: от {1} до {2}, шаг {3}", descriptor.Name,
                    descriptor.Min, descriptor.Max, descriptor.Step);
        }

        /// <summary>
        /// Ядро универсальной оптимизации без пользовательского интерфейса —
        /// пригодно для автоматизированных запусков.
        /// </summary>
        public void StartUniversalCore(Settings settings, StrategyDefinition definition,
            string securitiesFileName, Dictionary<string, double> seedGenes, Logger logger,
            DateTime startTime)
        {
            if (settings.Sides == null || settings.Sides.Count == 0)
                throw new Exception("В файле настроек не указана сторона торговли " +
                    "(строка PositionSide: Long или Short).");

            if (settings.TimeFrames == null || settings.TimeFrames.Count == 0)
                throw new Exception("В файле настроек не указан таймфрейм " +
                    "(строка TimeFrames: 01min, 05min, 15min, 30min, 60min или 1d).");

            var securitiesData = GetSecuritiesData(securitiesFileName);
            var loggerNull = new LoggerNull();
            var tickers = CreateTickers(securitiesData, securitiesFileName, settings,
                loggerNull, settings.TrimHistory, logger);

            if (tickers.Count == 0)
                throw new Exception("Не удалось загрузить ни одного инструмента из файла " +
                    securitiesFileName);

            var results = new List<ForwardAnalysisResult>();
            var reportName = $"{tickers.First().Name}_{settings.Sides.First()}_" +
                $"{definition.Name}";

            //Чек-поинт лежит в рабочей папке, а не в папке результатов: его надо
            //найти до того, как станет известно, куда писал прерванный прогон.
            var checkpointFileName = GetCheckpointFileName(settings, reportName + ".csv");
            var fingerprint = OptimizationCheckpoint.CalculateFingerprint(settings, definition);
            var checkpoint = LoadCheckpoint(checkpointFileName, fingerprint, logger);

            try
            {
                var context = new ContextLab();
                var effectiveSeed = checkpoint != null
                    ? checkpoint.Seed
                    : settings.Seed ?? seed ?? Environment.TickCount;

                //Сид всегда конкретный и записан в журнал: без этого прогон нельзя
                //ни повторить, ни продолжить с чек-поинта.
                var randomProvider = new RandomProvider(effectiveSeed);
                logger.Log("Сид генератора случайных чисел: {0}", effectiveSeed);

                var ga = new GeneticAlgorithmUniversal(settings.PopulationSize,
                    settings.Generations, settings.CrossoverRate, settings.MutationRate,
                    randomProvider, tickers, settings, context, definition, loggerNull,
                    logger);

                var state = new OptimizationCheckpoint()
                {
                    Fingerprint = fingerprint,
                    Seed = effectiveSeed,
                };

                //Отчёты — в свою папку на каждый прогон, иначе следующий запуск
                //затирает результаты предыдущего. Продолженный прогон дописывает
                //в ту же папку, что и прерванный: она запомнена в чек-поинте.
                state.ResultsFolder = CreateResultsFolder(settings, checkpoint,
                    startTime, logger);
                var resultFileName = Path.Combine(state.ResultsFolder, reportName + ".csv");

                if (checkpoint != null)
                {
                    randomProvider.Replay(checkpoint.RandomDraws);
                    state.Stage = checkpoint.Stage;
                    state.Period = checkpoint.Period;
                    state.BestGenes = checkpoint.BestGenes;
                    state.FinalBackwardResult = checkpoint.FinalBackwardResult;
                    state.CompletedResults = checkpoint.CompletedResults;

                    logger.Log("Продолжаем прерванный прогон: этап {0}, период {1}, " +
                        "готовых периодов {2}.", state.Stage, state.Period + 1,
                        state.CompletedResults.Count);
                }

                if (settings.SaveCheckpoint)
                    ga.GenerationCompleted = progress => SaveCheckpoint(state, progress,
                        randomProvider, settings, checkpointFileName, logger);

                //Сводный отчёт переписываем с нуля, возвращая в него уже посчитанные
                //периоды: дописывать в старый файл после перезапуска нельзя.
                results.AddRange(state.CompletedResults);
                WriteSummaryReport(results, resultFileName, logger);

                logger.Log("Старт генетического алгоритма");
                var tmpRes = state.FinalBackwardResult;

                if (state.Stage == OptimizationCheckpoint.StageFinalBackward)
                {
                    logger.Log("Актуальная оптимизация!");
                    ga.IsLastBackwardTesting = true;
                    var bestPopulationLast = ga.Run(period: 0, seedGenes,
                        ToProgress(checkpoint, settings, tickers,
                            OptimizationCheckpoint.StageFinalBackward, period: 0));

                    foreach (var chromosome in bestPopulationLast)
                    {
                        var result = chromosome.ForwardAnalysisResults.First();
                        result.BackwardFitness = chromosome.FitnessValue;
                        result.BackwardProfit = chromosome.Profit;
                        result.BackwardProfitPrcnt = chromosome.ProfitPrcnt;
                        result.BackwardMaxDrawDown = chromosome.MaxDrawDown;
                        result.BackwardRecoveryFactor = chromosome.RecoveryFactor;
                        result.BackwardDealsStatistics = chromosome.DealsStatistics;
                    }

                    var sumResults = 0d;
                    foreach (var chromosome in bestPopulationLast)
                        sumResults += chromosome.ForwardAnalysisResults.First().BackwardFitness;

                    var avgResults = sumResults / bestPopulationLast.Count;
                    tmpRes = new ForwardAnalysisResult() { BackwardFitness = avgResults, };

                    if (bestPopulationLast.Count > 0)
                    {
                        var first = bestPopulationLast.First().ForwardAnalysisResults.First();
                        tmpRes.BackwardStart = first.BackwardStart;
                        tmpRes.BackwardEnd = first.BackwardEnd;
                        tmpRes.BackwardProfit = first.BackwardProfit;
                        tmpRes.BackwardProfitPrcnt = first.BackwardProfitPrcnt;
                    }

                    PrintToTxtFile(bestPopulationLast, definition, logger,
                        Path.Combine(state.ResultsFolder, reportName + "_params.csv"));

                    state.BestGenes = bestPopulationLast.First().Genes;
                    state.FinalBackwardResult = tmpRes;
                    state.Stage = OptimizationCheckpoint.StageForward;
                    state.Period = 0;
                    SaveCheckpoint(state, progress: null, randomProvider: randomProvider,
                        settings: settings, fullFileName: checkpointFileName, logger: logger);
                }

                if (state.BestGenes == null)
                    throw new Exception("В чек-поинте нет лучшей хромосомы главного " +
                        "прогона — продолжить форвардные периоды не с чего.");

                ga.IsLastBackwardTesting = false;
                for (var period = state.Period; period < settings.ForwardPeriodsCount; period++)
                {
                    state.Period = period;
                    logger.Log("Период № {0}", period + 1);
                    var bestPopulation = ga.Run(period, state.BestGenes,
                        ToProgress(checkpoint, settings, tickers,
                            OptimizationCheckpoint.StageForward, period));

                    foreach (var chromosome in bestPopulation)
                    {
                        var result = chromosome.ForwardAnalysisResults.First();
                        result.BackwardFitness = chromosome.FitnessValue;
                        result.BackwardProfit = chromosome.Profit;
                        result.BackwardProfitPrcnt = chromosome.ProfitPrcnt;
                        result.BackwardMaxDrawDown = chromosome.MaxDrawDown;
                        result.BackwardRecoveryFactor = chromosome.RecoveryFactor;
                        result.BackwardDealsStatistics = chromosome.DealsStatistics;
                    }

                    //Форвардный тест: та же стратегия на барах форвардного окна.
                    foreach (var chromosome in bestPopulation)
                        chromosome.Fitness.Bars =
                            chromosome.ForwardAnalysisResults.First().ForwardBars;

                    foreach (var chromosome in bestPopulation)
                        chromosome.Fitness.SetUpChromosomeFitnessValue(
                            isCriteriaPassedNeedToCheck: false);

                    foreach (var chromosome in bestPopulation)
                    {
                        var result = chromosome.ForwardAnalysisResults.First();
                        result.ForwardFitness = chromosome.FitnessValue;
                        result.ForwardProfit = chromosome.Profit;
                        result.ForwardProfitPrcnt = chromosome.ProfitPrcnt;
                        result.ForwardMaxDrawDown = chromosome.MaxDrawDown;
                        result.ForwardRecoveryFactor = chromosome.RecoveryFactor;
                        result.ForwardDealsStatistics = chromosome.DealsStatistics;
                    }

                    var sumResultsBackward = 0d;
                    var sumResultsForward = 0d;
                    foreach (var chromosome in bestPopulation)
                    {
                        sumResultsBackward += chromosome.ForwardAnalysisResults.First().BackwardFitness;
                        sumResultsForward += chromosome.ForwardAnalysisResults.First().ForwardFitness;
                    }

                    var avgResultsBackward = sumResultsBackward / bestPopulation.Count;
                    var avgResultsForward = sumResultsForward / bestPopulation.Count;

                    var tmp = new ForwardAnalysisResult()
                    {
                        BackwardFitness = avgResultsBackward,
                        ForwardFitness = avgResultsForward,
                    };

                    if (bestPopulation.Count > 0)
                    {
                        tmp.BackwardStart = bestPopulation.First().ForwardAnalysisResults.First().BackwardStart;
                        tmp.BackwardEnd = bestPopulation.First().ForwardAnalysisResults.First().BackwardEnd;
                        tmp.ForwardStart = bestPopulation.First().ForwardAnalysisResults.First().ForwardStart;
                        tmp.ForwardEnd = bestPopulation.First().ForwardAnalysisResults.First().ForwardEnd;
                        tmp.BackwardProfit = bestPopulation.First().ForwardAnalysisResults.First().BackwardProfit;
                        tmp.ForwardProfit = bestPopulation.First().ForwardAnalysisResults.First().ForwardProfit;
                        tmp.BackwardProfitPrcnt = bestPopulation.First().ForwardAnalysisResults.First().BackwardProfitPrcnt;
                        tmp.ForwardProfitPrcnt = bestPopulation.First().ForwardAnalysisResults.First().ForwardProfitPrcnt;

                        //Показатели по сделкам и просадку тоже переносим: по ним
                        //считается итог по всем форвардным отрезкам. Без них он
                        //молча выходил нулевым.
                        CopyWindowMetrics(bestPopulation.First().ForwardAnalysisResults.First(),
                            tmp);
                    }

                    results.Add(tmp);
                    WriteSummaryReport(results, resultFileName, logger);

                    var bestPopulationFile = Path.Combine(state.ResultsFolder,
                        $"{reportName}_Period_{period}.csv");
                    PrintToTxtFile(bestPopulation, definition, logger, bestPopulationFile);

                    //Период закрыт: следующий перезапуск начнёт со следующего.
                    state.CompletedResults.Add(tmp);
                    state.Period = period + 1;
                    SaveCheckpoint(state, progress: null, randomProvider: randomProvider,
                        settings: settings, fullFileName: checkpointFileName, logger: logger);
                }
                //Итог по форвардным отрезкам считаем до того, как добавим строку
                //главного прогона: у неё форвардной части нет.
                var forwardSummary = ForwardSummary.Calculate(results);

                results.Add(tmpRes);
                WriteSummaryReport(results, resultFileName, logger);

                logger.Log("");
                foreach (var line in forwardSummary.ToLines())
                    logger.Log(line);
                logger.Log("");

                DeleteCheckpoint(checkpointFileName, logger);

                var stopTime = DateTime.Now;
                logger.Log("Стоп {0}", stopTime);

                var duration = stopTime - startTime;
                logger.Log("Время выполнения {0}", duration);
                logger.Log("Генетический алгоритм завершил работу.");
            }
            catch (Exception e)
            {
                logger.Log("\r\nОшибка во время оптимизации:\r\n{0}", e.ToString());
            }
        }

        /// <summary>
        /// Папка для отчётов прогона. Продолженный прогон пишет туда же, куда писал
        /// прерванный — иначе его результаты растеклись бы по двум папкам. Если папка
        /// не задана настройкой, заводится своя, с датой и временем старта: так
        /// следующий прогон не затирает результаты предыдущего.
        /// </summary>
        public string CreateResultsFolder(Settings settings,
            OptimizationCheckpoint checkpoint, DateTime startTime, Logger logger)
        {
            var folder = checkpoint != null && !string.IsNullOrEmpty(checkpoint.ResultsFolder)
                ? checkpoint.ResultsFolder
                : settings.ResultsFolder;

            if (string.IsNullOrEmpty(folder))
                folder = Path.GetFullPath(string.Format("Результаты_{0:yyyy-MM-dd_HH-mm-ss}",
                    startTime));

            try
            {
                Directory.CreateDirectory(folder);
                logger.Log("Отчёты прогона: {0}", folder);
                return folder;
            }
            catch (Exception e)
            {
                //Без папки прогон всё равно должен идти — пишем рядом с программой.
                logger.Log("Не удалось создать папку отчётов «{0}»: {1}. " +
                    "Отчёты будут в рабочей папке.", folder, e.Message);
                return string.Empty;
            }
        }

        /// <summary>Куда писать чек-поинт: из настроек либо рядом со сводным отчётом.</summary>
        public string GetCheckpointFileName(Settings settings, string resultFileName)
        {
            if (!string.IsNullOrEmpty(settings.CheckpointFile))
                return Path.GetFullPath(settings.CheckpointFile);

            return Path.GetFullPath(Path.ChangeExtension(resultFileName, null) +
                "_checkpoint.txt");
        }

        /// <summary>
        /// Читает чек-поинт, если он подходит к текущим настройкам. Чужой или битый
        /// файл не должен ломать запуск — тогда прогон просто начинается сначала.
        /// </summary>
        public OptimizationCheckpoint LoadCheckpoint(string fullFileName,
            string fingerprint, Logger logger)
        {
            if (!File.Exists(fullFileName))
                return null;

            try
            {
                var checkpoint = OptimizationCheckpoint.Load(fullFileName);
                if (checkpoint.Fingerprint != fingerprint)
                {
                    logger.Log("Найден чек-поинт «{0}», но он от прогона с другими " +
                        "настройками — начинаем сначала. Меняются и настройки " +
                        "фитнес-функции, и сид, и содержимое файла инструментов, и " +
                        "сами котировки: продолжать прогон, начатый по другим " +
                        "правилам, нельзя.", fullFileName);
                    return null;
                }

                logger.Log("Найден чек-поинт: {0}", fullFileName);
                return checkpoint;
            }
            catch (Exception e)
            {
                logger.Log("Не удалось прочитать чек-поинт «{0}»: {1}. " +
                    "Начинаем сначала.", fullFileName, e.Message);
                return null;
            }
        }

        private void SaveCheckpoint(OptimizationCheckpoint state, GaProgress progress,
            RandomProvider randomProvider, Settings settings, string fullFileName,
            Logger logger)
        {
            if (!settings.SaveCheckpoint)
                return;

            try
            {
                state.RandomDraws = randomProvider.DrawsCount;
                state.Generation = progress == null ? 0 : progress.Generation;
                state.BestFitnessEver = progress == null
                    ? double.MinValue : progress.BestFitnessEver;
                state.GenerationsWithoutImprovement = progress == null
                    ? 0 : progress.GenerationsWithoutImprovement;
                state.Population = progress == null
                    ? new List<CheckpointChromosome>()
                    : OptimizationCheckpoint.FromPopulation(progress.Population, settings);

                state.Save(fullFileName);
            }
            catch (Exception e)
            {
                //Прогон важнее чек-поинта: не смогли сохранить — идём дальше.
                logger.Log("Не удалось сохранить чек-поинт «{0}»: {1}",
                    fullFileName, e.Message);
            }
        }

        private void DeleteCheckpoint(string fullFileName, Logger logger)
        {
            try
            {
                if (File.Exists(fullFileName))
                {
                    File.Delete(fullFileName);
                    logger.Log("Прогон завершён, чек-поинт удалён.");
                }
            }
            catch (Exception e)
            {
                logger.Log("Не удалось удалить чек-поинт «{0}»: {1}",
                    fullFileName, e.Message);
            }
        }

        /// <summary>
        /// Разворачивает чек-поинт в состояние генетического алгоритма — но только
        /// если он про этот самый этап и период и в нём есть незаконченное поколение.
        /// </summary>
        public GaProgress ToProgress(OptimizationCheckpoint checkpoint, Settings settings,
            List<Ticker> tickers, string stage, int period)
        {
            if (checkpoint == null || checkpoint.Stage != stage ||
                checkpoint.Period != period || checkpoint.Generation <= 0 ||
                checkpoint.Population.Count == 0)
                return null;

            return new GaProgress()
            {
                Generation = checkpoint.Generation,
                BestFitnessEver = checkpoint.BestFitnessEver,
                GenerationsWithoutImprovement = checkpoint.GenerationsWithoutImprovement,
                Population = checkpoint.ToPopulation(settings, tickers),
            };
        }

        //Затравочные гены: JSON-словарь «имя параметра — значение».
        private Dictionary<string, double> LoadSeedGenes(string fullFileName)
        {
            var file = File.ReadAllText(fullFileName);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<Dictionary<string, double>>(
                new JsonTextReader(new StringReader(file)));
        }

        private void PrintToTxtFile(List<ChromosomeUniversal> population,
            StrategyDefinition definition, Logger logger, string fileName = "")
        {
            if (population == null || population.Count == 0)
                return;

            var t = population.Last();

            if (fileName == "")
                fileName = $"{t.Ticker.Name}_{t.Side}_{definition.Name}_params.csv";

            TryWriteReport(fileName, writer =>
            {
                //Заголовки: общие колонки + параметры стратегии + два блока
                //показателей, бэктест и форвард. Разрыв между блоками — это и есть
                //мера переобучения, ради неё они и стоят рядом.
                var header = $"{nameof(t.TimeFrame)};{nameof(t.Side)};{nameof(t.Ticker.Name)}";
                foreach (var descriptor in definition.Parameters)
                    header += ";" + descriptor.Name;
                header += ";Оценка (бэктест);Оценка (форвард)";
                header += GetMetricsHeader("бэктест") + GetMetricsHeader("форвард");
                writer.WriteLine(header);

                foreach (var c in population)
                {
                    var line = $"{c.TimeFrame};{c.Side};{c.Ticker.Name}";
                    foreach (var descriptor in definition.Parameters)
                        line += ";" + c.Genes[descriptor.Name];

                    var r = c.ForwardAnalysisResults.FirstOrDefault();
                    if (r == null)
                    {
                        writer.WriteLine(line);
                        continue;
                    }

                    line += $";{r.BackwardFitness};{r.ForwardFitness}";
                    line += GetMetricsLine(r.BackwardProfit, r.BackwardProfitPrcnt,
                        r.BackwardMaxDrawDown, r.BackwardRecoveryFactor,
                        r.BackwardDealsStatistics);
                    line += GetMetricsLine(r.ForwardProfit, r.ForwardProfitPrcnt,
                        r.ForwardMaxDrawDown, r.ForwardRecoveryFactor,
                        r.ForwardDealsStatistics);
                    writer.WriteLine(line);
                }
            }, logger);
        }

        private SurogateChromosome CreateBestChromosome(string fullFileName)
        {
            var file = File.ReadAllText(fullFileName);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<SurogateChromosome>(new JsonTextReader(new StringReader(file)));                        
        }

        private void PrintToTxtFile(List<ChromosomeDonchianChannel> population, string fileName ="")
        {
            if (population == null || population.Count == 0)
                return;

            var t = population.Last();

            if (fileName == "")
                fileName= $"{t.Ticker.Name}_{t.Side}_params.csv";

            using (StreamWriter writer = new StreamWriter(fileName, append: false, encoding: CsvEncoding))
            {
                // Запись заголовков столбцов                
                writer.WriteLine($"{nameof(t.FitnessValue)};{nameof(t.DealsCount)};" +
                    $"{nameof(t.TimeFrame)};{nameof(t.Side)};{nameof(t.Ticker.Name)};" +
                    $"{nameof(t.SlowDonchian)};{nameof(t.FastDonchian)};{nameof(t.AtrPeriod)};" +
                    $"{nameof(t.LimitOpenedPositions)};{nameof(t.KAtrForOpenPosition)};" +
                    $"{nameof(t.KAtrForStopLoss)};{nameof(t.Profit)};{nameof(t.ProfitPrcnt)}");

                foreach (var c in population)
                {
                    // Запись строк с данными
                    writer.WriteLine($"{c.FitnessValue};{c.DealsCount};{c.TimeFrame};{c.Side};" +
                        $"{c.Ticker.Name};{c.SlowDonchian};{c.FastDonchian};{c.AtrPeriod};" +
                        $"{c.LimitOpenedPositions};{c.KAtrForOpenPosition};{c.KAtrForStopLoss};" +
                        $"{c.Profit};{c.ProfitPrcnt}");
                }
            }
        }

        /// <summary>
        /// Переносит показатели окон из результата лучшей хромосомы в строку отчёта.
        /// Прибыль и даты копируются рядом, а это — просадка, фактор восстановления
        /// и статистика по сделкам, по которой считается итог по всем отрезкам.
        /// </summary>
        public static void CopyWindowMetrics(ForwardAnalysisResult from,
            ForwardAnalysisResult to)
        {
            to.BackwardMaxDrawDown = from.BackwardMaxDrawDown;
            to.ForwardMaxDrawDown = from.ForwardMaxDrawDown;
            to.BackwardRecoveryFactor = from.BackwardRecoveryFactor;
            to.ForwardRecoveryFactor = from.ForwardRecoveryFactor;
            to.BackwardDealsStatistics = from.BackwardDealsStatistics;
            to.ForwardDealsStatistics = from.ForwardDealsStatistics;
        }

        private string GetMetricsHeader(string window)
        {
            return $";Сделок ({window});Прибыль, р. ({window});Прибыль, % ({window});" +
                $"Просадка, % ({window});Фактор восстановления ({window});" +
                $"Выигрышных, % ({window});Выигрышных ({window});Убыточных ({window});" +
                $"Средний выигрыш ({window});Средний проигрыш ({window});" +
                $"Выигрыш к проигрышу ({window});Профит-фактор ({window});" +
                $"Средняя сделка ({window});Лучшая сделка ({window});" +
                $"Худшая сделка ({window});Убытков подряд ({window});" +
                $"Выигрышей подряд ({window});Длительность сделки, баров ({window})";
        }

        private string GetMetricsLine(double profit, double profitPrcnt,
            double maxDrawDown, double recoveryFactor, DealsStatistics statistics)
        {
            //Окна может не быть — например, у главного прогона нет форвардной части.
            if (statistics == null)
                return string.Concat(Enumerable.Repeat(";", 18));

            return $";{statistics.DealsCount};{profit};{profitPrcnt};{maxDrawDown};" +
                $"{recoveryFactor};{statistics.WinRatePrcnt};" +
                $"{statistics.WinningDealsCount};{statistics.LosingDealsCount};" +
                $"{statistics.AverageWin};{statistics.AverageLoss};" +
                $"{statistics.PayoffRatio};{statistics.ProfitFactor};" +
                $"{statistics.ExpectedPayoff};{statistics.LargestWin};" +
                $"{statistics.LargestLoss};{statistics.MaxConsecutiveLosses};" +
                $"{statistics.MaxConsecutiveWins};{statistics.AverageBarsInDeal}";
        }

        /// <summary>
        /// Пишет отчёт, не роняя прогон. Файл может быть занят — например, открыт
        /// в Excel, чтобы посмотреть промежуточный результат. Отчёт этого не стоит:
        /// прогон идёт часами, а его данные лежат в чек-поинте и будут дописаны
        /// при следующей записи.
        /// </summary>
        private bool TryWriteReport(string fileName, Action<StreamWriter> write,
            Logger logger, bool append = false)
        {
            const int attempts = 5;

            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    using (var writer = new StreamWriter(fileName, append, CsvEncoding))
                        write(writer);
                    return true;
                }
                catch (IOException e)
                {
                    if (attempt == attempts)
                    {
                        logger.Log("Не удалось записать отчёт «{0}»: {1}\r\n" +
                            "Скорее всего, файл открыт в другой программе. Прогон " +
                            "продолжается, отчёт допишется при следующей записи.",
                            fileName, e.Message);
                        return false;
                    }

                    Thread.Sleep(200);
                }
                catch (UnauthorizedAccessException e)
                {
                    logger.Log("Нет доступа к отчёту «{0}»: {1}. Прогон продолжается.",
                        fileName, e.Message);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Переписывает сводный отчёт целиком из накопленных результатов. Именно
        /// целиком, а не дописывает строку: если запись сорвалась, следующая
        /// восстановит и пропущенное.
        /// </summary>
        public void WriteSummaryReport(List<ForwardAnalysisResult> results,
            string fileName, Logger logger)
        {
            TryWriteReport(fileName, writer =>
            {
                writer.WriteLine("BackwardFitness;ForwardFitness;" +
                    "BackwardProfit;ForwardProfit;" +
                    "BackwardProfitPrcnt;ForwardProfitPrcnt;" +
                    "BackwardTestDates;ForwardTestDates;");

                foreach (var result in results)
                    writer.WriteLine($"{result.BackwardFitness};{result.ForwardFitness};" +
                        $"{result.BackwardProfit};{result.ForwardProfit};" +
                        $"{result.BackwardProfitPrcnt};{result.ForwardProfitPrcnt};" +
                        $"{result.BackwardStart}-{result.BackwardEnd};" +
                        $"{result.ForwardStart} - {result.ForwardEnd}");
            }, logger);
        }

        private void CreateTxtFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, append: false, encoding: CsvEncoding))
            {
                writer.WriteLine($"BackwardFitness;ForwardFitness;" +
                    $"BackwardProfit;ForwardProfit;" +
                    $"BackwardProfitPrcnt;ForwardProfitPrcnt;" +
                    $"BackwardTestDates;ForwardTestDates;");
            }
        }

        private void AppendToTxtFile(ForwardAnalysisResult result, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, append: true, encoding: CsvEncoding))
            {
                writer.WriteLine($"{result.BackwardFitness};{result.ForwardFitness};" +
                    $"{result.BackwardProfit};{result.ForwardProfit};" +
                    $"{result.BackwardProfitPrcnt};{result.ForwardProfitPrcnt};" +
                    $"{result.BackwardStart}-{result.BackwardEnd};" +
                    $"{result.ForwardStart} - {result.ForwardEnd}");
            }
        }

        public Settings CreateSettings(string fullFileName)
        {
            var settings = new Settings();
            var sides = new List<PositionSide>();
            var timeFrames = new List<Interval>();

            try
            {
                if (!System.IO.File.Exists(fullFileName))
                    throw new Exception("Файл не найден!");

                string[] listStrings = System.IO.File.ReadAllLines(fullFileName);

                if (listStrings == null)
                    throw new Exception("Файл пустой!");

                foreach (var str in listStrings)
                {
                    if (str.Contains("Long"))
                        sides.Add(PositionSide.Long);
                    if (str.Contains("Short"))
                        sides.Add(PositionSide.Short);
                    if (str.Contains("01min"))
                        timeFrames.Add(new Interval(1, DataIntervals.MINUTE));
                    if (str.Contains("05min"))
                        timeFrames.Add(new Interval(5, DataIntervals.MINUTE));
                    if (str.Contains("15min"))
                        timeFrames.Add(new Interval(15, DataIntervals.MINUTE));
                    if (str.Contains("30min"))
                        timeFrames.Add(new Interval(30, DataIntervals.MINUTE));
                    if (str.Contains("60min"))
                        timeFrames.Add(new Interval(60, DataIntervals.MINUTE));
                    if (str.Contains("1d"))
                        timeFrames.Add(new Interval(1, DataIntervals.DAYS));

                    ParseSettingsKeyValue(str, settings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            settings.Sides = sides;
            settings.TimeFrames = timeFrames;
            return settings;
        }

        //Разбор строк вида «Ключ:Значение»; неизвестные ключи игнорируются.
        private void ParseSettingsKeyValue(string line, Settings settings)
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
                return;

            var key = line.Substring(0, separatorIndex).Trim();
            var value = line.Substring(separatorIndex + 1).Trim();

            try
            {
                switch (key)
                {
                    case "Strategy": settings.Strategy = value; break;
                    case "Seed": settings.Seed = int.Parse(value); break;
                    case "PopulationSize": settings.PopulationSize = int.Parse(value); break;
                    case "Generations": settings.Generations = int.Parse(value); break;
                    case "CrossoverRate": settings.CrossoverRate = ParseDouble(value); break;
                    case "MutationRate": settings.MutationRate = ParseDouble(value); break;
                    case "Patience": settings.Patience = int.Parse(value); break;
                    case "TournamentSize": settings.TournamentSize = int.Parse(value); break;
                    case "MinDiversity": settings.MinDiversity = ParseDouble(value); break;
                    case "EliteFraction": settings.EliteFraction = ParseDouble(value); break;
                    case "Threads": settings.Threads = int.Parse(value); break;
                    case "ExcludeBestDealsPrcnt":
                        settings.ExcludeBestDealsPrcnt = ParseDouble(value); break;
                    case "MinDealsCount": settings.MinDealsCount = int.Parse(value); break;
                    case "MaxDrawDownPrcnt":
                        settings.MaxDrawDownPrcnt = ParseDouble(value); break;
                    case "MinWinRatePrcnt":
                        settings.MinWinRatePrcnt = ParseDouble(value); break;
                    case "PenaltyPower": settings.PenaltyPower = ParseDouble(value); break;
                    case "NeighbourhoodPoints":
                        settings.NeighbourhoodPoints = int.Parse(value); break;
                    case "NeighbourhoodPercent":
                        settings.NeighbourhoodPercent = ParseDouble(value); break;
                    case "NeighbourhoodUseMedian":
                        settings.NeighbourhoodUseMedian = ParseBool(value); break;
                    case "SaveCheckpoint": settings.SaveCheckpoint = ParseBool(value); break;
                    case "CheckpointFile": settings.CheckpointFile = value; break;
                    case "ResultsFolder": settings.ResultsFolder = value; break;
                    case "BackwardDays": settings.BackwardDays = int.Parse(value); break;
                    case "ForwardDays": settings.ForwardDays = int.Parse(value); break;
                    case "ForwardPeriodsCount": settings.ForwardPeriodsCount = int.Parse(value); break;
                    case "ShiftWindowDays": settings.ShiftWindowDays = int.Parse(value); break;
                    case "Equity": settings.Equity = ParseDouble(value); break;
                    case "RiskValuePrcnt": settings.RiskValuePrcnt = ParseDouble(value); break;
                    case "SecuritiesFile": settings.SecuritiesFile = value; break;
                    case "SeedGenesFile": settings.SeedGenesFile = value; break;
                    case "LogFile": settings.LogFile = value; break;
                    case "TrimHistory": settings.TrimHistory = ParseBool(value); break;
                    case "Range":
                        //Формат: Range:имя:мин:макс:шаг
                        var parts = value.Split(':');
                        if (parts.Length == 4)
                            settings.ParameterRanges[parts[0].Trim()] = new ParameterRange()
                            {
                                Min = ParseDouble(parts[1]),
                                Max = ParseDouble(parts[2]),
                                Step = ParseDouble(parts[3]),
                            };
                        else
                            Console.WriteLine("Неверный формат диапазона: " + line);
                        break;
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Не удалось разобрать строку настроек: " + line);
            }
        }

        private double ParseDouble(string value)
        {
            return double.Parse(value.Replace(',', '.'),
                System.Globalization.CultureInfo.InvariantCulture);
        }

        //Логический флаг настроек: принимаем и «1/0», и «true/false».
        private bool ParseBool(string value)
        {
            var trimmed = value.Trim();
            if (trimmed == "1")
                return true;
            if (trimmed == "0")
                return false;

            return bool.Parse(trimmed);
        }

        //Файл с описанием инструментов: строки вида
        //«Имя;Валюта;Лотов;Комиссия;ТоргуетсяВUSD;КурсUSD».
        //Ошибки здесь раньше молча проглатывались, и оптимизатор падал позже и в
        //другом месте — теперь разбор строгий и сообщает, что именно не так.
        public List<SecurityData> GetSecuritiesData(string fullFileName)
        {
            var result = new List<SecurityData>();

            if (!System.IO.File.Exists(fullFileName))
                throw new Exception("Файл с описанием инструментов не найден: " +
                    fullFileName);

            var listStrings = System.IO.File.ReadAllLines(fullFileName);
            var lineNumber = 0;

            foreach (var str in listStrings)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(str))
                    continue;

                var splStr = str.Split(';');
                if (splStr.Length < 6)
                    throw new Exception(string.Format(
                        "Файл с описанием инструментов «{0}», строка {1}: ожидались шесть " +
                        "полей через «;» — Имя;Валюта;Лотов;Комиссия;ТоргуетсяВUSD;КурсUSD, " +
                        "а получено {2}. Строка: «{3}». Возможно, вместо файла с описанием " +
                        "инструментов (!Securities_*.txt) указан файл с котировками.",
                        fullFileName, lineNumber, splStr.Length, str));

                try
                {
                    result.Add(new SecurityData()
                    {
                        Name = splStr[0],
                        Currency = GetCurrency(splStr[1]),
                        Shares = double.Parse(splStr[2]),
                        CommissionRate = double.Parse(splStr[3]),
                        IsUSD = int.Parse(splStr[4]) == 1,
                        RateUSD = double.Parse(splStr[5]),
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(
                        "Файл с описанием инструментов «{0}», строка {1}: не удалось " +
                        "разобрать «{2}». {3}", fullFileName, lineNumber, str, ex.Message));
                }
            }

            if (result.Count == 0)
                throw new Exception("Файл с описанием инструментов пуст: " + fullFileName);

            return result;
        }

        private Currency GetCurrency(string currency)
        {
            if (currency == Currency.USD.ToString())
                return Currency.USD;

            if (currency == Currency.RUB.ToString())
                return Currency.RUB;

            throw new Exception("Неверное значение валюты");
        }

        private List<Ticker> CreateTickers(List<SecurityData> securitiesData, string fullFileName,
            Settings settings, Logger logger, bool trimHistory = false,
            Logger reportLogger = null)
        {
            var result = new List<Ticker>();

            foreach (var side in settings.Sides)
            {
                foreach (var timeFrame in settings.TimeFrames)
                {
                    foreach (var data in securitiesData)
                    {
                        var securityName = data.Name;
                        var fileNameSplitted = fullFileName.Split('\\');

                        var path = string.Empty;
                        for (var i = 0; i < fileNameSplitted.Length - 1; i++)
                            path += fileNameSplitted[i] + "\\";
                        var fileName = path + securityName + ".txt";

                        var ticker = CreateTicker(fileName, data, timeFrame, logger,
                            trimHistory ? settings : null, reportLogger);

                        result.Add(ticker);
                    }
                }
            }

            return result;
        }

        private Ticker CreateTicker(string fileName, SecurityData data, Interval timeframe,
            Logger logger, Settings settingsForTrimming = null, Logger reportLogger = null)
        {
            var converter = ConverterTextDataToBar.Create(fileName);
            var bars = CompressBars(ReadBars(converter, settingsForTrimming), timeframe);

            if (settingsForTrimming != null)
            {
                //Подстраховка: если слева от начала окна не осталось ни одного бара
                //(запаса не хватило на длинные праздники), читаем файл целиком —
                //иначе ForwardAnalysis сочтёт историю недостаточной.
                if (bars.Count > 0 && bars.First().Date >
                    HistoryTrimmer.GetEarliestRequiredDate(bars, settingsForTrimming))
                {
                    bars = CompressBars(converter.ConvertFileWithBarsToListOfBars(),
                        timeframe);
                }

                bars = TrimHistory(bars, settingsForTrimming, data.Name, reportLogger);
            }

            var ticker = new Ticker(data.Name, data.Currency, data.Shares, bars,
                logger, data.CommissionRate, data.IsUSD, data.RateUSD);

            return ticker;
        }

        //Читаем только ту часть файла, которая может понадобиться тестированию:
        //разбор лишних лет — самая долгая часть загрузки минутных данных.
        //Режем по границе суток, чтобы при сжатии в таймфрейм не получить неполный
        //первый бар: сетка сжатия привязана к абсолютному времени, а не к первому бару.
        private List<Bar> ReadBars(ConverterTextDataToBar converter, Settings settingsForTrimming)
        {
            if (settingsForTrimming == null)
                return converter.ConvertFileWithBarsToListOfBars();

            var lastBarDate = converter.GetLastBarDate();
            if (lastBarDate == null)
                return converter.ConvertFileWithBarsToListOfBars();

            var daysBack = HistoryTrimmer.GetRequiredDays(settingsForTrimming) - 1 +
                HistoryTrimmer.ExtraDaysToRead;
            if (daysBack >= (lastBarDate.Value - DateTime.MinValue).TotalDays)
                return converter.ConvertFileWithBarsToListOfBars();

            return converter.ConvertFileWithBarsToListOfBars(
                HistoryTrimmer.GetReadFromDate(lastBarDate.Value, settingsForTrimming));
        }

        //Обрезка истории до окон тестирования: файл котировок можно подавать
        //целиком, вручную готовить его не нужно.
        private List<Bar> TrimHistory(List<Bar> bars, Settings settings, string securityName,
            Logger reportLogger)
        {
            if (bars == null || bars.Count == 0)
                return bars;

            var trimmedBars = HistoryTrimmer.Trim(bars, settings);
            if (reportLogger != null && trimmedBars.Count > 0)
                reportLogger.Log("{0}: для тестирования нужно {1} дней — взяли {2} баров " +
                    "с {3:dd.MM.yyyy} по {4:dd.MM.yyyy}, более старую историю не читали.",
                    securityName, HistoryTrimmer.GetRequiredDays(settings),
                    trimmedBars.Count, trimmedBars.First().Date, trimmedBars.Last().Date);

            return trimmedBars;
        }

        public List<Bar> CompressBars(List<Bar> bars, Interval timeframe)
        {
            var compressor = new BarCompressor();
            var result = new List<Bar>();

            switch (timeframe.Base)
            {
                case DataIntervals.MINUTE:
                    {
                        switch (timeframe.Value)
                        {
                            case 1:
                                {
                                    result = bars;
                                    break;
                                }

                            case 5:
                                {
                                    result = compressor.To5Minute(bars);
                                    break;
                                }
                            case 15:
                                {
                                    result = compressor.To15Minute(bars);
                                    break;
                                }
                            case 30:
                                {
                                    result = compressor.To30Minute(bars);
                                    break;
                                }
                            case 60:
                                {
                                    result = compressor.ToHourly(bars);
                                    break;
                                }
                            default:
                                {
                                    throw new NotImplementedException();
                                }
                        }
                        break;
                    }
                case DataIntervals.DAYS:
                    {
                        switch (timeframe.Value)
                        {
                            case 1:
                                {
                                    result = compressor.ToDaily(bars);
                                    break;
                                }
                            default:
                                {
                                    throw new NotImplementedException();
                                }
                        }
                        break;
                    }
            }

            return result;
        }        
    }
}