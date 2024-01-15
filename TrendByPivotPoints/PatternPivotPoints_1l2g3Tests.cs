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
    public class PatternPivotPoints_1l2g3Tests
    {
        [TestMethod()]
        public void CheckTest_1l2g3()
        {
            //arrange
            var pattern = new PatternPivotPoints_1l2g3();
            var lows = new List<double>() { 74000.0, 73500.0, 73750.0 };            

            //act
            var actual = pattern.Check(lows);
            
            //assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void CheckTest_1l2g3g4()
        {
            //arrange
            var pattern = new PatternPivotPoints_1l2g3();
            var lows = new List<double>() { 74000.0, 73500.0, 73750.0, 74250.0 };

            //act
            var actual = pattern.Check(lows);

            //assert
            Assert.IsFalse(actual);
        }        
    }
}