using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradingSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script.Handlers;
using TSLab.DataSource;
using TSLab.Script;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class SecurityLabTests
    {
        List<Bar> bars;
        SecurityLab security;

        [TestInitialize]
        public void TestInitialize()
        {
            bars = new List<Bar>() {
                Bar.Create(new DateTime(2023, 11, 17), 90090, 91400, 89926, 91012),
                Bar.Create(new DateTime(2023, 11, 20), 91036, 91233, 88744, 89003),
                Bar.Create(new DateTime(2023, 11, 22), 90082, 90657, 88787, 89233),
                Bar.Create(new DateTime(2023, 11, 24), 89127, 90080, 88830, 89463),
                Bar.Create(new DateTime(2023, 11, 27), 89127, 90080, 88830, 89463)
            };

            security = new SecurityLab("SecurityNameTest", Currency.Ruble, shares: 1,
                5000, 4500, bars);
        }

        [TestMethod()]
        public void UpdateTest()
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
                "SE", isConverted: false);
            security.Update(barNumber: 1);
            security.SellIfLess(barNumber: 2, contracts: 1, entryPricePlanned: 87000,
                "SE", isConverted: false);
            security.Update(barNumber: 2);
            security.SellIfLess(barNumber: 3, contracts: 1, entryPricePlanned: 86000,
                "SE", isConverted: false);
            security.Update(barNumber: 3);
            security.SellIfLess(barNumber: 4, contracts: 1, entryPricePlanned: 85000,
                "SE", isConverted: false);
            security.Update(barNumber: 4);            

            var orders = security.GetActiveOrders(barNumber: 3);
                        
            Assert.AreEqual(expected: 3, actual: orders.Count);
            Assert.AreEqual(expected: 88000, actual: orders[0].Price);
            Assert.AreEqual(expected: 87000, actual: orders[1].Price);
            Assert.AreEqual(expected: 86000, actual: orders[2].Price);
        }
    }
}