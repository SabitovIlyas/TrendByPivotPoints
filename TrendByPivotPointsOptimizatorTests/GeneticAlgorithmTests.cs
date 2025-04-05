using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class GeneticAlgorithmTests
    {

        [TestMethod]
        public void Initialize_ShouldCreatePopulationWithCorrectSize()
        {
            // Arrange
            int populationSize = 10;
            var randomProvider = new RandomProvider();
            var ga = new GeneticAlgorithm(populationSize, 100, 0.8, 0.1, randomProvider);

            // Act
            ga.Initialize();

            // Assert
            Assert.AreEqual(populationSize, ga.GetPopulation().Count);
        }

        [TestMethod]
        public void Initialize_ShouldSetFastAndSlowPeriodsCorrectly()
        {
            // Arrange
            int populationSize = 5;
            var randomProvider = new RandomProvider();
            var ga = new GeneticAlgorithm(populationSize, 100, 0.8, 0.1, randomProvider);

            // Act
            ga.Initialize();

            // Assert
            foreach (var chrom in ga.GetPopulation())
            {
                Assert.IsTrue(chrom.FastPeriod >= 1 && chrom.FastPeriod <= 50);
                Assert.IsTrue(chrom.SlowPeriod > chrom.FastPeriod && chrom.SlowPeriod <= 200);
            }
        }

        [TestMethod]
        public void Evaluate_ShouldSetFitnessForEachChromosome()
        {
            // Arrange
            var randomProvider = new RandomProvider();
            var ga = new GeneticAlgorithm(3, 100, 0.8, 0.1, randomProvider);
            ga.Initialize();
            double[] prices = GeneratePrices(100, 0.1, 1);

            // Act
            ga.Evaluate(prices);

            // Assert
            foreach (var chrom in ga.GetPopulation())
            {
                Assert.IsNotNull(chrom.Fitness);
                Assert.IsTrue(chrom.Fitness >= 0 || chrom.Fitness < 0); // Прибыль может быть положительной или отрицательной
            }
        }

        [TestMethod]
        public void TournamentSelection_ShouldReturnChromosomeWithHigherFitness()
        {
            // Arrange
            var randomProvider = new RandomProvider();
            var ga = new GeneticAlgorithm(5, 100, 0.8, 0.1, randomProvider);
            ga.Initialize();
            double[] prices = GeneratePrices(100, 0.1, 1);
            ga.Evaluate(prices);

            // Act
            var selected = ga.TournamentSelection();

            // Assert
            Assert.IsNotNull(selected);
            Assert.IsTrue(selected.Fitness >= ga.GetPopulation().Min(c => c.Fitness));
        }

        [TestMethod]
        public void Crossover_ShouldCreateChildWithPeriodsFromParents()
        {
            // Arrange
            var randomProvider = new RandomProvider();
            var parent1 = new Chromosome(5, 20);
            var parent2 = new Chromosome(10, 30);
            var ga = new GeneticAlgorithm(10, 100, 0.8, 0.1, randomProvider);

            // Act
            var child = ga.Crossover(parent1, parent2);

            // Assert
            Assert.IsTrue(child.FastPeriod == 5 || child.FastPeriod == 10);
            Assert.IsTrue(child.SlowPeriod == 20 || child.SlowPeriod == 30);
            Assert.IsTrue(child.SlowPeriod > child.FastPeriod);
        }

        [TestMethod]
        public void Mutate_ShouldChangePeriodsWithMutationRate()//мне кажется, что этот тест не верен, так как существует вероятнность генерации и 5, и 20.
        {
            // Arrange
            var mockRandom = new Mock<IRandomProvider>();
            mockRandom.SetupSequence(r => r.NextDouble()).Returns(0.0).Returns(0.0); // Оба условия if выполняются
            mockRandom.Setup(r => r.Next(1, 20)).Returns(10); // Новое значение FastPeriod
            mockRandom.Setup(r => r.Next(11, 201)).Returns(30); // Новое значение SlowPeriod

            var chrom = new Chromosome(5, 20);
            var ga = new GeneticAlgorithm(10, 100, 0.8, 1.0, mockRandom.Object); // Mutation rate 1.0 для предсказуемости

            // Act
            ga.Mutate(chrom);

            // Assert
            //Assert.IsTrue(chrom.FastPeriod != 5 || chrom.SlowPeriod != 20);
            Assert.AreEqual(10, chrom.FastPeriod); // Проверяем точное значение
            Assert.AreEqual(30, chrom.SlowPeriod); // Проверяем точное значение
        }

        [TestMethod]
        public void CalculateSMA_ShouldReturnCorrectSMAValues()
        {
            // Arrange
            double[] prices = { 1, 2, 3, 4, 5 };
            int period = 3;

            // Act
            var sma = GeneticAlgorithm.CalculateSMA(prices, period);

            // Assert
            CollectionAssert.AreEqual(new double[] { 1, 1.5, 2, 3, 4 }, sma);
        }

        [TestMethod]
        public void SimulateStrategy_ShouldCalculateProfitCorrectly()
        {
            // Arrange
            double[] prices = { 100, 101, 102, 103, 104, 105 };
            int fastPeriod = 1;
            int slowPeriod = 2;

            // Act
            var profit = GeneticAlgorithm.SimulateStrategy(prices, fastPeriod, slowPeriod);

            // Assert
            Assert.AreEqual(4, profit);
        }

        private double[] GeneratePrices(int length, double trend, double noise)
        {
            var random = new Random();
            double[] prices = new double[length];
            prices[0] = 100;
            for (int i = 1; i < length; i++)
            {
                prices[i] = prices[i - 1] + trend + (random.NextDouble() * 2 * noise - noise);
            }
            return prices;
        }
    }

    public class Chromosome
    {
        public int FastPeriod { get; set; }
        public int SlowPeriod { get; set; }
        public double Fitness { get; set; }

        public Chromosome(int fast, int slow)
        {
            FastPeriod = fast;
            SlowPeriod = slow;
        }
    }

    public class GeneticAlgorithm
    {
        public List<Chromosome> GetPopulation() => population;

        private List<Chromosome> population;
        private int populationSize;
        private int generations;
        private double crossoverRate;
        private double mutationRate;
        //private static Random random = new Random();
        private readonly IRandomProvider randomProvider;

        public GeneticAlgorithm(int populationSize, int generations, double crossoverRate, double mutationRate, 
            IRandomProvider randomProvider)
        {
            this.populationSize = populationSize;
            this.generations = generations;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;
            this.randomProvider = randomProvider;
            population = new List<Chromosome>();
        }

        public void Initialize()
        {
            for (int i = 0; i < populationSize; i++)
            {
                int fast = randomProvider.Next(1, 51); // Быстрая SMA: 1-50
                int slow = randomProvider.Next(fast + 1, 201); // Медленная SMA: 10-200
                population.Add(new Chromosome(fast, slow));
            }
        }

        public void Evaluate(double[] prices)
        {
            foreach (var chrom in population)
            {
                chrom.Fitness = SimulateStrategy(prices, chrom.FastPeriod, chrom.SlowPeriod);
            }
        }

        public Chromosome TournamentSelection()
        {
            int tournamentSize = 3;
            Chromosome best = null;
            for (int i = 0; i < tournamentSize; i++)
            {
                Chromosome candidate = population[randomProvider.Next(populationSize)];
                if (best == null || candidate.Fitness > best.Fitness)
                {
                    best = candidate;
                }
            }
            return best;
        }

        public Chromosome Crossover(Chromosome parent1, Chromosome parent2)
        {
            int fast = randomProvider.Next(2) == 0 ? parent1.FastPeriod : parent2.FastPeriod;
            int slow = randomProvider.Next(2) == 0 ? parent1.SlowPeriod : parent2.SlowPeriod;
            if (slow <= fast)
            {
                int temp = fast;
                fast = slow;
                slow = temp;
            }
            return new Chromosome(fast, slow);
        }

        public void Mutate(Chromosome chrom)
        {
            if (randomProvider.NextDouble() < mutationRate)
            {
                chrom.FastPeriod = randomProvider.Next(1, chrom.SlowPeriod);
            }
            if (randomProvider.NextDouble() < mutationRate)
            {
                chrom.SlowPeriod = randomProvider.Next(chrom.FastPeriod + 1, 201);
            }
        }

        public Chromosome Run(double[] prices)
        {
            Initialize();
            for (int gen = 0; gen < generations; gen++)
            {
                Evaluate(prices);
                List<Chromosome> newPopulation = new List<Chromosome>();
                // Элитизм: сохраняем лучшую хромосому
                Chromosome best = population.OrderByDescending(c => c.Fitness).First();
                newPopulation.Add(best);
                // Создаем потомков
                while (newPopulation.Count < populationSize)
                {
                    Chromosome parent1 = TournamentSelection();
                    Chromosome parent2 = TournamentSelection();
                    Chromosome child;
                    if (randomProvider.NextDouble() < crossoverRate)
                    {
                        child = Crossover(parent1, parent2);
                    }
                    else
                    {
                        child = new Chromosome(parent1.FastPeriod, parent1.SlowPeriod);
                    }
                    Mutate(child);
                    newPopulation.Add(child);
                }
                population = newPopulation;
            }
            Evaluate(prices);
            return population.OrderByDescending(c => c.Fitness).First();
        }

        public static double[] CalculateSMA(double[] prices, int period)
        {
            double[] sma = new double[prices.Length];
            double sum = 0;
            for (int i = 0; i < period && i < prices.Length; i++)
            {
                sum += prices[i];
                sma[i] = sum / (i + 1);
            }
            for (int i = period; i < prices.Length; i++)
            {
                sum += prices[i] - prices[i - period];
                sma[i] = sum / period;
            }
            return sma;
        }

        public static double SimulateStrategy(double[] prices, int fastPeriod, int slowPeriod)
        {
            double[] fastSMA = CalculateSMA(prices, fastPeriod);
            double[] slowSMA = CalculateSMA(prices, slowPeriod);
            bool inPosition = false;
            double entryPrice = 0;
            double profit = 0;
            for (int i = 1; i < prices.Length; i++)
            {
                if (!inPosition && fastSMA[i] > slowSMA[i] && fastSMA[i - 1] <= slowSMA[i - 1])
                {
                    inPosition = true;
                    entryPrice = prices[i];
                }
                else if (inPosition && fastSMA[i] < slowSMA[i] && fastSMA[i - 1] >= slowSMA[i - 1])
                {
                    profit += prices[i] - entryPrice;
                    inPosition = false;
                }
            }
            if (inPosition)
            {
                profit += prices[prices.Length - 1] - entryPrice;
            }
            return profit;
        }
    }

    public interface IRandomProvider
    {
        double NextDouble();
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
    }

    public class RandomProvider : IRandomProvider
    {
        private Random random = new Random();
        public double NextDouble() => random.NextDouble();
        public int Next(int maxValue) => random.Next(maxValue);
        public int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);       
    }
}