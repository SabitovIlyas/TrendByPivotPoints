using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class TransactionTests
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
            var dateTime = new DateTime(2023, 01, 25);
            balance.Update(dateTime, 2639157.23m);
            account.CreateTransaction(Operation.Deposit, 50000, dateTime);
        }

        [TestMethod()]
        public void ToStringTest()
        {
            var exptected = "Transaction: 25.01.2023 0:00:00 Deposit 50000. 2639157,23 (balance before transaction). " +
                "Пятанов Иван Вадимович";
            var actual = account.Transactions.First().ToString();
            Assert.AreEqual(exptected, actual);
        }

        [TestMethod()]
        public void CreateTest_GetCorrectId()
        { 
            var balance = Balance.Create();            
            var dateTime = new DateTime(2023, 01, 25, 0, 0, 0);
            var balanceStamp = BalanceStamp.Create(dateTime, value: 0m, balance);
            var portfolio = Portfolio.Create(balance);
            var account = Account.Create("Иванов Иван Иванович", portfolio);

            var transaction = Transaction.Create(Operation.Deposit, value: 50000m, dateTime,
                balanceStamp, account);
            
            Assert.AreEqual(expected:0, actual: transaction.Id);
            transaction = Transaction.Create(Operation.WithdrawDeposit, value: 50000m, dateTime,
                balanceStamp, account);

            Assert.AreEqual(expected: 1, actual: transaction.Id);
        }    
    }
}