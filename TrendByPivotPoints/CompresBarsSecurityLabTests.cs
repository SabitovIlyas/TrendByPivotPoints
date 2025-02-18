using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TrendByPivotPointsStarter;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class CompresBarsSecurityLabTests
    {
        private SecurityLab CreateSecurity()
        {
            var logger = new ConsoleLogger();
            var bars = CreateBars();
            var security = new SecurityLab(Currency.Ruble, shares: 1, bars, logger, commissionRate:0);          

            return security;
        }

        private List<Bar> CreateBars()
        {
            var bars = new List<Bar>()
            {
                Bar.Create(new DateTime(2024,01,01,09,30,00), open: 5425, high: 5500, low: 5400, close: 5475, volume: 10000, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),//0               
            };

            return bars;
        }

        [TestMethod]        
        public void GetCompressedBarsTest()
        {
            var sec = CreateSecurity();
            var expected = Bar.Create(new DateTime(2024, 01, 01, 09, 30, 00), 
                open: 5425, high: 5500, low: 5400, close: 5475, volume: 10000, 
                ticker: "TEST.TICKER", period: "1", digitsAfterPoint: 0);

            var actual = sec.Bars.First();
            Assert.AreEqual(expected, actual);
        }              
    }
}