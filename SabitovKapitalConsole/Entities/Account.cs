using System.Text;

public class Account
{
    public string Name { get; private set; }
    public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
    private Balance balance;

    public static Account Create(string name, Balance balance)
    {
        return new Account(name, balance);
    }

    private Account(string name, Balance balance)
    {
        Name = name;
        this.balance = balance;
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
}