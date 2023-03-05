using System.Text;

public class Account
{
    public static List<Account> Accounts { get; private set; } = new List<Account>();

    public string Name { get; private set; }
    public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public int Id { get; private set; }
    private Balance balance;    

    public static Account Create(string name, Balance balance)
    {
        var account = new Account(name, balance);
        Accounts.Add(account);
        return account;
    }

    private Account(string name, Balance balance)
    {
        Id = GetId();
        Name = name;
        this.balance = balance;
    }   

    private static int GetId()
    {
        if (Accounts.Count == 0)
            return 0;
        return Accounts.Last().Id + 1;
    }

    public static Account Create(string name, Balance balance, int id)
    {
        if (!IsThisIdAvailable(id))
            throw new ArgumentException("Такой Id уже используется");
        var account = new Account(name, balance, id);
        Accounts.Add(account);
        Accounts = SortAccounts();
        return account;
    }

    private Account(string name, Balance balance, int id)
    {
        Id = id;
        Name = name;
        this.balance = balance;
    }

    private static List<Account> SortAccounts()
    {
        return (from account in Accounts
                orderby account.Id
                select account).ToList();
    }

    private static bool IsThisIdAvailable(int id)
    {
        if (Accounts.Find(p => p.Id == id) == null)
            return true;
        return false;
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
        
        foreach(var transaction in Transactions)
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
        if (ReferenceEquals(this, obj))
            return true;
        return false;    
    }    
}