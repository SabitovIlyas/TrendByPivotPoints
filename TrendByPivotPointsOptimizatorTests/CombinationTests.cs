using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPointsOptimizator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class CombinationTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [DataTestMethod]
        [DataRow(5, 3, 3, true)]
        [DataRow(5, 3, 4, false)]
        [DataRow(3, 5, 4, false)]
        [TestMethod()]
        public void IsNearCobinationPassedCaseTest(double combination1, double combination2, double barrier, bool expected)
        {
            var combinationMain = Combination.Create(value: combination1);
            var combinationNeighborhood = Combination.Create(value: combination2);
            combinationMain.AddNearCombination(combinationNeighborhood);

            var actual =
                combinationMain.IsCombinationPassedTestWhenTheyAreAllGreaterOrEqualThenValue(barrier);

            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow(5, 3, 1, 3)]     
        public void GetAverageValueTest(double combination1, double combination2, double combination3, double expected)
        {
            var combinationMain = Combination.Create(value: combination1);
            var combinationNeighborhood = Combination.Create(value: combination2);
            combinationMain.AddNearCombination(combinationNeighborhood);

            combinationNeighborhood = Combination.Create(value: combination3);
            combinationMain.AddNearCombination(combinationNeighborhood);

            var actual =
                combinationMain.GetAverageValue();

            Assert.AreEqual(expected, actual);
        }
    }
}