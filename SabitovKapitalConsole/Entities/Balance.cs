public class Balance
{
    private List<BalanceStamp> balances = new List<BalanceStamp>();

    public static Balance Create()
    {
        return new Balance();
    }

    private Balance() { }

    public void Update(DateTime dateTime, decimal balance)
    {
        var balanceStamp = BalanceStamp.Create(dateTime, balance);
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
    public BalanceStamp GetCurrentBalanceStamp()
    {
        if (balances.Count == 0)
            return null;    //TODO: Не возвращать null
        return balances.Last();
    }
}
