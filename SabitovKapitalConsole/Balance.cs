// See https://aka.ms/new-console-template for more information


public class Balance
{
    private List<BalanceStamp> balances = new List<BalanceStamp>();

    public static Balance Create()
    {
        return new Balance();
    }

    private Balance() { }

    public void Update(DateTime dateTime, decimal value)
    {
        var balanceStamp = BalanceStamp.Create(dateTime, value);
        balances.Add(balanceStamp);
        balances =
            (from n in balances
             orderby n.DateTime
             select n).ToList();
    }

    public decimal GetCurrentBalance()
    {
        if (balances.Count == 0)
            return 0;
        return balances.Last().Value;
    }
}
