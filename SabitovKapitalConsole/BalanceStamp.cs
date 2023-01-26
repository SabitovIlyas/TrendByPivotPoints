// See https://aka.ms/new-console-template for more information


public class BalanceStamp
{
    public DateTime DateTime { get; private set; }
    public decimal Value { get; private set; }
    public static BalanceStamp Create(DateTime dateTime, decimal value)
    {
        return new BalanceStamp(dateTime, value);
    }

    private BalanceStamp(DateTime dateTime, decimal value)
    {
        DateTime = dateTime;
        Value = value;
    }
}
