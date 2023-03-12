using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Entities;

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
            var expected = -10000m;// -10 000
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
        public void GetProfitTest4()
        {
            var expected = -10000m;
            balance = Balance.Create();
            portfolio = Portfolio.Create(balance);
            var account1 = Account.Create("Сабитов Ильяс Ильдарович", portfolio);
            var account2 = Account.Create("Пятанов Иван Вадимович", portfolio);
            GetProfitTestHelperCase4(account1, account2);
            
            var actual = account2.GetProfit();
            var delta = Math.Abs(expected - actual);
            if (delta >= 0.01m)
                Assert.Fail();

            expected = -90000m;
            actual = account1.GetProfit();
            delta = Math.Abs(expected - actual);
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

        private void GetProfitTestHelperCase4(Account account1, Account account2)
        {   
            dateTime = new DateTime(2022, 12, 01);
            balance.Update(dateTime, value: 0m);    //0
            account1.CreateTransaction(Operation.Deposit, 900000m, dateTime);    //900 000

            dateTime = new DateTime(2023, 01, 01);
            balance.Update(dateTime, value: 900000m);    //900 000

            account2.CreateTransaction(Operation.Deposit, 100000m, dateTime);    //100 000
            dateTime = new DateTime(2023, 02, 01);
            balance.Update(dateTime, value: 900000m);     //900 000
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
        public void ToStringTest()
        {
            account.CreateTransaction(Operation.WithdrawProfit, 0, dateTime);

            var expected = "0) Пятанов Иван Вадимович";

            var actual = account.ToString();
            Assert.AreEqual(expected, actual);
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

        [TestMethod()]
        public void CreateTest_GetCorrectId()
        {
            Assert.AreEqual(expected: 0, actual: account.Id);
            account = Account.Create("Сабитов Ильяс Ильдарович", portfolio);
            Assert.AreEqual(expected: 1, actual: account.Id);
        }
    }
}