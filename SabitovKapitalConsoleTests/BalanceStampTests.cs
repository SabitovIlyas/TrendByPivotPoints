using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class BalanceStampTests
    {
        [TestMethod()]
        public void CreateTest_GetCorrectId()
        {
            var balance = Balance.Create();
            var dateTime = new DateTime(2023, 01, 25, 0, 0, 0);
            var balanceStamp = BalanceStamp.Create(dateTime, value: 100000, balance);
            Assert.AreEqual(expected: 0, balanceStamp.Id);

            balanceStamp = BalanceStamp.Create(dateTime, value: 50000, balance);
            Assert.AreEqual(expected: 1, balanceStamp.Id);
        }
    }
}