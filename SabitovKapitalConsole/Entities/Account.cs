using System.Data.Common;
using System.Transactions;

namespace SabitovCapitalConsole.Entities
{
    public class Account
    {
        public string Name { get; private set; }
        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public int Id { get; private set; }
        public decimal Share { get; set; }
        private Balance balance;
        private Portfolio portfolio;
        public bool isClosed = false;
        
        public static Account Create(string name, Portfolio portfolio)
        {
            var account = new Account(name, portfolio);
            portfolio.AddAccount(account);
            return account;
        }

        private Account(string name, Portfolio portfolio)
        {
            Id = portfolio.GetAccountId();
            Name = name;
            balance = portfolio.Balance;
            this.portfolio = portfolio;
        }

        public static Account Create(string name, Portfolio portfolio, int id)
        {
            if (!portfolio.IsThisAccountIdAvailable(id))
                throw new ArgumentException("Такой Id уже используется");

            var account = new Account(name, portfolio, id);
            portfolio.AddAccount(account);
            return account;
        }

        private Account(string name, Portfolio portfolio, int id)
        {
            Id = id;
            Name = name;
            balance = portfolio.Balance;
            this.portfolio = portfolio;
        }

        public void CloseAccount(DateTime dateTime)
        {
            var totalBalance = GetDeposit() + GetProfit();
            CreateTransaction(Operation.CloseAccount, totalBalance, dateTime);
        }

        public void CreateTransaction(Operation operation, decimal value, DateTime dateTime)
        {
            var balanceStampBeforeTransaction = balance.GetCurrentBalanceStamp();
            Transactions.Add(Transaction.Create(operation, value, dateTime,
                balanceStampBeforeTransaction, this));

            portfolio.RecalcSharesForAllAccounts(this);
        }

        public decimal GetDeposit()
        {
            var sum = 0m;
            foreach (var transaction in Transactions)
                sum = sum + transaction.GetDeposit();
            return sum;
        }

        public decimal GetProfit()
        {
            //var share = 0m;

            /*foreach (var transaction in Transactions)
            {
                var moneyBeforeTransaction = share * transaction.BalanceBeforeTransaction;
                share = (moneyBeforeTransaction + transaction.ValueWithSign) /
                    transaction.BalanceAfterTransaction;
            }*/

            var money = Share * balance.GetCurrentBalance();
            //var money = share * transactions.Last().BalanceAfterTransaction;
            return money - GetDeposit();
        }

        public override string ToString()
        {
            var result = string.Empty;
            //foreach (var transaction in Transactions)
            //    result += string.Format("{0}\r\n", transaction);

            result = string.Format("{0}) {1}", Id, Name);
            return result;
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj.GetType() == GetType())
            {
                var account = obj as Account;
                if (Id == account.Id && Name == account.Name)
                    return true;
            }

            return false;
        }

        public int GetTransactionId()
        {
            return portfolio.GetTransactionId();
        }

        public int GetLastId()
        {
            if (Transactions.Count == 0) 
                return -1;
            return Transactions.Max(p => p.Id);
        }

        public void AddTransaction(Transaction transaction)
        {
            Transactions.Add(transaction);
            Transactions = SortTransactions();
        }

        public void RemoveTransaction(Transaction transaction)
        {
            Transactions.Remove(transaction);
        }

        private List<Transaction> SortTransactions()
        {
            return (from transaction in Transactions
                    orderby transaction.Id
                    select transaction).ToList();
        }

        public bool IsThisTransactionIdAvailable(int id)
        {
            return portfolio.IsThisTransactionIdAvailable(id);            
        }
    }
}