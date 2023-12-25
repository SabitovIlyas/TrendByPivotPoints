using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using System.Runtime.InteropServices;

namespace TrendByPivotPointsStrategy.Tests
{
    [TestClass()]
    public class CompressionBarsTest
    {
        [TestMethod()]
        public void GetBarsBaseFromBarCompressedTest_barNumber0_returned0_1()
        {
            //arrange                        
            IReadOnlyList<IDataBar> barsBase = new ReadOnlyList<IDataBar>();
            var barsBaseAccessAdding = (ReadOnlyList<IDataBar>)barsBase;

            IDataBar bar;
            bar = new DataBarFake() { Date = new DateTime(2021, 6, 18, 14, 1, 0), Open = 9, High = 10, Low = 8, Close = 10 };
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake() { Date = new DateTime(2021, 6, 18, 14, 2, 0), Open = 10, High = 10, Low = 7, Close = 7 };
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake() { Date = new DateTime(2021, 6, 18, 15, 30, 0), Open = 12, High = 14, Low = 8, Close = 10 };
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake() { Date = new DateTime(2021, 6, 18, 17, 33, 0), Open = 6, High = 7, Low = 6, Close = 6 };
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake() { Date = new DateTime(2021, 6, 19, 14, 34, 0), Open = 15, High = 17, Low = 15, Close = 17 };
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake() { Date = new DateTime(2021, 6, 19, 14, 40, 0), Open = 16, High = 18, Low = 15, Close = 15 };
            barsBaseAccessAdding.Add(bar);

            ISecurity securityBase = new SecurityISecurityFake();
            var securityBaseAccessAdding = (SecurityISecurityFake)securityBase;
            securityBaseAccessAdding.Bars = barsBaseAccessAdding;

            IReadOnlyList<IDataBar> barsCompressed = new ReadOnlyList<IDataBar>();
            var expected = (ReadOnlyList<IDataBar>)barsCompressed;
            bar = new DataBarFake() { Date = new DateTime(2021, 6, 18, 10, 0, 0), Open = 9, High = 14, Low = 6, Close = 6 };
            expected.Add(bar);
            bar = new DataBarFake() { Date = new DateTime(2021, 6, 19, 10, 0, 0), Open = 15, High = 18, Low = 15, Close = 15 };
            expected.Add(bar);

            //act            
            var security = new SecurityTSlab(securityBase);
            var securityCompressed = security.CompressLessIntervalTo1DayInterval();
            var actual = securityCompressed.Bars;

            //assert
            if (expected.Count == expected.Count)
            {
                int i;
                for (i = 0; i < expected.Count; i++)                
                    if (expected[i] != expected[i])
                        break;
                
                    Assert.IsTrue(i == securityCompressed.Bars.Count);
            }
            
            Assert.Fail();

        }        
    }
}