using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPointsStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPoints.Tests
{
    [TestClass()]
    public class PivotPointsIndicatorTests
    {

        DataBarsForTesting dataBarsForTesting;
        List<Bar> bars;
        PivotPointsIndicator pivotPointsIndicator;

        [TestInitialize]
        public void TestInitialize()
        {
            dataBarsForTesting = new DataBarsForTesting();
            bars = dataBarsForTesting.GetBars();
            pivotPointsIndicator = new PivotPointsIndicator();
        }


        [TestMethod()]
        public void GetLowsTest()
        {
            //arrange            
            var expected = dataBarsForTesting.GetExpectedLows_lefLocal3_rightLocal3();

            //act
            var actual = pivotPointsIndicator.GetLows(bars, 3, 3);

            //assert
            var check = true;
            if (expected != null && actual != null && expected.Count == actual.Count)
                for (var i = 0; i < expected.Count; i++)
                {
                    if ((actual[i].BarNumber != expected[i].BarNumber) || (actual[i].Value != expected[i].Value))
                    {
                        check = false;
                        break;
                    }                    
                }
            else
                check = false;

            Assert.IsTrue(check);
        }

        [TestMethod()]
        public void GetLowsSecurityTest()
        {
            //arrange            
            var expected = dataBarsForTesting.GetExpectedLows_lefLocal3_rightLocal3();

            //act
            var actual = pivotPointsIndicator.GetLows(bars, 3, 3);

            //assert
            var check = true;
            if (expected != null && actual != null && expected.Count == actual.Count)
                for (var i = 0; i < expected.Count; i++)
                {
                    if ((actual[i].BarNumber != expected[i].BarNumber) || (actual[i].Value != expected[i].Value))
                    {
                        check = false;
                        break;
                    }
                }
            else
                check = false;

            Assert.IsTrue(check);
        }

        [TestMethod()]
        public void GetHighsTest()
        {
            //arrange            
            var expected = dataBarsForTesting.GetExpectedHighs_lefLocal3_rightLocal3();

            //act
            var actual = pivotPointsIndicator.GetHighs(bars, 3, 3);

            //assert
            var check = true;
            if (expected != null && actual != null && expected.Count == actual.Count)
                for (var i = 0; i < expected.Count; i++)
                {
                    if ((actual[i].BarNumber != expected[i].BarNumber) || (actual[i].Value != expected[i].Value))
                    {
                        check = false;
                        break;
                    }
                }
            else
                check = false;

            Assert.IsTrue(check);
        }
    }
}