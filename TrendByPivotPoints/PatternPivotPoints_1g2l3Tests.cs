using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPoints.Tests
{
    [TestClass()]
    public class PatternPivotPoints_1g2l3Tests
    {

        [TestMethod()]
        public void CheckTest_1g2l3()
        {
            //arrange
            var pattern = new PatternPivotPoints_1g2l3();
            var highs = new List<double>() { 77500.0, 77800.0, 77300.0 };

            //act
            var actual = pattern.Check(highs);

            //assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void CheckTest_1g2l3l4()
        {
            //arrange
            var pattern = new PatternPivotPoints_1g2l3();
            var highs = new List<double>() { 77500.0, 77800.0, 77300.0, 77000.0 };

            //act
            var actual = pattern.Check(highs);

            //assert
            Assert.IsFalse(actual);
        }       
    }
}