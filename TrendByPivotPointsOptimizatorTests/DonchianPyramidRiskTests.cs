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
                ["useTimeExit"] = 1,
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

        [TestMethod()]
        public void LevelsCountIsCategorical()
        {
            //Одна позиция и пирамида — разные стратегии, да ещё с разным риском на
            //уровень. Сдвиг по этому гену давал четверть всего разброса окрестности
            //(замер на 128 соседях: вклад 0,21 против 0,17 у следующей оси), потому
            //что шаг сетки равен единице — треть диапазона, меньше сдвинуть нельзя.
            var definition = new DonchianStrategyDefinition();
            var levels = definition.Parameters.Find(p => p.Name == "limitOpenedPositions");

            Assert.IsTrue(levels.IsCategorical,
                "Число уровней пирамиды не должно сдвигаться при построении окрестности.");
            Assert.AreEqual(1, levels.Min);
            Assert.AreEqual(4, levels.Max);
        }

        [TestMethod()]
        public void NeighbourhoodKeepsLevelsCount()
        {
            //Проверяем не флаг, а поведение: соседи обязаны сохранять число уровней.
            var definition = new DonchianStrategyDefinition();
            var builder = new NeighbourhoodBuilder(definition.Parameters, points: 32,
                percent: 0.05, seed: 1);

            var genes = new Dictionary<string, double>();
            foreach (var parameter in definition.Parameters)
                genes[parameter.Name] = (parameter.Min + parameter.Max) / 2;
            genes["limitOpenedPositions"] = 2;

            foreach (var neighbour in builder.Build("проверка", genes))
                Assert.AreEqual(2, neighbour["limitOpenedPositions"], 1e-9,
                    "Сосед сменил число уровней пирамиды — это другая стратегия, а не сосед.");
        }
    }
}
