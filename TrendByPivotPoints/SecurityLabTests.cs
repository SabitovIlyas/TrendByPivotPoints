using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class SecurityLabTests
    {
        SecurityLab security;

        [TestInitialize]
        public void TestInitialize()
        {
            var logger = new LoggerNull();
            var bars = new List<Bar>() {
                Bar.Create(new DateTime(2023, 11, 17), 90090, 91400, 89926, 91012),
                Bar.Create(new DateTime(2023, 11, 20), 91036, 91233, 88744, 89003),
                Bar.Create(new DateTime(2023, 11, 22), 90082, 90657, 88787, 89233),
                Bar.Create(new DateTime(2023, 11, 24), 89127, 90080, 88830, 89463),
                Bar.Create(new DateTime(2023, 11, 27), 89127, 90080, 88830, 89463)
            };

            security = new SecurityLab("SecurityNameTest", Currency.RUB, shares: 1,
                5000, 4500, bars, logger);
        }

        [TestMethod()]
        public void GetLastActiveForSignal_Test()
        {
            security.Update(barNumber: 0);
            security.SellIfLess(barNumber: 1, contracts: 1, entryPricePlanned: 89900,
                "SE", isConverted: false);

            security.Update(barNumber: 1);
            var position = security.GetLastActiveForSignal("SE", barNumber: 1);
            Assert.IsNotNull(position);
            Assert.AreEqual(expected: 89900, actual: position.EntryPrice);
        }

        [TestMethod()]
        public void GetActiveOrdersTest()
        {
            security.Update(barNumber: 0);
            security.SellIfLess(barNumber: 1, contracts: 1, entryPricePlanned: 88000,
                "SE_1", isConverted: false);
            security.Update(barNumber: 1);
            security.SellIfLess(barNumber: 2, contracts: 1, entryPricePlanned: 87000,
                "SE_2", isConverted: false);
            security.Update(barNumber: 2);
            security.SellIfLess(barNumber: 3, contracts: 1, entryPricePlanned: 86000,
                "SE_3", isConverted: false);
            security.Update(barNumber: 3);
            security.SellIfLess(barNumber: 4, contracts: 1, entryPricePlanned: 85000,
                "SE_4", isConverted: false);
            security.Update(barNumber: 4);            

            var orders = security.GetActiveOrders(barNumber: 3);
                        
            Assert.AreEqual(expected: 3, actual: orders.Count);
            Assert.AreEqual(expected: 88000, actual: orders[0].Price);
            Assert.AreEqual(expected: 87000, actual: orders[1].Price);
            Assert.AreEqual(expected: 86000, actual: orders[2].Price);
        }

        [TestMethod()]
        public void GetLastClosedShortPosition_Test()
        {
            security.Update(barNumber: 0);
            security.SellIfLess(barNumber: 1, contracts: 1, entryPricePlanned: 89900,
                "SE", isConverted: false);

            security.Update(barNumber: 1);
            var position = security.GetLastActiveForSignal("SE", barNumber: 1);
            Assert.IsNotNull(position);

            position.CloseAtMarket(barNumber: 2, "SXS");
            security.Update(barNumber: 2);
            var closedPosition = security.GetLastClosedShortPosition(barNumber: 2);
            Assert.IsNotNull(closedPosition);

            Assert.AreEqual(expected:position, actual:closedPosition);
        }

        [TestMethod()]
        public void OpenAndClosePosition_Test()
        {
            security.Update(barNumber: 0);
            security.SellIfLess(barNumber: 1, contracts: 1, entryPricePlanned: 89900,
                "SE", isConverted: false);

            security.Update(barNumber: 1);
            var position = security.GetLastActiveForSignal("SE", barNumber: 1);
            Assert.IsNotNull(position);

            position.CloseAtMarket(barNumber: 2, "SXS");
            security.Update(barNumber: 2);
            var closedPosition = security.GetLastClosedShortPosition(barNumber: 2);
            Assert.IsNotNull(closedPosition);

            Assert.AreEqual(expected: position, actual: closedPosition);
        }
    }
}