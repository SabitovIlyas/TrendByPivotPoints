using SabitovCapitalConsole.Entities;
using System.Runtime.InteropServices;

namespace SabitovCapitalConsole.Data
{
    public class PortfolioSerializator : Serializator
    {
        Portfolio portfolio;
        string serializedPortfolio;
        public static PortfolioSerializator Create(Portfolio portfolio)
        {
            return new PortfolioSerializator(portfolio);
        }

        private PortfolioSerializator(Portfolio portfolio)
        {
            this.portfolio = portfolio;
        }

        public static PortfolioSerializator Create(string serializedPortfolio)
        {
            return new PortfolioSerializator(serializedPortfolio);
        }

        private PortfolioSerializator(string serializedPortfolio)
        {
            this.serializedPortfolio = serializedPortfolio;
        }

        public object Deserialize()
        {
            if (serializedPortfolio == null)
                throw new NullReferenceException("Строка с сериализованным объектом " +
                    "не определена.");

            if (serializedPortfolio == string.Empty)
                throw new InvalidDataException("Строка с сериализованным объектом пуста.");

            try
            {
                var info = serializedPortfolio.Split("\r\n");
                var lines = ConvertToListOfLinesWithoutEmptyLines(info);

                var accountLines = GetSpecifiedLines(lines, "Account");
                var balanceStampLines = GetSpecifiedLines(lines, "BalanceStamp");
                var transactionLines = GetSpecifiedLines(lines, "Transaction");

                var balance = DeserializeBalance(balanceStampLines);
                var portfolio = Portfolio.Create(balance);
                DeserializeAccounts(accountLines, portfolio);                
                DeserializeTransactions(transactionLines, balance, portfolio);

                return portfolio;
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Не удалось десериализовать портфель: " + e.Message);
            }
        }

        private List<string> ConvertToListOfLinesWithoutEmptyLines(string[] lines)
        {
            var result = new List<string>();
            foreach (var line in lines)
                if (line != string.Empty)
                    result.Add(line);

            return result;
        }

        private List<string> GetSpecifiedLines(List<string> lines, string specifiedParameter)
        {
            return (from line in lines
                    where line.Split(';')[0].Contains(specifiedParameter)
                    select line).ToList();
        }

        private Balance DeserializeBalance(List<string> balanceStampLines)
        {
            var serializedBalance = string.Empty;
            foreach (var line in balanceStampLines)
                serializedBalance += line + "\r\n";
            var balanceSerializator = BalanceSerializator.Create(serializedBalance);
            return (Balance)balanceSerializator.Deserialize();
        }

        private void DeserializeAccounts(List<string> accountLines, Portfolio portfolio)
        {            
            var accountSerializator = AccountSerializator.Create();
            foreach (var line in accountLines)
            {
                accountSerializator.SerializedAccount = line;
                accountSerializator.Portfolio = portfolio;
                accountSerializator.Deserialize();                
            }
        }

        private void DeserializeTransactions(List<string> transactionLines, Balance balance,
            Portfolio portfolio)
        {
            var transactionSerializator = TransactionSerializator.Create();

            foreach (var line in transactionLines)
            {
                transactionSerializator.SerializedTransaction = line;
                transactionSerializator.Balance = balance;
                transactionSerializator.Portfolio = portfolio;
                transactionSerializator.Deserialize();
            }
        }

        public string Serialize()
        {
            var result = string.Empty;
            var accountSerializator = AccountSerializator.Create();
            foreach (var account in portfolio.Accounts)
            {
                accountSerializator.Account = account;
                result += accountSerializator.Serialize() + "\r\n";
            }
            result += "\r\n";

            var balanceSerializator = BalanceSerializator.Create(portfolio.Balance);
            result += balanceSerializator.Serialize() + "\r\n";

            var transactionSerializator = TransactionSerializator.Create();
            
            foreach (var transaction in portfolio.GetAllAccountsTransactions())
            {
                transactionSerializator.Transaction = transaction;                
                transactionSerializator.BalanceStamp = transaction.BalanceStampBeforeTransaction;
                transactionSerializator.Account = transaction.Account;
                result += transactionSerializator.Serialize() + "\r\n";
            }            
             
            return result;
        }       
    }
}