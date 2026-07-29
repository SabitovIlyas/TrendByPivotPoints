using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class GeneticAlgorithmUniversalTests
    {
        //Синтетические часовые бары: детерминированное случайное блуждание.
        private List<Bar> CreateBars(int days)
        {
            var random = new Random(123);
            var bars = new List<Bar>();
            var price = 100000d;
            var date = new DateTime(2026, 1, 5);

            for (var d = 0; d < days; d++)
            {
                for (var h = 10; h <= 23; h++)
                {
                    var open = price;
                    var delta = random.Next(-150, 151);
                    var close = open + delta;
                    var high = Math.Max(open, close) + random.Next(0, 80);
                    var low = Math.Min(open, close) - random.Next(0, 80);
                    bars.Add(Bar.Create(date.AddDays(d).AddHours(h), open, high, low,
                        close, 1, "TEST", "60", 0));
                    price = close;
                }
            }
            return bars;
        }

        private Settings CreateTestSettings(PositionSide side = PositionSide.Long)
        {
            return new Settings
            {
                Sides = new List<PositionSide>() { side },
                TimeFrames = new List<Interval>() { new Interval(60, DataIntervals.MINUTE) },
                PopulationSize = 8,
                Generations = 2,
                BackwardDays = 40,
                ForwardDays = 10,
                ForwardPeriodsCount = 2,
                ShiftWindowDays = 5,
            };
        }

        private List<ChromosomeUniversal> RunGa(StrategyDefinition definition, int seed,
            PositionSide side = PositionSide.Long)
        {
            var bars = CreateBars(days: 90);
            var logger = new LoggerNull();
            var ticker = new Ticker("TEST", Currency.RUB, 1, bars, logger,
                commissionRate: 0, isUSD: false, rateUSD: 1);
            var settings = CreateTestSettings(side);
            var context = new ContextLab();
            var randomProvider = new RandomProvider(seed);

            var ga = new GeneticAlgorithmUniversal(settings.PopulationSize,
                settings.Generations, crossoverRate: 0.85, mutationRate: 0.10,
                randomProvider, new List<Ticker>() { ticker }, settings, context,
                definition, logger);
            ga.IsLastBackwardTesting = true;
            return ga.Run(period: 0);
        }

        [TestMethod()]
        public void SameSeed_ProducesIdenticalResult_MeanReversion()
        {
            var first = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 42);
            var second = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 42);

            Assert.IsTrue(first.Any(), "Оптимизатор не вернул ни одной хромосомы.");
            Assert.AreEqual(first.First().Name, second.First().Name);
            Assert.AreEqual(first.First().FitnessValue, second.First().FitnessValue);
            Assert.AreEqual(first.First().Profit, second.First().Profit);
        }

        [TestMethod()]
        public void DifferentSeeds_BothProduceResult_MeanReversion()
        {
            var first = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 42);
            var second = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 43);

            Assert.IsTrue(first.Any());
            Assert.IsTrue(second.Any());
        }

        [TestMethod()]
        public void DonchianDefinition_RunsThroughUniversalGa()
        {
            var best = RunGa(new DonchianStrategyDefinition(), seed: 42);

            Assert.IsTrue(best.Any(), "Оптимизатор не вернул ни одной хромосомы.");
        }

        [TestMethod()]
        public void ShortSide_UsesShortRanges()
        {
            //Для шорта пороги RSI ищутся в собственных диапазонах:
            //вход [50;95], выход [5;50] — независимо от лонга.
            var definition = new MeanReversionStrategyDefinition(PositionSide.Short);
            var best = RunGa(definition, seed: 42, side: PositionSide.Short);

            Assert.IsTrue(best.Any(), "Оптимизатор не вернул ни одной хромосомы.");
            var genes = best.First().Genes;
            Assert.IsTrue(genes["rsiEntryLevel"] >= 50 && genes["rsiEntryLevel"] <= 95,
                "rsiEntryLevel шорта вне диапазона [50; 95]: " + genes["rsiEntryLevel"]);
            Assert.IsTrue(genes["rsiExitLevel"] >= 5 && genes["rsiExitLevel"] <= 50,
                "rsiExitLevel шорта вне диапазона [5; 50]: " + genes["rsiExitLevel"]);
        }

        [TestMethod()]
        public void GeneValues_StayWithinDescriptorBounds()
        {
            var definition = new MeanReversionStrategyDefinition(PositionSide.Long);
            var best = RunGa(definition, seed: 7);

            Assert.IsTrue(best.Any());
            foreach (var descriptor in definition.Parameters)
            {
                var value = best.First().Genes[descriptor.Name];
                Assert.IsTrue(value >= descriptor.Min && value <= descriptor.Max,
                    string.Format("Ген {0} = {1} вне диапазона [{2}; {3}].",
                        descriptor.Name, value, descriptor.Min, descriptor.Max));
            }
        }
    }
}
