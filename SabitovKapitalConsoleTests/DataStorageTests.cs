using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Entities;
using System.Transactions;
using SabitovCapitalConsole.Data;

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
            //real data
            balance = Balance.Create();
            portfolio = Portfolio.Create(balance);
            account = Account.Create("Пятанов Иван Вадимович", portfolio);
            dateTime = new DateTime(2023, 01, 25);
            balance.Update(dateTime, 2639157.23m);
            account.CreateTransaction(Operation.Deposit, 50000, dateTime);
            account.CreateTransaction(Operation.WithdrawProfit, 0, dateTime);

            dateTime = new DateTime(2023, 02, 07);
            balance.Update(dateTime, 2697205.27m);
            account = Account.Create("Сабитов Ильяс Ильдарович", portfolio);
            account.CreateTransaction(Operation.WithdrawProfit, 10000, dateTime);

            dataStorage = DataStorage.Create("SabitovCapitalDataBase.txt");
            dataStorage.SaveDataToFile(portfolio);
        }

        [TestMethod()]
        public void SaveDataToFileTest()
        {
            var exptected = "0) Пятанов Иван Вадимович\r\n1) Сабитов Ильяс Ильдарович\r\n";

            var actual = dataStorage.ReadFile();
            Assert.AreEqual(exptected, actual);
        }

        //[TestMethod()]
        public void LoadDataFromFileTest()
        {
            var expected = portfolio;
            var actual = dataStorage.LoadDataFromFile();

            Assert.AreEqual(expected, actual);
        }        
    }
}