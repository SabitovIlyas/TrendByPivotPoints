using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class AccountTests
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
            account.CreateTransaction(Operation.Deposit, 50000, dateTime);
            balance.Update(dateTime, 1344578.62m);
        }

        [TestMethod()]
        public void GetDepositTest1()
        {
            var expected = 50000;
            var actual= account.GetDeposit();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDepositTest2()
        {
            var expected = 400000;
            GetProfitTestHelperCase3();
            var actual = account.GetDeposit();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetProfitTest() 
        {
            var expected = -10000m;
            GetProfitTestHelperCase1();
            var actual = account.GetProfit();            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetProfitTest2()
        {
            var expected = 87500m;
            GetProfitTestHelperCase2();
            var actual = account.GetProfit();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetProfitTest3()
        {
            var expected = 0m;
            GetProfitTestHelperCase3();
            var actual = account.GetProfit();
            var delta = Math.Abs(expected - actual);
            if (delta >= 0.01m)
                Assert.Fail();            
        }

        [TestMethod()]
        public void GetBalanceTest()
        {
            var expected = 75m;
            var balance = GetBalanceTestHelper();
            var actual = balance.GetCurrentBalance();
            Assert.AreEqual(expected, actual);
        }

        private Balance GetBalanceTestHelper()
        {
            balance = Balance.Create();
            balance.Update(new DateTime(2023, 01, 25), 100m);
            balance.Update(new DateTime(2023, 01, 26), 75m);
            balance.Update(new DateTime(2023, 01, 24), 50m);
            return balance;
        }

        private void GetProfitTestHelperCase1()
        {
            balance = Balance.Create();
            account = Account.Create("Пятанов Иван Вадимович", balance);
            dateTime = new DateTime(2023, 01, 01);
            balance.Update(dateTime, balance: 900000m);    //900 000

            account.CreateTransaction(Operation.Deposit, 100000m, dateTime);    //100 000
            dateTime = new DateTime(2023, 02, 01);
            balance.Update(dateTime, balance: 900000m);     //900 000
        }

        private void GetProfitTestHelperCase2()
        {
            GetProfitTestHelperCase1();
            account.CreateTransaction(Operation.Deposit, 300000m, dateTime);    //300 000
            dateTime = new DateTime(2023, 03, 01);
            balance.Update(dateTime, balance: 1500000m);     //1 500 000
        }

        private void GetProfitTestHelperCase3()
        {
            GetProfitTestHelperCase2();
            account.CreateTransaction(Operation.WithdrawProfit, 87500m, dateTime);    //300 000
            balance.Update(dateTime, balance: 1412500m);    //1 412 500
        }

        [TestCleanup]
        public void Cleanup() { }


    }
}