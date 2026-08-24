using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsOptimizator.Tests
{
    /// <summary>
    /// Проверка Repair у стратегии возврата к среднему. Он наводит порядок в паре
    /// «вход — выход», иначе условие выхода оказывается выполненным уже в момент
    /// входа: пересечение сработать не может, и позиция пропускает первую
    /// возможность закрыться.
    /// </summary>
    [TestClass()]
    public class MeanReversionRepairTests
    {
        private Dictionary<string, double> Genes(int entryPeriod, int exitPeriod,
            int entryLevel, int exitLevel)
        {
            return new Dictionary<string, double>()
            {
                ["maPeriod"] = 100,
                ["rsiEntryPeriod"] = entryPeriod,
                ["rsiExitPeriod"] = exitPeriod,
                ["atrPeriod"] = 14,
                ["rsiEntryLevel"] = entryLevel,
                ["rsiExitLevel"] = exitLevel,
                ["atrMultiplier"] = 2,
                ["useTrailingStop"] = 0,
                ["rsiEntryMode"] = 0,
                ["rsiExitMode"] = 1,
            };
        }

        private Dictionary<string, double> Repaired(PositionSide side, int entryPeriod,
            int exitPeriod, int entryLevel, int exitLevel)
        {
            var genes = Genes(entryPeriod, exitPeriod, entryLevel, exitLevel);
            new MeanReversionStrategyDefinition(side).Repair(genes);
            return genes;
        }

        [TestMethod()]
        public void Repair_MakesExitRsiNoFasterThanEntryRsi_Long()
        {
            //Быстрый выходной RSI на замерах давал до 98% входов, на которых
            //условие выхода выполнено заранее.
            var genes = Repaired(PositionSide.Long, entryPeriod: 40, exitPeriod: 9,
                entryLevel: 30, exitLevel: 70);

            Assert.AreEqual(9, genes["rsiEntryPeriod"]);
            Assert.AreEqual(40, genes["rsiExitPeriod"]);
        }

        [TestMethod()]
        public void Repair_MakesExitRsiNoFasterThanEntryRsi_Short()
        {
            var genes = Repaired(PositionSide.Short, entryPeriod: 40, exitPeriod: 9,
                entryLevel: 70, exitLevel: 30);

            Assert.AreEqual(9, genes["rsiEntryPeriod"]);
            Assert.AreEqual(40, genes["rsiExitPeriod"]);
        }

        [TestMethod()]
        public void Repair_KeepsPeriodsWhenAlreadyOrdered()
        {
            var genes = Repaired(PositionSide.Long, entryPeriod: 9, exitPeriod: 40,
                entryLevel: 30, exitLevel: 70);

            Assert.AreEqual(9, genes["rsiEntryPeriod"]);
            Assert.AreEqual(40, genes["rsiExitPeriod"]);
        }

        [TestMethod()]
        public void Repair_PutsLongExitLevelAboveEntryLevel()
        {
            var genes = Repaired(PositionSide.Long, entryPeriod: 9, exitPeriod: 40,
                entryLevel: 80, exitLevel: 60);

            Assert.IsTrue(genes["rsiExitLevel"] > genes["rsiEntryLevel"],
                "У лонга порог выхода должен быть выше порога входа.");
            Assert.AreEqual(60, genes["rsiEntryLevel"]);
            Assert.AreEqual(80, genes["rsiExitLevel"]);
        }

        [TestMethod()]
        public void Repair_PutsShortExitLevelBelowEntryLevel()
        {
            var genes = Repaired(PositionSide.Short, entryPeriod: 9, exitPeriod: 40,
                entryLevel: 40, exitLevel: 75);

            Assert.IsTrue(genes["rsiExitLevel"] < genes["rsiEntryLevel"],
                "У шорта порог выхода должен быть ниже порога входа.");
            Assert.AreEqual(75, genes["rsiEntryLevel"]);
            Assert.AreEqual(40, genes["rsiExitLevel"]);
        }

        [TestMethod()]
        public void Repair_SeparatesEqualLevels_Long()
        {
            //При равных порогах и равных периодах ряд RSI буквально один и тот же,
            //и условие выхода выполнено на каждом входе без исключения.
            var genes = Repaired(PositionSide.Long, entryPeriod: 20, exitPeriod: 20,
                entryLevel: 70, exitLevel: 70);

            Assert.IsTrue(genes["rsiExitLevel"] > genes["rsiEntryLevel"],
                "Совпавшие пороги должны быть разведены.");
        }

        [TestMethod()]
        public void Repair_SeparatesEqualLevels_Short()
        {
            var genes = Repaired(PositionSide.Short, entryPeriod: 20, exitPeriod: 20,
                entryLevel: 70, exitLevel: 70);

            Assert.IsTrue(genes["rsiExitLevel"] < genes["rsiEntryLevel"],
                "Совпавшие пороги должны быть разведены.");
        }

        [TestMethod()]
        public void Repair_SeparatesEqualLevelsAtRangeBoundary()
        {
            //На верхней границе диапазона поднять порог выхода некуда — значит
            //опускается порог входа, и оба остаются внутри [5;95].
            var genes = Repaired(PositionSide.Long, entryPeriod: 20, exitPeriod: 20,
                entryLevel: 95, exitLevel: 95);

            Assert.IsTrue(genes["rsiExitLevel"] > genes["rsiEntryLevel"]);
            Assert.IsTrue(genes["rsiEntryLevel"] >= 5 && genes["rsiEntryLevel"] <= 95);
            Assert.IsTrue(genes["rsiExitLevel"] >= 5 && genes["rsiExitLevel"] <= 95);
        }

        [TestMethod()]
        public void Repair_SeparatesEqualLevelsAtLowerRangeBoundary()
        {
            var genes = Repaired(PositionSide.Short, entryPeriod: 20, exitPeriod: 20,
                entryLevel: 5, exitLevel: 5);

            Assert.IsTrue(genes["rsiExitLevel"] < genes["rsiEntryLevel"]);
            Assert.IsTrue(genes["rsiEntryLevel"] >= 5 && genes["rsiEntryLevel"] <= 95);
            Assert.IsTrue(genes["rsiExitLevel"] >= 5 && genes["rsiExitLevel"] <= 95);
        }

        [TestMethod()]
        public void Parameters_SearchBothLevelsInOneRange()
        {
            //Раньше порядок задавался непересекающимися диапазонами, и вход по
            //высокому порогу был недостижим для лонга.
            var definition = new MeanReversionStrategyDefinition(PositionSide.Long);

            var entry = definition.Parameters.Find(p => p.Name == "rsiEntryLevel");
            var exit = definition.Parameters.Find(p => p.Name == "rsiExitLevel");

            Assert.AreEqual(5, entry.Min);
            Assert.AreEqual(95, entry.Max);
            Assert.AreEqual(5, exit.Min);
            Assert.AreEqual(95, exit.Max);
        }

        [TestMethod()]
        public void Parameters_ModesAreCategorical()
        {
            //Режим — переключатель, а не величина: в окрестности он не сдвигается.
            var definition = new MeanReversionStrategyDefinition(PositionSide.Long);

            var entryMode = definition.Parameters.Find(p => p.Name == "rsiEntryMode");
            var exitMode = definition.Parameters.Find(p => p.Name == "rsiExitMode");

            Assert.IsTrue(entryMode.IsCategorical);
            Assert.IsTrue(exitMode.IsCategorical);
            Assert.AreEqual(2, entryMode.Max);
            Assert.AreEqual(3, exitMode.Max);
        }
    }
}
