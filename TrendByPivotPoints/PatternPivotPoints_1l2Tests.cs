using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradingSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPoints.Tests
{
    [TestClass()]
    public class PatternPivotPoints_1l2Tests
    {
        [TestMethod()]
        public void CheckTest_1l2()
        {
            //arrange
            var pattern = new PatternPivotPoints_1l2();
            var extremums = new List<double>() { 77500.0, 77250.0 };

            //act
            var actual = pattern.Check(extremums);

            //assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void CheckTest_1g2()
        {
            //arrange
            var pattern = new PatternPivotPoints_1l2();
            var extremums = new List<double>() { 77500.0, 77800.0 };

            //act
            var actual = pattern.Check(extremums);

            //assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void CheckTest_1g2l3()
        {
            //arrange
            var pattern = new PatternPivotPoints_1l2();
            var extremums = new List<double>() { 77500.0, 77800.0, 77600.0 };

            //act
            var actual = pattern.Check(extremums);

            //assert
            Assert.IsTrue(actual);
        }
    }
}