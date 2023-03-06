using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework.Constraints;
using SabitovCapitalConsole.Data;
using SabitovCapitalConsole.Entities;
using System;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class BalanceStampSerializatorTests
    {
        [TestMethod()]
        public void SerializeTest()
        {
            var expected = "BalanceStamp;Id\t0;DateTime\t29.08.1997 2:14:00;Value\t1000000,00";
            var balance = Balance.Create();
            var dateTime = new DateTime(1997, 08, 29, 2, 14, 0);

            var balanceStamp = BalanceStamp.Create(dateTime, value: 1000000m, balance, id: 0);
            var balanceStampSerializator = BalanceStampSerializator.Create(balanceStamp);
            var actual = balanceStampSerializator.Serialize();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DeserializeTest()
        {
            var serializedBalanceStamp =
                "BalanceStamp;Id\t0;DateTime\t29.08.1997 2:14:00;Value\t1000000,00";
            var balance = Balance.Create();
            var dateTime = new DateTime(1997, 08, 29, 2, 14, 0);
            var expected = BalanceStamp.Create(dateTime, value: 1000000m, balance, id: 0);

            var anotherBalance = Balance.Create();
            var balanceStamplSerializator =
                BalanceStampSerializator.Create(serializedBalanceStamp, anotherBalance);
            var actual = balanceStamplSerializator.Deserialize();

            Assert.AreEqual(expected, actual);
        }
    }
}