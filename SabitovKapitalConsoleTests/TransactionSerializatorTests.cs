using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SabitovCapitalConsoleTests
{
    [TestClass()]
    public class TransactionSerializatorTests
    {
        //[TestMethod()]
        public void SerializeTest()
        {
            var expected = "Id:1;Date:25.01.2023 0:00:00;Operation:Deposit;Value:50000;" +
                "BalanceStampId:1;AccountId:1";
            //var transaction = Transaction.Create();
            //var actual =

            //    Assert.AreEqual(expected, actual);
            Assert.Fail();
        }
    }
}