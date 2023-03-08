using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Data;
using SabitovCapitalConsole.Entities;
using System;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class TransactionSerializatorTests
    {
        Balance balance;
        Transaction transaction;
        BalanceStamp balanceStamp;
        Account account;
        String serializedTransaction;
        Portfolio portfolio;

        [TestInitialize()]
        public void Init()
        {
            balance = Balance.Create();
            portfolio = Portfolio.Create(balance);
            var dateTime = new DateTime(2023, 01, 25, 0, 0, 0);
            balanceStamp = BalanceStamp.Create(dateTime, value: 2639157.23m, balance, id: 0);
            account = Account.Create("Пятанов Иван Вадимович", portfolio, id: 0);
            transaction = Transaction.Create(Operation.Deposit, value: 50000, dateTime, balanceStamp, account);
            serializedTransaction = "Transaction;Id\t0;DateTime\t25.01.2023 0:00:00;Operation\tDeposit;" +
                "Value\t50000,00;BalanceStampId\t0;AccountId\t0";
        }

        [TestMethod()]
        public void SerializeTest()
        {
            var expected = serializedTransaction;
            var transactionSerializator = TransactionSerializator.Create(transaction, balanceStamp, account);
            var actual = transactionSerializator.Serialize();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DeserializeTest()
        {
            account.RemoveTransaction(transaction);
            var transactionSerializator = TransactionSerializator.Create(serializedTransaction, balance,
                portfolio);
            var actual = transactionSerializator.Deserialize();
            Assert.AreEqual(expected:transaction, actual);
        }
    }
}