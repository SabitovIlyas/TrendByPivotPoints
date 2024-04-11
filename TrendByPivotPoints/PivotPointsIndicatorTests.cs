using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using TradingSystems;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class PivotPointsIndicatorTests
    {
        DataBarsForTesting dataBarsForTesting;
        List<Bar> bars;
        PivotPointsIndicator pivotPointsIndicator;
        Security security;
        int lastBarNumber;

        [TestInitialize]
        public void TestInitialize()
        {
            dataBarsForTesting = new DataBarsForTesting();
            bars = dataBarsForTesting.GetBars();
            pivotPointsIndicator = new PivotPointsIndicator();

            IReadOnlyList<IDataBar> barsBase = new ReadAndAddList<IDataBar>();
            var barsBaseAccessAdding = (ReadAndAddList<IDataBar>)barsBase;

            IDataBar bar;
            foreach (var b in bars)
            {
                bar = new DataBarFake() { Open = b.Open, High = b.High, Low = b.Low, Close = b.Close };
                barsBaseAccessAdding.Add(bar);
            }
            ISecurity securityBase = new SecurityISecurityFake();
            var securityBaseAccessAdding = (SecurityISecurityFake)securityBase;
            securityBaseAccessAdding.Bars = barsBaseAccessAdding;

            security = new TSLabSecurity(securityBase);
            lastBarNumber = security.GetBarsCountReal() - 1;
        }      

        [TestMethod()]
        public void GetLowsSecurityTest()
        {
            //arrange            
            var expected = dataBarsForTesting.GetExpectedLows_lefLocal3_rightLocal3();

            //act
            pivotPointsIndicator.CalculateLows(security, 3, 3);
            var actual = pivotPointsIndicator.GetLows(lastBarNumber);

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
        public void GetHighsSecurityTest()
        {
            //arrange            
            var expected = dataBarsForTesting.GetExpectedHighs_lefLocal3_rightLocal3();

            //act
            pivotPointsIndicator.CalculateHighs(security, 3, 3);
            var actual = pivotPointsIndicator.GetHighs(lastBarNumber);

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