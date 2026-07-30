using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TradingSystems;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class SettingsParsingTests
    {
        [TestMethod()]
        public void CreateSettings_ParsesAllKeys()
        {
            var fileName = Path.GetTempFileName();
            File.WriteAllLines(fileName, new[]
            {
                "PositionSide:Short",
                "TimeFrames:60min",
                "Strategy:MeanReversion",
                "Seed:42",
                "PopulationSize:50",
                "Generations:100",
                "CrossoverRate:0.9",
                "MutationRate:0,15",
                "Patience:25",
                "BackwardDays:730",
                "ForwardDays:180",
                "ForwardPeriodsCount:5",
                "ShiftWindowDays:15",
                "TrimHistory:0",
                "Equity:200000",
                "RiskValuePrcnt:1.5",
                @"SecuritiesFile:C:\Данные\!Securities_Si.txt",
                "Range:maPeriod:100:200:5",
                "Range:atrMultiplier:1:2:0.5",
            });

            try
            {
                var starter = new OptimizatorGeneticAlgorithmStarter();
                var settings = starter.CreateSettings(fileName);

                Assert.AreEqual(1, settings.Sides.Count);
                Assert.AreEqual(PositionSide.Short, settings.Sides[0]);
                Assert.AreEqual("MeanReversion", settings.Strategy);
                Assert.AreEqual(42, settings.Seed);
                Assert.AreEqual(50, settings.PopulationSize);
                Assert.AreEqual(100, settings.Generations);
                Assert.AreEqual(0.9, settings.CrossoverRate);
                Assert.AreEqual(0.15, settings.MutationRate);   //запятая тоже понимается
                Assert.AreEqual(25, settings.Patience);
                Assert.AreEqual(730, settings.BackwardDays);
                Assert.AreEqual(180, settings.ForwardDays);
                Assert.AreEqual(5, settings.ForwardPeriodsCount);
                Assert.AreEqual(15, settings.ShiftWindowDays);
                Assert.IsFalse(settings.TrimHistory);
                Assert.AreEqual(200000, settings.Equity);
                Assert.AreEqual(1.5, settings.RiskValuePrcnt);
                Assert.AreEqual(@"C:\Данные\!Securities_Si.txt", settings.SecuritiesFile);

                Assert.AreEqual(2, settings.ParameterRanges.Count);
                Assert.AreEqual(100, settings.ParameterRanges["maPeriod"].Min);
                Assert.AreEqual(200, settings.ParameterRanges["maPeriod"].Max);
                Assert.AreEqual(5, settings.ParameterRanges["maPeriod"].Step);
                Assert.AreEqual(0.5, settings.ParameterRanges["atrMultiplier"].Step);
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod()]
        public void ApplyRangeOverrides_ReplacesDescriptorRanges()
        {
            var settings = new Settings();
            settings.ParameterRanges["maPeriod"] = new ParameterRange()
            {
                Min = 100,
                Max = 150,
                Step = 10,
            };

            var definition = new MeanReversionStrategyDefinition(PositionSide.Long);
            definition.ApplyRangeOverrides(settings);

            var maPeriod = definition.Parameters.Find(p => p.Name == "maPeriod");
            Assert.AreEqual(100, maPeriod.Min);
            Assert.AreEqual(150, maPeriod.Max);
            Assert.AreEqual(10, maPeriod.Step);

            //Остальные параметры не тронуты.
            var atrPeriod = definition.Parameters.Find(p => p.Name == "atrPeriod");
            Assert.AreEqual(5, atrPeriod.Min);
            Assert.AreEqual(50, atrPeriod.Max);
        }
    }
}
