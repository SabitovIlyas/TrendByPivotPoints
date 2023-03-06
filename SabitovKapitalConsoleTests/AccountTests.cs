using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework.Constraints;
using SabitovCapitalConsole.Entities;
using System;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class AccountTests
    {
        Balance balance;
        Portfolio portfolio;
        Account account;
        DateTime dateTime;

        [TestInitialize()]
        public void Init()
        {
            //real data
            balance = Balance.Create();
            portfolio = Portfolio.Create(balance);
            account = Account.Create("Пятанов Иван Вадимович", portfolio);
            dateTime = new DateTime(2023, 01, 25);
            balance.Update(dateTime, 2639157.23m);
            account.CreateTransaction(Operation.Deposit, 50000, dateTime);

            //dateTime = new DateTime(2023, 02, 07);
            //balance.Update(dateTime, 2697205.27m);
            //account = Account.Create("Сабитов Ильяс Ильдарович", balance);
            //account.CreateTransaction(Operation.WithdrawProfit, 10000, dateTime);
        }

        [TestMethod()]
        public void GetDepositTest1()
        {
            var expected = 50000;
            var actual = account.GetDeposit();
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
            portfolio = Portfolio.Create(balance);
            account = Account.Create("Пятанов Иван Вадимович", portfolio);
            dateTime = new DateTime(2023, 01, 01);
            balance.Update(dateTime, value: 900000m);    //900 000

            account.CreateTransaction(Operation.Deposit, 100000m, dateTime);    //100 000
            dateTime = new DateTime(2023, 02, 01);
            balance.Update(dateTime, value: 900000m);     //900 000
        }

        private void GetProfitTestHelperCase2()
        {
            GetProfitTestHelperCase1();
            account.CreateTransaction(Operation.Deposit, 300000m, dateTime);    //300 000
            dateTime = new DateTime(2023, 03, 01);
            balance.Update(dateTime, value: 1500000m);     //1 500 000
        }

        private void GetProfitTestHelperCase3()
        {
            GetProfitTestHelperCase2();
            account.CreateTransaction(Operation.WithdrawProfit, 87500m, dateTime);    //300 000
            balance.Update(dateTime, value: 1412500m);    //1 412 500
        }

        [TestMethod()]
        public void LinqWhereTest()
        {
            var expected = 5;
            var list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var subList = list.Where(p => p <= 5).ToList();
            var actual = subList.Count;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void SelectWhereTest()
        {
            var expected = 5;
            var list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var subList = list.Select(p => p <= 5).ToList();
            var actual = subList.Where(p => p == true).Count();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void WriteReadTransactionToFileTest()
        {
            var expected = "";
            WriteTransactionToFile();
            var actual = ReadTransactionFromFile();
            Assert.AreEqual(expected, actual);
        }

        public void WriteTransactionToFile()
        {
            StreamWriter writer = new StreamWriter("SabitovCapitalData.txt", false);
            //account.

            //account.
            //writer.WriteLine("<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>");
            //foreach (var bar in spread.Bars)
            //    writer.WriteLine(bar.ToString());
            writer.Close();
        }

        public string ReadTransactionFromFile()
        {
            return "";
        }

        [TestMethod()]
        public void ToStringTest()
        {
            //var exptected = "Transaction: 25.01.2023 0:00:00 Deposit 50000. 2639157,23 " +
            //    "(balance before transaction). Пятанов Иван Вадимович\r\n" +
            //    "Transaction: 07.02.2023 0:00:00 WithdrawProfit 10000. 2697205,23 (balance " +
            //    "before transaction). Сабитов Ильяс Ильдарович\r\n";

            account.CreateTransaction(Operation.WithdrawProfit, 0, dateTime);

            var exptected = "Transaction: 25.01.2023 0:00:00 Deposit 50000. 2639157,23 " +
             "(balance before transaction). Пятанов Иван Вадимович\r\n" +
             "Transaction: 25.01.2023 0:00:00 WithdrawProfit 0. 2639157,23 (balance before " +
             "transaction). Пятанов Иван Вадимович\r\n";

            var actual = account.ToString();
            Assert.AreEqual(exptected, actual);
        }

        [TestMethod()]
        public void SortAccountsTest()
        {
            var balance = Balance.Create();
            var portfolio = Portfolio.Create(balance);
            var account1 = Account.Create("Сабитов Ильяс Ильдарович", portfolio, 2);
            var account2 = Account.Create("Пятанов Иван Вадимович", portfolio, 3);
            var account3 = Account.Create("Ати", portfolio, 1);

            var expected = new List<Account>() { account3, account1, account2 };
            var actual = portfolio.Accounts;

            if (expected.Count != actual.Count)
                Assert.Fail();

            for (var i = 0; i < expected.Count; i++)
                if (expected[i] != actual[i])
                    Assert.Fail();
        }
    }
}