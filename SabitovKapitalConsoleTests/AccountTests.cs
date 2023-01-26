using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class AccountTests
    {
        Balance balance;
        Account account; 

        [TestInitialize()]
        public void Init()
        {
            balance = Balance.Create();
            account = Account.Create("Пятанов Иван Вадимович", balance);
            var dateTime = new DateTime(2023, 01, 25);
            account.CreateTransaction(Operation.Deposit, 50000, dateTime);
            balance.Update(dateTime, 1344578.62m);
        }

        [TestMethod()]
        public void GetDepositTest()
        {
            var expected = 50000;
            var actual= account.GetDeposit();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetProfitTest() 
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetBalanceTest()
        {
            Assert.Fail();
        }

        [TestCleanup]
        public void Cleanup() { }

    }
}