namespace SabitovCapitalConsole.Entities
{
    public class Portfolio
    {
        public List<Account> Accounts { get; private set; } = new List<Account>();
        public Balance Balance { get; private set; }

        public static Portfolio Create(Balance balance)
        {
            return new Portfolio(balance);
        }

        private Portfolio(Balance balance) 
        {
            Balance = balance;
        }

        public void AddAccount(Account account)
        {
            Accounts.Add(account);
            Accounts = SortAccounts();
        }

        public void RemoveAccount(Account account)
        {
            Accounts.Remove(account);          
        }

        public override string ToString()
        {
            var result = string.Empty;
            foreach (var account in Accounts)
                result += string.Format("{0}\r\n", account);

            return result;
        }

        public bool IsThisAccountIdAvailable(int id)
        {
            if (Accounts.Find(p => p.Id == id) == null)
                return true;
            return false;
        }

        private List<Account> SortAccounts()
        {
            return (from account in Accounts
                    orderby account.Id
                    select account).ToList();
        }

        public int GetAccountId()
        {
            if (Accounts.Count == 0)
                return 0;
            return Accounts.Last().Id + 1;
        }

        public bool IsThisTransactionIdAvailable(int id)
        {
            var isAvailable = true;

            foreach (var account in Accounts)            
                if (account.Transactions.Find(p => p.Id == id) != null)
                    return false;            
            
            return isAvailable;
        }

        public int GetTransactionId()
        {
            var max = -1;
            foreach(var account in Accounts)
            {               
                var accountMaxId = account.GetLastId();
                if (accountMaxId > max)
                    max = accountMaxId;                
            }

            return max + 1;
        }

        internal Account GetAccountById(int id)
        {
            var account = Accounts.Find(x => x.Id == id);
            if (account == null)
                throw new NullReferenceException("Аккаунта с таким ID не существует");
            return account;
        }

        public List<Transaction> GetAllAccountsTransactions()
        {
            var allAccountsTransactions = new List<Transaction>();
            foreach (var account in Accounts)
                foreach (var transaction in account.Transactions)
                    allAccountsTransactions.Add(transaction);

            return SortTransactions(allAccountsTransactions);
        }

        private List<Transaction> SortTransactions(List<Transaction> transactions)
        {
            return (from transaction in transactions
                    orderby transaction.Id
                    select transaction).ToList();
        }

        public void RecalcSharesForAllAccounts(Account account)
        {
            var transaction = account.Transactions.Last();
         
            foreach (var acc in Accounts)
            {
                decimal accountBalanceBeforeTransaction = acc.Share * transaction.BalanceBeforeTransaction;
                if (acc == account)                
                    accountBalanceBeforeTransaction += transaction.ValueWithSign; 

                acc.Share = accountBalanceBeforeTransaction / transaction.BalanceAfterTransaction;               
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var portfolio = obj as Portfolio;
            if (Accounts.Count != portfolio.Accounts.Count)
                return false;

            for (var i = 0; i < Accounts.Count; i++)
                if (!Accounts[i].Equals(portfolio.Accounts[i]))
                    return false;

            if (!Balance.Equals(portfolio.Balance))
                return false;

            var thisAllAccountsTransactions = GetAllAccountsTransactions();
            var objAllAccountsTransactions = portfolio.GetAllAccountsTransactions();

            if (thisAllAccountsTransactions.Count != objAllAccountsTransactions.Count)
                return false;

            for (var i = 0; i < thisAllAccountsTransactions.Count; i++)
                if (!thisAllAccountsTransactions[i].Equals(objAllAccountsTransactions[i]))
                    return false;

            return true;
        }
    }
}