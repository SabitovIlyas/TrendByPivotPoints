using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly int patience;
        private readonly int tournamentSize;
        private readonly double minNormalizedDiversity;
        private readonly double epsilon = 1e-5;

        private Dictionary<string, double> seedGenes;
        private ForwardAnalysis forwardAnalysis;

        private readonly Dictionary<string, ChromosomeUniversal> chromosomeCache =
            new Dictionary<string, ChromosomeUniversal>();

        public GeneticAlgorithmUniversal(int populationSize, int generations,
            double crossoverRate, double mutationRate, IRandomProvider randomProvider,
            List<Ticker> tickers, Settings settings, Context context,
            StrategyDefinition definition, Logger logger)
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
            patience = settings.Patience;
            tournamentSize = settings.TournamentSize;
            minNormalizedDiversity = settings.MinDiversity;
        }

        public List<ChromosomeUniversal> Run(int period,
            Dictionary<string, double> seedGenes = null)
        {
            this.seedGenes = seedGenes;

            var generationsWithoutImprovement = 0;
            var bestFitnessEver = double.MinValue;
            var isGenerationsWithoutImprovement = false;
            var isNormalizedDiversityBreaks = false;
            int gen = 0;

            chromosomeCache.Clear();
            Initialize();

            for (; gen < generations; gen++)
            {
                Evaluate(period);
                var newPopulation = new List<ChromosomeUniversal>();
                var qtyBestChromosomes = Math.Min(20, populationSize);

                //Элитизм: лучшие хромосомы переходят в следующее поколение.
                var best = SelectBestChromosomes(population, qtyBestChromosomes);
                newPopulation.AddRange(best);

                var currentBest = newPopulation.First().FitnessValue;
                if (currentBest > bestFitnessEver + epsilon)
                {
                    bestFitnessEver = currentBest;
                    generationsWithoutImprovement = 0;
                    Console.WriteLine($"Поколение {gen + 1}: Новый рекорд = {currentBest}");
                }
                else
                {
                    generationsWithoutImprovement++;
                    Console.WriteLine($"Поколение {gen + 1}:");
                }

                if (generationsWithoutImprovement >= patience)
                {
                    isGenerationsWithoutImprovement = true;
                    break;
                }

                var diversity = CalculatePopulationDiversity(population);
                if (diversity < minNormalizedDiversity)
                {
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
            }

            if (isGenerationsWithoutImprovement)
                Console.WriteLine($"Остановка: {patience} поколений без улучшения.");
            else if (isNormalizedDiversityBreaks)
                Console.WriteLine("Остановка: разнообразие популяции упало ниже порога.");
            else
                Evaluate(period);

            return population.Where(c => c.FitnessPassed)
                .OrderByDescending(c => c.FitnessValue).Take(1).ToList();
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
            Console.WriteLine($"Разнообразие после инициализации: {diversity:P1}");
        }

        public void Evaluate(int period)
        {
            var chromosomesForRemove = new List<ChromosomeUniversal>();
            var chromosomesForAdd = new List<ChromosomeUniversal>();

            var i = 0;
            foreach (var chromosome in population)
            {
                Console.WriteLine("\r\nХромосома №{0} из {1}.\r\n", ++i, population.Count);
                if (!chromosome.FitnessValue.Equals(double.NaN))
                    continue;

                var key = chromosome.Name;
                if (chromosomeCache.TryGetValue(key, out ChromosomeUniversal cached))
                {
                    Console.WriteLine("Взяли хромосому из кэша");
                    chromosomesForRemove.Add(chromosome);
                    chromosomesForAdd.Add(cached);
                }
                else
                {
                    var starter = CreateStarterForChromosome(chromosome);
                    var parameters = definition.CreateSystemParameters(chromosome.Genes,
                        chromosome.Ticker, chromosome.Side, chromosome.TimeFrame, settings);

                    if (IsLastBackwardTesting)
                        PrepareChromosomeFinal(chromosome, period);
                    else
                        PrepareChromosome(chromosome, period);
                    chromosome.SetBackwardBarsAsTickerBars();

                    var fitness = new FitnessUniversal(parameters, chromosome, starter);
                    fitness.SetUpChromosomeFitnessValue();
                    chromosomeCache[key] = chromosome;
                }
            }

            foreach (var chromosome in chromosomesForRemove)
                population.Remove(chromosome);
            foreach (var chromosome in chromosomesForAdd)
                population.Add(chromosome);

            i = 0;
            foreach (var chromosome in population)
            {
                Console.WriteLine("Расчёт фитнес-функции для {0} хромосомы из {1}." +
                    "\r\n\r\nХромосома: {2}", ++i, population.Count, chromosome.Name);
                Console.WriteLine("Фитнес-функция = {0}. Количество сделок = {1}. " +
                    "Прибыль, р. = {2}. Прибыль, % = {3}. Максимальная просадка, % = {4}. " +
                    "Фактор восстановления = {5}\r\n", chromosome.FitnessValue,
                    chromosome.DealsCount, chromosome.Profit, chromosome.ProfitPrcnt,
                    chromosome.MaxDrawDown, chromosome.RecoveryFactor);
            }
        }

        private Starter CreateStarterForChromosome(ChromosomeUniversal chromosome)
        {
            var ticker = chromosome.Ticker;
            var security = new SecurityLab(ticker.Name, ticker.Currency, ticker.Shares,
                ticker.Bars, ticker.Logger, ticker.CommissionRate);
            security.RateUSD = ticker.RateUSD;

            return definition.CreateStarter(context, new List<Security>() { security }, logger);
        }

        private void PrepareChromosome(ChromosomeUniversal chromosome, int period)
        {
            forwardAnalysis = new ForwardAnalysis(genAlg: null,
                forwardPeriodDays: settings.ForwardDays,
                backwardPeriodDays: settings.BackwardDays,
                forwardPeriodsCount: settings.ForwardPeriodsCount,
                shiftWindowDays: settings.ShiftWindowDays);

            forwardAnalysis.Period = period;
            chromosome.ResetBarsToInitBars();
            forwardAnalysis.SetTradingPeriods(chromosome);
        }

        private void PrepareChromosomeFinal(ChromosomeUniversal chromosome, int period)
        {
            forwardAnalysis = new ForwardAnalysis(genAlg: null,
                forwardPeriodDays: 0,
                backwardPeriodDays: settings.BackwardDays,
                forwardPeriodsCount: 1,
                shiftWindowDays: settings.ShiftWindowDays);

            forwardAnalysis.Period = period;
            chromosome.ResetBarsToInitBars();
            forwardAnalysis.SetTradingPeriodsFinal(chromosome);
        }

        public ChromosomeUniversal TournamentSelection()
        {
            ChromosomeUniversal best = null;
            for (int i = 0; i < tournamentSize; i++)
            {
                var candidate = population[randomProvider.Next(population.Count)];
                if (best == null || (candidate.FitnessPassed &&
                    candidate.FitnessValue > best.FitnessValue))
                {
                    best = candidate;
                }
            }
            return best;
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
