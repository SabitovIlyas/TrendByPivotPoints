using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Data;
using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class BalanceSerializatorTests
    {
        [TestMethod()]
        public void SerializeTest()
        {
            var expected = "BalanceStamp;Id\t0;DateTime\t29.08.1997 2:14:00;Value\t1000000,00" +
                "\r\nBalanceStamp;Id\t1;DateTime\t01.01.2029 0:00:00;Value\t1000000000,00\r\n";
            var balance = Balance.Create();
            var balanceSerializator = BalanceSerializator.Create(balance);
            //"BalanceStamp;Id\t0;DateTime\t29.08.1997 2:14:00;Value\t1000000,00"
            var dateTime = new DateTime(1997, 08, 29, 2, 14, 0);
            balance.Update(dateTime, value: 1000000m);
            dateTime = new DateTime(2029, 01, 01, 0, 0, 0);
            balance.Update(dateTime, value: 1000000000m);
            var actual = balanceSerializator.Serialize();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DeserializeTest()
        {
            var balance = Balance.Create();            
            var dateTime = new DateTime(1997, 08, 29, 2, 14, 0);
            balance.Update(dateTime, value: 1000000m);
            dateTime = new DateTime(2029, 01, 01, 0, 0, 0);
            balance.Update(dateTime, value: 1000000000m);
            var expected = balance;

            var serializedBalance = "BalanceStamp;Id\t0;DateTime\t29.08.1997 2:14:00;Value\t1000000,00" +
                "\r\nBalanceStamp;Id\t1;DateTime\t01.01.2029 0:00:00;Value\t1000000000,00\r\n";

            var balanceSerializator = BalanceSerializator.Create(serializedBalance);
            var actual = (Balance)balanceSerializator.Deserialize();

            Assert.AreEqual(expected, actual);
        }
    }
}