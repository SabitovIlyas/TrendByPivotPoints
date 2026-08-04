using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TradingSystems;
using TSLab.DataSource;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class PopulationDiversityTests
    {
        /// <summary>Стратегия с задаваемыми диапазонами — чтобы проверить,
        /// как считается разнообразие при закреплённых генах.</summary>
        private class TestStrategyDefinition : StrategyDefinition
        {
            private readonly List<ParameterDescriptor> parameters;

            public TestStrategyDefinition(List<ParameterDescriptor> parameters)
            {
                this.parameters = parameters;
            }

            public override string Name => "Test";
            public override List<ParameterDescriptor> Parameters => parameters;

            public override Starter CreateStarter(Context context,
                List<Security> securities, Logger logger)
            {
                throw new NotImplementedException();
            }
        }

        private GeneticAlgorithmUniversal CreateGa(List<ParameterDescriptor> parameters)
        {
            var logger = new LoggerNull();
            var bars = new List<Bar>()
            {
                Bar.Create(new DateTime(2026, 1, 5), 1, 1, 1, 1, 1, "TEST", "60", 0),
            };
            var ticker = new Ticker("TEST", Currency.RUB, 1, bars, logger,
                commissionRate: 0, isUSD: false, rateUSD: 1);

            var settings = new Settings()
            {
                Sides = new List<PositionSide>() { PositionSide.Long },
                TimeFrames = new List<Interval>() { new Interval(60, DataIntervals.MINUTE) },
            };

            return new GeneticAlgorithmUniversal(populationSize: 2, generations: 1,
                crossoverRate: 0.85, mutationRate: 0.1, new RandomProvider(42),
                new List<Ticker>() { ticker }, settings, new ContextLab(),
                new TestStrategyDefinition(parameters), logger, logger);
        }

        private List<ChromosomeUniversal> CreatePopulation(
            Dictionary<string, double> first, Dictionary<string, double> second)
        {
            var logger = new LoggerNull();
            var bars = new List<Bar>()
            {
                Bar.Create(new DateTime(2026, 1, 5), 1, 1, 1, 1, 1, "TEST", "60", 0),
            };
            var ticker = new Ticker("TEST", Currency.RUB, 1, bars, logger,
                commissionRate: 0, isUSD: false, rateUSD: 1);
            var timeFrame = new Interval(60, DataIntervals.MINUTE);

            return new List<ChromosomeUniversal>()
            {
                new ChromosomeUniversal(ticker, timeFrame, PositionSide.Long, first),
                new ChromosomeUniversal(ticker, timeFrame, PositionSide.Long, second),
            };
        }

        [TestMethod()]
        public void Diversity_IgnoresPinnedGenesInDenominator()
        {
            //Ген, закреплённый строкой Range вида 0:0, не может отличаться у особей.
            //Если делить сумму на общее число генов, разнообразие занижается тем
            //сильнее, чем больше генов закреплено, и прогон останавливается раньше.
            var withPinned = CreateGa(new List<ParameterDescriptor>()
            {
                new ParameterDescriptor("maPeriod", 50, 250),
                new ParameterDescriptor("useTrailingStop", 0, 0),
            });

            var onlyFree = CreateGa(new List<ParameterDescriptor>()
            {
                new ParameterDescriptor("maPeriod", 50, 250),
            });

            var populationWithPinned = CreatePopulation(
                new Dictionary<string, double>()
                {
                    { "maPeriod", 50 }, { "useTrailingStop", 0 },
                },
                new Dictionary<string, double>()
                {
                    { "maPeriod", 250 }, { "useTrailingStop", 0 },
                });

            var populationOnlyFree = CreatePopulation(
                new Dictionary<string, double>() { { "maPeriod", 50 } },
                new Dictionary<string, double>() { { "maPeriod", 250 } });

            Assert.AreEqual(
                onlyFree.CalculatePopulationDiversity(populationOnlyFree),
                withPinned.CalculatePopulationDiversity(populationWithPinned),
                "Закреплённый ген не должен занижать разнообразие.");
        }

        [TestMethod()]
        public void Diversity_IsOneForOppositeCorners()
        {
            //Две особи на противоположных краях диапазонов — разнообразие 100%.
            var ga = CreateGa(new List<ParameterDescriptor>()
            {
                new ParameterDescriptor("maPeriod", 50, 250),
                new ParameterDescriptor("atrPeriod", 5, 50),
            });

            var population = CreatePopulation(
                new Dictionary<string, double>()
                {
                    { "maPeriod", 50 }, { "atrPeriod", 5 },
                },
                new Dictionary<string, double>()
                {
                    { "maPeriod", 250 }, { "atrPeriod", 50 },
                });

            Assert.AreEqual(1.0, ga.CalculatePopulationDiversity(population), 1e-9);
        }

        [TestMethod()]
        public void Diversity_IsZeroForIdenticalChromosomes()
        {
            var ga = CreateGa(new List<ParameterDescriptor>()
            {
                new ParameterDescriptor("maPeriod", 50, 250),
            });

            var population = CreatePopulation(
                new Dictionary<string, double>() { { "maPeriod", 100 } },
                new Dictionary<string, double>() { { "maPeriod", 100 } });

            Assert.AreEqual(0, ga.CalculatePopulationDiversity(population));
        }

        [TestMethod()]
        public void Diversity_IsZeroWhenEveryGeneIsPinned()
        {
            //Делить не на что — но падать тоже нельзя.
            var ga = CreateGa(new List<ParameterDescriptor>()
            {
                new ParameterDescriptor("useTrailingStop", 0, 0),
            });

            var population = CreatePopulation(
                new Dictionary<string, double>() { { "useTrailingStop", 0 } },
                new Dictionary<string, double>() { { "useTrailingStop", 0 } });

            Assert.AreEqual(0, ga.CalculatePopulationDiversity(population));
        }
    }
}
