// See https://aka.ms/new-console-template for more information
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
        transactions.Add(Transaction.Create(operation, value, dateTime));
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
        var sum = 0m;
        foreach (var transaction in transactions)
            sum = sum + transaction.GetDeposit();//TODO: доделать метод
        return sum;
    }
}
