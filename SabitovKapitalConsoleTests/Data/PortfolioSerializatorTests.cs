using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Data;
using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class PortfolioSerializatorTests
    {
        Balance balance;
        Transaction transaction;
        BalanceStamp balanceStamp;
        Account account;
        String serializedPortfolio;
        Portfolio portfolio;

        [TestInitialize()]
        public void Init()
        {
            balance = Balance.Create();
            portfolio = Portfolio.Create(balance);            
            serializedPortfolio = ReadFile();
            var accounts = new List<Account>()
            {
                Account.Create("Сабитов Ильдар Мифтахович", portfolio),
                Account.Create("Сабитов Ильяс Ильдарович", portfolio),
                Account.Create("Пятанов Иван Вадимович", portfolio),
                Account.Create("Елена", portfolio)
            };

            var dateTimeBalanceStamp = new DateTime(2023, 01, 25, 00, 00, 00);
            balanceStamp = BalanceStamp.Create(dateTimeBalanceStamp, value: 0.00m, balance);
            var dateTimeTransaction = dateTimeBalanceStamp;
            accounts[1].CreateTransaction(Operation.Deposit, 2276298.88m, dateTimeTransaction);

            balanceStamp = BalanceStamp.Create(dateTimeBalanceStamp, value: 2276298.88m, balance);
            accounts[0].CreateTransaction(Operation.Deposit, 362858.35m, dateTimeTransaction);

            balanceStamp = BalanceStamp.Create(dateTimeBalanceStamp, value: 2639157.23m, balance);
            accounts[2].CreateTransaction(Operation.Deposit, 50000.00m, dateTimeTransaction);

            balanceStamp = BalanceStamp.Create(dateTimeBalanceStamp, value: 2689157.23m, balance);
            dateTimeTransaction = new DateTime(2023, 01, 26, 00, 00, 00);            
            accounts[1].CreateTransaction(Operation.WithdrawProfit, 6034.95m, dateTimeTransaction); 

            dateTimeBalanceStamp = new DateTime(2023, 02, 07, 19, 51, 00);
            balanceStamp = BalanceStamp.Create(dateTimeBalanceStamp, value: 2697205.27m, balance);
            dateTimeTransaction = new DateTime(2023, 02, 08, 12, 05, 00);
            accounts[1].CreateTransaction(Operation.WithdrawProfit, 10000.00m, dateTimeTransaction);

            dateTimeBalanceStamp = new DateTime(2023, 02, 23, 01, 32, 00);
            balanceStamp = BalanceStamp.Create(dateTimeBalanceStamp, value: 2650746.92m, balance);
            dateTimeTransaction = new DateTime(2023, 02, 24, 12, 38, 00);
            accounts[1].CreateTransaction(Operation.WithdrawProfit, 10000.00m, dateTimeTransaction);

            dateTimeBalanceStamp = new DateTime(2023, 02, 24, 21, 00, 00);
            balanceStamp = BalanceStamp.Create(dateTimeBalanceStamp, value: 2738809.23m, balance);
            dateTimeTransaction = dateTimeBalanceStamp;
            accounts[1].CreateTransaction(Operation.WithdrawProfit, 7470.87m, dateTimeTransaction);

            dateTimeBalanceStamp = new DateTime(2023, 02, 24, 22, 00, 00);
            balanceStamp = BalanceStamp.Create(dateTimeBalanceStamp, value: 2731338.36m, balance);
            dateTimeTransaction = dateTimeBalanceStamp;
            accounts[3].CreateTransaction(Operation.Deposit, 7470.87m, dateTimeTransaction);
        }

        public string ReadFile()
        {
            var result = string.Empty;
            try
            {
                using (var sr = new StreamReader("SerializedPortfolio.txt"))
                    result = sr.ReadToEnd();
            }

            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return result;
        }

        [TestMethod()]
        public void SerializeTest()
        {
            var expected = serializedPortfolio;

            var portfolioSerializator = PortfolioSerializator.Create(portfolio);
            var actual = portfolioSerializator.Serialize();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DeserializeTest()
        {
            var portfolioSerializator = PortfolioSerializator.Create(serializedPortfolio);
            var actual = portfolioSerializator.Deserialize();
            Assert.AreEqual(expected: portfolio, actual);
        }
    }
}