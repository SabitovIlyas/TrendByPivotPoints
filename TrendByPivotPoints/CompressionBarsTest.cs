using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using TrendByPivotPoints;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class CompressionBarsTest
    {
        [TestMethod()]
        public void GetBarsBaseFromBarCompressedTest_barNumber0_returned0_1()
        {
            //arrange                        

            IReadOnlyList<Bar> barsBase = new ReadAndAddList<Bar>();
            var sourceBars = (ReadAndAddList<Bar>)barsBase;

            Bar bar;
            bar = new Bar() { Date = new DateTime(2021, 6, 18, 14, 1, 0), Open = 9, High = 10, Low = 8, Close = 10 };
            sourceBars.Add(bar);
            bar = new Bar() { Date = new DateTime(2021, 6, 18, 14, 2, 0), Open = 10, High = 10, Low = 7, Close = 7 };
            sourceBars.Add(bar);
            bar = new Bar() { Date = new DateTime(2021, 6, 18, 15, 30, 0), Open = 12, High = 14, Low = 8, Close = 10 };
            sourceBars.Add(bar);
            bar = new Bar() { Date = new DateTime(2021, 6, 18, 17, 33, 0), Open = 6, High = 7, Low = 6, Close = 6 };
            sourceBars.Add(bar);
            bar = new Bar() { Date = new DateTime(2021, 6, 19, 14, 34, 0), Open = 15, High = 17, Low = 15, Close = 17 };
            sourceBars.Add(bar);
            bar = new Bar() { Date = new DateTime(2021, 6, 19, 14, 40, 0), Open = 16, High = 18, Low = 15, Close = 15 };
            sourceBars.Add(bar);

            //ISecurity securityBase = new SecurityISecurityFake();
            //var securityBaseAccessAdding = (SecurityISecurityFake)securityBase;
            //securityBaseAccessAdding.Bars = sourceBars;

            ISecurity securityBase = CustomSecurity.Create(sourceBars);
            //var securityBaseAccessAdding = (SecurityISecurityFake)securityBase;
            //securityBaseAccessAdding.Bars = sourceBars;

            IReadOnlyList<Bar> exptectedBars = new ReadAndAddList<Bar>();
            var barsCompressed = (ReadAndAddList<Bar>)exptectedBars;
            bar = new Bar() { Date = new DateTime(2021, 6, 18, 10, 0, 0), Open = 9, High = 14, Low = 6, Close = 6 };
            barsCompressed.Add(bar);
            bar = new Bar() { Date = new DateTime(2021, 6, 19, 10, 0, 0), Open = 15, High = 18, Low = 15, Close = 15 };
            barsCompressed.Add(bar);
            
            //act            

            var security = new TSLabSecurity(securityBase);
            var securityCompressed = security.CompressLessIntervalTo1DayInterval();
            var actualBars = securityCompressed.Bars;
                        
            //assert

            Assert.IsTrue(exptectedBars.IsListBarEquals(actualBars));
        }        
    }
}