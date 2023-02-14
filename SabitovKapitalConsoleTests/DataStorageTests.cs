using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class DataStorageTests
    {
        Portfolio portfolio;
        Balance balance;
        Account account;
        DateTime dateTime;
        DataStorage dataStorage;

        [TestInitialize()]
        public void Init()
        {
            portfolio = Portfolio.Create();
            //real data
            balance = Balance.Create();
            account = Account.Create("Пятанов Иван Вадимович", balance);
            portfolio.AddAccount(account);
            dateTime = new DateTime(2023, 01, 25);
            balance.Update(dateTime, 2639157.23m);
            account.CreateTransaction(Operation.Deposit, 50000, dateTime);
            account.CreateTransaction(Operation.WithdrawProfit, 0, dateTime);

            dateTime = new DateTime(2023, 02, 07);
            balance.Update(dateTime, 2697205.27m);
            account = Account.Create("Сабитов Ильяс Ильдарович", balance);
            portfolio.AddAccount(account);
            account.CreateTransaction(Operation.WithdrawProfit, 10000, dateTime);

            dataStorage = DataStorage.Create();
        }

        [TestMethod()]
        public void SaveDataToFileTest()
        {
            var exptected = "Transaction: 25.01.2023 0:00:00 Deposit 50000. 2639157,23 " +
             "(balance before transaction). Пятанов Иван Вадимович\r\n" +
             "Transaction: 25.01.2023 0:00:00 WithdrawProfit 0. 2639157,23 (balance before " +
             "transaction). Пятанов Иван Вадимович\r\n\r\nTransaction: 07.02.2023 0:00:00 " +
             "WithdrawProfit 10000. 2697205,27 (balance before transaction). " +
             "Сабитов Ильяс Ильдарович\r\n\r\n";

            dataStorage.SaveDataToFile(portfolio);
            var actual = dataStorage.ReadFile();
            Assert.AreEqual(exptected, actual);
        }

        public void _LoadDataFromFileTest()
        {
            var exptected = "Transaction: 25.01.2023 0:00:00 Deposit 50000. 2639157,23 " +
             "(balance before transaction). Пятанов Иван Вадимович\r\n" +
             "Transaction: 25.01.2023 0:00:00 WithdrawProfit 0. 2639157,23 (balance before " +
             "transaction). Пятанов Иван Вадимович\r\n\r\nTransaction: 07.02.2023 0:00:00 " +
             "WithdrawProfit 10000. 2697205,27 (balance before transaction). " +
             "Сабитов Ильяс Ильдарович\r\n\r\n";

            var actual = portfolio.ToString();
            Assert.AreEqual(exptected, actual);
        }
    }
}