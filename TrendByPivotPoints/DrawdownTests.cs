using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
using System.Collections.Generic;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class DrawdownTests
    {
        private double[] equityHistory;

        [TestInitialize]
        public void TestInitialize()
        {
            equityHistory = new double[] {
                100000.00,
                104666.67,
                109333.33,
                114000.00,
                118666.67,
                123333.33,
                128000.00,
                121200.00,
                114400.00,
                107600.00,
                100800.00,
                94000.00,
                95714.29,
                97428.58,
                99142.87,
                100857.16,
                102571.45,
                104285.74,
                106000.00,
                101666.67,
                97333.34,
                93000.01,
                88666.68,
                84333.35,
                80000.00,
                84200.00,
                88400.00,
                92600.00,
                96800.00,
                101000.00
            };
        }
        
        [TestMethod()]
        public void GetDrawdownTest()
        {          
            double expected = 37.5d;
            var logger = new LoggerNull();
            var bar = new Bar() { Close = 100 };            
            var security = new SecurityLab(currency: Currency.RUB, shares: 1, new List<Bar> { bar },
                logger, commissionRate: 0);

            var account = new AccountFake(initDeposit: 100000, Currency.RUB,
                new List<Security> { security }, logger);
            account.EquityHistory = equityHistory;
            var actual = account.GetMaxDrawDownPrcnt(barNumber: equityHistory.Length);
            Assert.AreEqual(expected, actual);            
        }
    }
}