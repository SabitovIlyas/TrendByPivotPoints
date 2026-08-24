using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator.Tests
{
    /// <summary>
    /// Деление риска между уровнями пирамиды. Каждый уровень считает контракты
    /// самостоятельно и рискует полной долей, поэтому без деления четыре уровня
    /// дают четырёхкратный риск на сделку.
    /// </summary>
    [TestClass()]
    public class DonchianPyramidRiskTests
    {
        private Dictionary<string, double> Genes(int levels)
        {
            return new Dictionary<string, double>()
            {
                ["fastDonchian"] = 20,
                ["slowDonchian"] = 55,
                ["atrPeriod"] = 14,
                ["limitOpenedPositions"] = levels,
                ["kAtrForOpenPosition"] = 0.5,
                ["kAtrForStopLoss"] = 2,
                ["maxBarsInPosition"] = 200,
                ["useChannelExit"] = 0,
            };
        }

        private double RiskFor(int levels)
        {
            var settings = new Settings() { RiskValuePrcnt = 2, Equity = 1000000 };
            var ticker = new Ticker("Si", Currency.RUB, shares: 1, bars: new List<Bar>(),
                logger: new LoggerNull(), commissionRate: 0, isUSD: false, rateUSD: 1);

            var parameters = new DonchianStrategyDefinition().CreateSystemParameters(
                Genes(levels), ticker, PositionSide.Long,
                new Interval(60, DataIntervals.MINUTE), settings);

            return (double)parameters.GetValue("riskValuePrcnt");
        }

        [TestMethod()]
        public void SingleLevel_KeepsFullRisk()
        {
            Assert.AreEqual(2, RiskFor(1), 1e-9);
        }

        [TestMethod()]
        public void PyramidDividesRiskBetweenLevels()
        {
            Assert.AreEqual(1, RiskFor(2), 1e-9);
            Assert.AreEqual(0.5, RiskFor(4), 1e-9);
        }

        [TestMethod()]
        public void TotalRiskNeverExceedsTheLimit()
        {
            //Суммарный риск полностью построенной пирамиды: доля на уровень,
            //умноженная на число уровней.
            for (var levels = 1; levels <= 4; levels++)
                Assert.AreEqual(2, RiskFor(levels) * levels, 1e-9,
                    "Суммарный риск на сделку должен оставаться в пределах лимита.");
        }
    }
}
