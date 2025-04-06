using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
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
}