public class Account
{
    public string Name { get; private set; }
    private List<Transaction> transactions = new List<Transaction>();
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
        transactions.Add(Transaction.Create(operation, value, dateTime,
            balanceStampBeforeTransaction));
    }

    public decimal GetDeposit()
    {
        var sum = 0m;
        foreach (var transaction in transactions)
            sum = sum + transaction.GetDeposit();
        return sum;
    }    

    public decimal GetProfit()
    {
        var share = 0m;
        
        foreach(var transaction in transactions)
        {
            var moneyBeforeTransaction = share * transaction.BalanceBeforeTransaction;
            share = (moneyBeforeTransaction + transaction.ValueWithSign) /
                transaction.BalanceAfterTransaction;            
        }

        var money = share * balance.GetCurrentBalance();
        //var money = share * transactions.Last().BalanceAfterTransaction;
        return money - GetDeposit();
    }    
}