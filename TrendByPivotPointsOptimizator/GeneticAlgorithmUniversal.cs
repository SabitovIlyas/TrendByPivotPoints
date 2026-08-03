using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using TradingSystems;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Универсальный генетический алгоритм: работает с любой стратегией через
    /// StrategyDefinition (набор дескрипторов параметров + фабрика стартера).
    /// Устройство повторяет GeneticAlgorithmDonchianChannel: элитизм, турнирный отбор,
    /// равномерный кроссовер, гауссова мутация, ранние остановки по терпению и
    /// разнообразию популяции.
    /// </summary>
    public class GeneticAlgorithmUniversal
    {
        public bool IsLastBackwardTesting = false;
        public List<ChromosomeUniversal> GetPopulation() => population;

        /// <summary>
        /// Вызывается после каждого завершённого поколения — по этому событию
        /// оптимизатор сохраняет чек-поинт.
        /// </summary>
        public Action<GaProgress> GenerationCompleted { get; set; }

        private List<ChromosomeUniversal> population;
        private readonly int populationSize;
        private readonly int generations;
        private readonly double crossoverRate;
        private readonly double mutationRate;
        private readonly IRandomProvider randomProvider;
        private readonly List<Ticker> tickers;
        private readonly Settings settings;
        private readonly Context context;
        private readonly StrategyDefinition definition;
        private readonly Logger logger;
        private readonly Logger runLogger;

        private readonly int patience;
        private readonly int tournamentSize;
        private readonly double minNormalizedDiversity;
        private readonly double eliteFraction;
        private readonly double epsilon = 1e-5;

        private Dictionary<string, double> seedGenes;

        //Окна тестирования на поколение: инструмент -> нарезка баров.
        private Dictionary<string, ForwardAnalysisResult> windows;
        private int windowsPeriod = -1;
        private bool windowsAreFinal;

        //Кэш результатов по комбинации генов: только числа, без баров и стартеров.
        private readonly Dictionary<string, ChromosomeMetrics> chromosomeCache =
            new Dictionary<string, ChromosomeMetrics>();

        //Оценки отдельных точек пространства параметров — центров и соседей.
        //Пишется из нескольких потоков сразу, поэтому потокобезопасный словарь.
        private readonly ConcurrentDictionary<string, double> pointFitnessCache =
            new ConcurrentDictionary<string, double>();

        private readonly NeighbourhoodBuilder neighbourhoodBuilder;

        /// <param name="logger">Журнал торговой системы: его стратегия глушит на
        /// исторических барах, поэтому ход оптимизации в него писать нельзя.</param>
        /// <param name="runLogger">Журнал прогона (консоль + файл). Если не задан,
        /// пишем только в консоль, как раньше.</param>
        public GeneticAlgorithmUniversal(int populationSize, int generations,
            double crossoverRate, double mutationRate, IRandomProvider randomProvider,
            List<Ticker> tickers, Settings settings, Context context,
            StrategyDefinition definition, Logger logger, Logger runLogger = null)
        {
            this.populationSize = populationSize;
            this.generations = generations;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;
            this.randomProvider = randomProvider;
            this.tickers = tickers;
            this.settings = settings;
            this.context = context;
            this.definition = definition;
            this.logger = logger;
            this.runLogger = runLogger ?? new ConsoleLogger();
            patience = settings.Patience;
            tournamentSize = settings.TournamentSize;
            minNormalizedDiversity = settings.MinDiversity;
            eliteFraction = settings.EliteFraction;

            if (settings.NeighbourhoodPoints > 0)
                neighbourhoodBuilder = new NeighbourhoodBuilder(definition.Parameters,
                    settings.NeighbourhoodPoints, settings.NeighbourhoodPercent,
                    settings.Seed ?? 0);
        }

        /// <param name="resumeFrom">Состояние прерванного прогона: популяция и счётчики
        /// последнего завершённого поколения. Если задано — стартовая популяция не
        /// создаётся, прогон продолжается со следующего поколения.</param>
        public List<ChromosomeUniversal> Run(int period,
            Dictionary<string, double> seedGenes = null, GaProgress resumeFrom = null)
        {
            this.seedGenes = seedGenes;

            var generationsWithoutImprovement = 0;
            var bestFitnessEver = double.MinValue;
            var isGenerationsWithoutImprovement = false;
            var isNormalizedDiversityBreaks = false;
            var lastDiversity = 0d;
            int gen = 0;

            chromosomeCache.Clear();
            pointFitnessCache.Clear();

            if (resumeFrom != null)
            {
                population = resumeFrom.Population;
                gen = resumeFrom.Generation;
                bestFitnessEver = resumeFrom.BestFitnessEver;
                generationsWithoutImprovement = resumeFrom.GenerationsWithoutImprovement;
                runLogger.Log($"Продолжаем прогон с поколения {gen + 1}: рекорд " +
                    $"{bestFitnessEver}, поколений без улучшения " +
                    $"{generationsWithoutImprovement} из {patience}.");
            }
            else
                Initialize();

            for (; gen < generations; gen++)
            {
                Evaluate(period);
                var newPopulation = new List<ChromosomeUniversal>();
                var qtyBestChromosomes = GetEliteCount();

                //Элитизм: лучшие хромосомы переходят в следующее поколение.
                var best = SelectBestChromosomes(population, qtyBestChromosomes);
                newPopulation.AddRange(best);

                var diversity = CalculatePopulationDiversity(population);
                var currentBest = newPopulation.First().FitnessValue;
                if (currentBest > bestFitnessEver + epsilon)
                {
                    bestFitnessEver = currentBest;
                    generationsWithoutImprovement = 0;
                    runLogger.Log($"Поколение {gen + 1}: новый рекорд = {currentBest}. " +
                        DescribeConvergence(diversity, generationsWithoutImprovement));
                }
                else
                {
                    generationsWithoutImprovement++;
                    runLogger.Log($"Поколение {gen + 1}: рекорд прежний = {bestFitnessEver}. " +
                        DescribeConvergence(diversity, generationsWithoutImprovement));
                }

                if (generationsWithoutImprovement >= patience)
                {
                    isGenerationsWithoutImprovement = true;
                    break;
                }

                if (diversity < minNormalizedDiversity)
                {
                    lastDiversity = diversity;
                    isNormalizedDiversityBreaks = true;
                    break;
                }

                while (newPopulation.Count < populationSize)
                {
                    var parent1 = TournamentSelection();
                    var parent2 = TournamentSelection();

                    ChromosomeUniversal child;
                    if (randomProvider.NextDouble() < crossoverRate)
                        child = Crossover(parent1, parent2);
                    else
                        child = Clone(parent1);

                    Mutate(child);
                    newPopulation.Add(child);
                }

                population = newPopulation;

                GenerationCompleted?.Invoke(new GaProgress()
                {
                    Generation = gen + 1,
                    BestFitnessEver = bestFitnessEver,
                    GenerationsWithoutImprovement = generationsWithoutImprovement,
                    Population = population,
                });
            }

            if (isGenerationsWithoutImprovement)
                runLogger.Log($"Остановка на поколении {gen + 1}: " +
                    $"{patience} поколений без улучшения.");
            else if (isNormalizedDiversityBreaks)
                runLogger.Log($"Остановка на поколении {gen + 1}: разнообразие популяции " +
                    $"{lastDiversity:P1} упало ниже порога {minNormalizedDiversity:P1}.");
            else
            {
                runLogger.Log($"Пройдены все {generations} поколений.");
                Evaluate(period);
            }

            var result = population.Where(c => c.FitnessPassed)
                .OrderByDescending(c => c.FitnessValue).Take(1).ToList();

            if (result.Count == 0)
                throw new Exception("Ни одна хромосома не прошла отбор: все " +
                    "отбракованы. Похоже, стратегия не совершает сделок ни при " +
                    "каких значениях параметров из заданных диапазонов.");

            //Возвращаемым хромосомам нужен живой прогон: по нему оптимизатор
            //считает форвардный тест и пишет отчёт.
            foreach (var chromosome in result)
                RestoreHeavyData(chromosome, period);

            return result;
        }

        public void Initialize()
        {
            population = new List<ChromosomeUniversal>();
            for (int i = 0; i < populationSize; i++)
            {
                var ticker = tickers[randomProvider.Next(tickers.Count)];
                var timeFrames = settings.TimeFrames;
                var timeFrame = timeFrames[randomProvider.Next(timeFrames.Count)];
                var sides = settings.Sides;
                var side = sides[randomProvider.Next(sides.Count)];

                var genes = new Dictionary<string, double>();
                foreach (var descriptor in definition.Parameters)
                {
                    if (seedGenes != null && seedGenes.ContainsKey(descriptor.Name))
                        genes[descriptor.Name] = descriptor.Snap(seedGenes[descriptor.Name]);
                    else
                        genes[descriptor.Name] = descriptor.RandomValue(randomProvider);
                }
                definition.Repair(genes);

                //Затравка применяется только к первой особи, остальные — случайные.
                seedGenes = null;

                var chromosome = new ChromosomeUniversal(ticker, timeFrame, side, genes);
                population.Add(chromosome);
            }

            var diversity = CalculatePopulationDiversity(population);
            runLogger.Log($"Разнообразие после инициализации: {diversity:P1} " +
                $"(порог остановки {minNormalizedDiversity:P1}).");
        }

        public void Evaluate(int period)
        {
            PrepareWindows(period);

            //Разбор популяции идёт последовательно: так порядок обращений к кэшу
            //и содержимое журнала не зависят от того, сколько потоков считает.
            var toEvaluate = new List<ChromosomeUniversal>();
            var twinsByName = new Dictionary<string, List<ChromosomeUniversal>>();
            var elite = 0;
            var fromCache = 0;

            foreach (var chromosome in population)
            {
                if (!double.IsNaN(chromosome.FitnessValue))
                {
                    //Элита: перешла из прошлого поколения с готовым результатом.
                    elite++;
                    continue;
                }

                SetWindow(chromosome);
                var key = chromosome.Name;

                if (chromosomeCache.TryGetValue(key, out ChromosomeMetrics cached))
                {
                    cached.ApplyTo(chromosome);
                    fromCache++;
                    continue;
                }

                //Одинаковые наборы генов внутри поколения считаем один раз.
                if (twinsByName.TryGetValue(key, out List<ChromosomeUniversal> twins))
                {
                    twins.Add(chromosome);
                    fromCache++;
                    continue;
                }

                twinsByName[key] = new List<ChromosomeUniversal>();
                toEvaluate.Add(chromosome);
            }

            LogPopulationBreakdown(toEvaluate.Count, elite, fromCache);
            EvaluateInParallel(toEvaluate, period);

            foreach (var chromosome in toEvaluate)
            {
                var metrics = ChromosomeMetrics.From(chromosome);
                chromosomeCache[chromosome.Name] = metrics;

                foreach (var twin in twinsByName[chromosome.Name])
                    metrics.ApplyTo(twin);

                //Числа посчитаны и лежат в кэше — бары, сделки и стартер больше
                //не нужны. Без этого прогон удерживает историю каждой хромосомы
                //и съедает десятки гигабайт.
                ReleaseHeavyData(chromosome);
            }

            //Подробности пишем только по хромосомам, посчитанным в этом поколении:
            //у элиты и попаданий в кэш числа те же, что и раньше, и повторять их
            //каждое поколение — это десятки тысяч лишних строк за прогон.
            var i = 0;
            foreach (var chromosome in toEvaluate)
            {
                runLogger.Log("Хромосома {0} из {1}: {2}", ++i, toEvaluate.Count,
                    chromosome.Name);
                runLogger.Log("Фитнес-функция = {0}. Количество сделок = {1}. " +
                    "Прибыль, р. = {2}. Прибыль, % = {3}. Максимальная просадка, % = {4}. " +
                    "Фактор восстановления = {5}\r\n", chromosome.FitnessValue,
                    chromosome.DealsCount, chromosome.Profit, chromosome.ProfitPrcnt,
                    chromosome.MaxDrawDown, chromosome.RecoveryFactor);
            }
        }

        /// <summary>
        /// Считает хромосомы поколения в несколько потоков. Прогоны независимы:
        /// у каждого свои стартер, бумага и фитнес-функция, бары окна общие и только
        /// на чтение, а генератор случайных чисел здесь не используется — поэтому
        /// результат не зависит ни от количества потоков, ни от порядка расчёта.
        /// </summary>
        private void EvaluateInParallel(List<ChromosomeUniversal> chromosomes, int period)
        {
            if (chromosomes.Count == 0)
                return;

            var threads = GetThreadsCount(chromosomes.Count);
            runLogger.Log("Потоков: {0}.", threads);

            if (threads <= 1)
            {
                var number = 0;
                foreach (var chromosome in chromosomes)
                {
                    EvaluateChromosome(chromosome, period);
                    LogEvaluated(++number, chromosomes.Count);
                }
                return;
            }

            var counter = 0;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = threads };

            try
            {
                Parallel.ForEach(chromosomes, options, chromosome =>
                {
                    EvaluateChromosome(chromosome, period);
                    LogEvaluated(Interlocked.Increment(ref counter), chromosomes.Count);
                });
            }
            catch (AggregateException e)
            {
                //Иначе настоящая ошибка тонет внутри AggregateException, и в журнале
                //вместо внятного сообщения оказывается «произошла одна или несколько
                //ошибок».
                ExceptionDispatchInfo.Capture(e.Flatten().InnerExceptions.First()).Throw();
            }
        }

        /// <summary>
        /// Раскладка популяции перед расчётом: сколько особей надо посчитать, а
        /// сколько уже с готовым результатом. Без неё непонятно, почему счётчик
        /// расчётов меньше размера популяции.
        /// </summary>
        private void LogPopulationBreakdown(int toEvaluate, int elite, int fromCache)
        {
            runLogger.Log("Популяция {0}: считаем {1}, в элите {2}, взяли из кэша {3}.",
                population.Count, toEvaluate, elite, fromCache);
        }

        private void LogEvaluated(int number, int total)
        {
            //Живой отсчёт по ходу поколения: одно поколение боевого прогона считается
            //минутами, и без него непонятно, идёт работа или нет.
            runLogger.Log("Посчитана хромосома {0} из {1}.", number, total);
        }

        /// <summary>
        /// Сколько потоков пустить на поколение: из настроек, а 0 — по числу ядер.
        /// Больше, чем хромосом, не нужно.
        /// </summary>
        public int GetThreadsCount(int chromosomesCount)
        {
            var threads = settings.Threads > 0
                ? settings.Threads : Environment.ProcessorCount;
            return Math.Max(1, Math.Min(threads, chromosomesCount));
        }

        /// <summary>
        /// Полный прогон стратегии для хромосомы: свой стартер, своя бумага и своя
        /// фитнес-функция на барах окна бэктеста.
        /// </summary>
        private void EvaluateChromosome(ChromosomeUniversal chromosome, int period)
        {
            SetWindow(chromosome);
            var backwardBars = chromosome.ForwardAnalysisResults.First().BackwardBars;

            var starter = CreateStarterForChromosome(chromosome, backwardBars);
            var parameters = definition.CreateSystemParameters(chromosome.Genes,
                chromosome.Ticker, chromosome.Side, chromosome.TimeFrame, settings);

            var fitness = CreateFitness(parameters, chromosome, starter, backwardBars);
            fitness.SetUpChromosomeFitnessValue();

            //Оценка самой точки — она же может оказаться соседом другой хромосомы.
            var centreFitness = chromosome.FitnessValue;
            pointFitnessCache[chromosome.Name] = centreFitness;

            //Показатели по сделкам остаются от самой хромосомы, усредняется только
            //оценка: отчёт должен описывать её саму, а не размазанную окрестность.
            chromosome.FitnessValue = CalculateNeighbourhoodFitness(chromosome,
                centreFitness, backwardBars);
        }

        /// <summary>
        /// Усредняет оценку по окрестности хромосомы. Так побеждает не одиночный пик,
        /// который развалится от сдвига параметра на шаг, а плато, где рядом стоящие
        /// наборы параметров работают тоже.
        /// </summary>
        private double CalculateNeighbourhoodFitness(ChromosomeUniversal chromosome,
            double centreFitness, List<Bar> backwardBars)
        {
            if (neighbourhoodBuilder == null || double.IsNegativeInfinity(centreFitness))
                return centreFitness;

            var neighbours = neighbourhoodBuilder.Build(chromosome.Name, chromosome.Genes);
            if (neighbours.Count == 0)
                return centreFitness;

            var values = new List<double>() { centreFitness };
            foreach (var genes in neighbours)
                values.Add(EvaluateNeighbour(chromosome, genes, backwardBars));

            return Aggregate(values);
        }

        private double EvaluateNeighbour(ChromosomeUniversal chromosome,
            Dictionary<string, double> genes, List<Bar> backwardBars)
        {
            definition.Repair(genes);

            //Кэш оценок точек — отдельный от кэша хромосом: там лежит уже усреднённая
            //по окрестности оценка, а соседу нужна оценка самой точки.
            var key = chromosome.GetNameForGenes(genes);
            if (pointFitnessCache.TryGetValue(key, out double cached))
                return cached;

            var starter = CreateStarterForChromosome(chromosome, backwardBars);
            var parameters = definition.CreateSystemParameters(genes, chromosome.Ticker,
                chromosome.Side, chromosome.TimeFrame, settings);

            //Хромосому не передаём: результат соседа идёт только в усреднение.
            var fitness = CreateFitness(parameters, null, starter, backwardBars);
            var value = fitness.CalculateFitnessValue();

            pointFitnessCache[key] = value;
            return value;
        }

        private double Aggregate(List<double> values)
        {
            if (!settings.NeighbourhoodUseMedian)
                return Math.Round(values.Average(), 2);

            var sorted = values.OrderBy(v => v).ToList();
            var middle = sorted.Count / 2;
            var median = sorted.Count % 2 == 1
                ? sorted[middle]
                : (sorted[middle - 1] + sorted[middle]) / 2;

            return Math.Round(median, 2);
        }

        private FitnessUniversal CreateFitness(SystemParameters parameters,
            ChromosomeUniversal chromosome, Starter starter, List<Bar> bars)
        {
            return new FitnessUniversal(parameters, chromosome, starter, bars)
            {
                PrcntDealForExclude = settings.ExcludeBestDealsPrcnt,
                MinDealsCount = settings.MinDealsCount,
                MaxDrawDownPrcnt = settings.MaxDrawDownPrcnt,
                MinWinRatePrcnt = settings.MinWinRatePrcnt,
                PenaltyPower = settings.PenaltyPower,
            };
        }

        /// <summary>
        /// Отпускает всё тяжёлое, что осталось от прогона хромосомы: списки баров
        /// окна и фитнес-функцию со стартером, сделками и рядами индикаторов.
        /// Числовой результат к этому моменту уже лежит в кэше и в самой хромосоме.
        /// </summary>
        private void ReleaseHeavyData(ChromosomeUniversal chromosome)
        {
            chromosome.Fitness = null;
            foreach (var result in chromosome.ForwardAnalysisResults)
            {
                result.BackwardBars = null;
                result.ForwardBars = null;
            }
        }

        /// <summary>
        /// Возвращает хромосоме то, что было отпущено после расчёта: форвардный тест
        /// в оптимизаторе гоняет стратегию заново через chromosome.Fitness на барах
        /// форвардного окна. Прогон здесь один — на итоговых хромосомах, поэтому
        /// на время это не влияет.
        /// </summary>
        private void RestoreHeavyData(ChromosomeUniversal chromosome, int period)
        {
            if (chromosome.Fitness != null)
                return;

            PrepareWindows(period);
            EvaluateChromosome(chromosome, period);
        }

        private Starter CreateStarterForChromosome(ChromosomeUniversal chromosome,
            List<Bar> bars)
        {
            var ticker = chromosome.Ticker;
            var security = new SecurityLab(ticker.Name, ticker.Currency, ticker.Shares,
                bars, ticker.Logger, ticker.CommissionRate);
            security.RateUSD = ticker.RateUSD;

            return definition.CreateStarter(context, new List<Security>() { security }, logger);
        }

        /// <summary>
        /// Нарезает окна тестирования — по одному разу на инструмент за поколение.
        /// Окно зависит только от баров инструмента и номера периода, поэтому всем
        /// хромосомам поколения достаются одни и те же списки баров (только на
        /// чтение). Раньше нарезка считалась заново для каждой хромосомы: сортировка
        /// десятков тысяч баров десятки тысяч раз за прогон.
        /// </summary>
        private void PrepareWindows(int period)
        {
            if (windows != null && windowsPeriod == period &&
                windowsAreFinal == IsLastBackwardTesting)
                return;

            var forwardAnalysis = new ForwardAnalysis(genAlg: null,
                forwardPeriodDays: IsLastBackwardTesting ? 0 : settings.ForwardDays,
                backwardPeriodDays: settings.BackwardDays,
                forwardPeriodsCount: IsLastBackwardTesting ? 1 : settings.ForwardPeriodsCount,
                shiftWindowDays: settings.ShiftWindowDays);
            forwardAnalysis.Period = period;

            windows = new Dictionary<string, ForwardAnalysisResult>();
            foreach (var ticker in tickers)
                windows[ticker.Name] = IsLastBackwardTesting
                    ? forwardAnalysis.CreateTradingPeriodFinal(ticker.InitBars)
                    : forwardAnalysis.CreateTradingPeriod(ticker.InitBars);

            windowsPeriod = period;
            windowsAreFinal = IsLastBackwardTesting;
        }

        /// <summary>
        /// Выдаёт хромосоме окно её инструмента: свой объект с датами и результатами,
        /// но общие списки баров. Окно у хромосомы одно — при повторном расчёте
        /// старое заменяется, а не добавляется второе.
        /// </summary>
        private void SetWindow(ChromosomeUniversal chromosome)
        {
            var window = windows[chromosome.Ticker.Name];

            chromosome.ForwardAnalysisResults.Clear();
            chromosome.ForwardAnalysisResults.Add(new ForwardAnalysisResult()
            {
                BackwardStart = window.BackwardStart,
                BackwardEnd = window.BackwardEnd,
                ForwardStart = window.ForwardStart,
                ForwardEnd = window.ForwardEnd,
                BackwardBars = window.BackwardBars,
                ForwardBars = window.ForwardBars,
            });
        }

        public ChromosomeUniversal TournamentSelection()
        {
            ChromosomeUniversal best = null;
            for (int i = 0; i < tournamentSize; i++)
            {
                var candidate = population[randomProvider.Next(population.Count)];
                if (best == null || IsBetter(candidate, best))
                    best = candidate;
            }
            return best;
        }

        /// <summary>
        /// Сравнение участников турнира. «Не число» приходится разбирать отдельно:
        /// любое сравнение с ним ложно, поэтому непосчитанная хромосома, вытянутая
        /// первой, выигрывала турнир у любой нормальной и уходила в потомство.
        /// </summary>
        public static bool IsBetter(ChromosomeUniversal candidate, ChromosomeUniversal best)
        {
            if (double.IsNaN(best.FitnessValue))
                return !double.IsNaN(candidate.FitnessValue);

            if (double.IsNaN(candidate.FitnessValue))
                return false;

            return candidate.FitnessValue > best.FitnessValue;
        }

        public ChromosomeUniversal Crossover(ChromosomeUniversal parent1,
            ChromosomeUniversal parent2)
        {
            var ticker = randomProvider.Next(2) == 0 ? parent1.Ticker : parent2.Ticker;
            var timeFrame = randomProvider.Next(2) == 0 ? parent1.TimeFrame : parent2.TimeFrame;
            var side = randomProvider.Next(2) == 0 ? parent1.Side : parent2.Side;

            var genes = new Dictionary<string, double>();
            foreach (var descriptor in definition.Parameters)
                genes[descriptor.Name] = randomProvider.Next(2) == 0
                    ? parent1.Genes[descriptor.Name]
                    : parent2.Genes[descriptor.Name];

            definition.Repair(genes);
            return new ChromosomeUniversal(ticker, timeFrame, side, genes);
        }

        private ChromosomeUniversal Clone(ChromosomeUniversal chromosome)
        {
            var genes = new Dictionary<string, double>(chromosome.Genes);
            return new ChromosomeUniversal(chromosome.Ticker, chromosome.TimeFrame,
                chromosome.Side, genes);
        }

        public void Mutate(ChromosomeUniversal chromosome)
        {
            if (randomProvider.NextDouble() < mutationRate)
                chromosome.Ticker = tickers[randomProvider.Next(tickers.Count)];

            if (randomProvider.NextDouble() < mutationRate)
            {
                var timeFrames = settings.TimeFrames;
                chromosome.TimeFrame = timeFrames[randomProvider.Next(timeFrames.Count)];
            }

            if (randomProvider.NextDouble() < mutationRate)
            {
                var sides = settings.Sides;
                chromosome.Side = sides[randomProvider.Next(sides.Count)];
            }

            foreach (var descriptor in definition.Parameters)
            {
                if (randomProvider.NextDouble() >= mutationRate)
                    continue;

                //Для параметров с маленькой сеткой (флаги, коэффициенты с крупным
                //шагом) — равномерный случайный узел; для остальных — гауссова
                //мутация ±20% с прищёлкиванием к сетке.
                if (descriptor.StepsCount <= 12)
                    chromosome.Genes[descriptor.Name] = descriptor.RandomValue(randomProvider);
                else
                    chromosome.Genes[descriptor.Name] = descriptor.Snap(
                        GaussianMutate(chromosome.Genes[descriptor.Name]));
            }

            definition.Repair(chromosome.Genes);
            chromosome.UpdateName();
        }

        private double GaussianMutate(double value)
        {
            //±20% от текущего значения, σ = 0.2 (Box-Muller)
            double sigma = 0.2;
            double u = randomProvider.NextDouble();
            double v = randomProvider.NextDouble();
            if (u <= double.Epsilon)
                u = 1e-12;
            double z = Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v);
            return value * (1 + sigma * z);
        }

        private List<ChromosomeUniversal> SelectBestChromosomes(
            List<ChromosomeUniversal> population, int count)
        {
            return population.OrderByDescending(c => c.FitnessValue).Take(count).ToList();
        }

        /// <summary>
        /// Сколько лучших особей переходят в следующее поколение без изменений.
        /// Считается долей от популяции, но не меньше одной особи и не больше всей
        /// популяции — иначе новым потомкам не осталось бы места.
        /// </summary>
        public int GetEliteCount()
        {
            return GetEliteCount(populationSize, eliteFraction);
        }

        public static int GetEliteCount(Settings settings)
        {
            return GetEliteCount(settings.PopulationSize, settings.EliteFraction);
        }

        public static int GetEliteCount(int populationSize, double eliteFraction)
        {
            var count = (int)Math.Round(populationSize * eliteFraction,
                MidpointRounding.AwayFromZero);
            return Math.Max(1, Math.Min(populationSize, count));
        }

        /// <summary>
        /// Строка о том, насколько мы близко к сходимости: текущее разнообразие
        /// популяции против порога остановки и счётчик поколений без улучшения
        /// против терпения. Прогон закончится, когда сработает любое из двух.
        /// </summary>
        private string DescribeConvergence(double diversity, int generationsWithoutImprovement)
        {
            return $"Разнообразие = {diversity:P1} (порог остановки {minNormalizedDiversity:P1}). " +
                $"Поколений без улучшения: {generationsWithoutImprovement} из {patience}.";
        }

        public double CalculatePopulationDiversity(List<ChromosomeUniversal> population)
        {
            if (population.Count < 2)
                return 0.0;

            var descriptors = definition.Parameters;
            double totalDiversity = 0;
            int pairsCount = 0;

            for (int i = 0; i < population.Count; i++)
            {
                for (int j = i + 1; j < population.Count; j++)
                {
                    double pairDiversity = 0;
                    foreach (var descriptor in descriptors)
                    {
                        if (descriptor.Range <= 0)
                            continue;
                        pairDiversity += Math.Abs(population[i].Genes[descriptor.Name] -
                            population[j].Genes[descriptor.Name]) / descriptor.Range;
                    }
                    totalDiversity += pairDiversity / descriptors.Count;
                    pairsCount++;
                }
            }

            return totalDiversity / pairsCount;
        }
    }
}
