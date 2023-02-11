using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class TransactionTests
    {
        Balance balance;
        Account account;
        DateTime dateTime;

        [TestInitialize()]
        public void Init()
        {
            //real data
            balance = Balance.Create();
            account = Account.Create("Пятанов Иван Вадимович", balance);
            var dateTime = new DateTime(2023, 01, 25);
            balance.Update(dateTime, 2639157.23m);
            account.CreateTransaction(Operation.Deposit, 50000, dateTime);            
        }

        [TestMethod()]
        public void ToStringTest()
        {
            var exptected = "Transaction: 25.01.2023 0:00:00 Deposit 50000. 2639157,23 (balance before transaction).";
            var actual = account.Transactions.First().ToString();
            Assert.AreEqual(exptected, actual);
        }
    }
}