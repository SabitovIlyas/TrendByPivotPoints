namespace SabitovCapitalConsole.Entities
{
    public class Account
    {
        public string Name { get; private set; }
        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public int Id { get; private set; }
        private Balance balance;

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
        }

        public static Account Create(string name, Portfolio portfolio, int id)
        {
            if (!portfolio.IsThisIdAvailable(id))
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
        }

        public void CreateTransaction(Operation operation, decimal value, DateTime dateTime)
        {
            var balanceStampBeforeTransaction = balance.GetCurrentBalanceStamp();
            Transactions.Add(Transaction.Create(operation, value, dateTime,
                balanceStampBeforeTransaction));
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
            var share = 0m;

            foreach (var transaction in Transactions)
            {
                var moneyBeforeTransaction = share * transaction.BalanceBeforeTransaction;
                share = (moneyBeforeTransaction + transaction.ValueWithSign) /
                    transaction.BalanceAfterTransaction;
            }

            var money = share * balance.GetCurrentBalance();
            //var money = share * transactions.Last().BalanceAfterTransaction;
            return money - GetDeposit();
        }

        public override string ToString()
        {
            var result = string.Empty;
            foreach (var transaction in Transactions)
                result += string.Format("{0} {1}\r\n", transaction, Name);

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
    }
}