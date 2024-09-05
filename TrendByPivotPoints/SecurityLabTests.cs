using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradingSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script.Handlers;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class SecurityLabTests
    {
        [TestMethod()]
        public void UpdateTest()
        {
            var bars = new List<Bar>() {
                Bar.Create(new DateTime(2023, 11, 17), 90090, 91400, 89926, 91012),
                Bar.Create(new DateTime(2023, 11, 20), 91036, 91233, 88744, 89003),
                Bar.Create(new DateTime(2023, 11, 22), 90082, 90657, 88787, 89233),
                Bar.Create(new DateTime(2023, 11, 24), 89127, 90080, 88830, 89463),
                Bar.Create(new DateTime(2023, 11, 27), 89127, 90080, 88830, 89463)
            };

            var security = new SecurityLab("SecurityNameTest", Currency.Ruble, shares: 1,
                5000, 4500, bars);

            security.Update(barNumber: 0);
            security.SellIfLess(barNumber: 1, contracts: 1, entryPricePlanned: 89900,
                "SE", isConverted: false);

            security.Update(barNumber: 1);
            var position = security.GetLastActiveForSignal("SE", barNumber: 1);

            Assert.IsNotNull(position);
        }
    }
}