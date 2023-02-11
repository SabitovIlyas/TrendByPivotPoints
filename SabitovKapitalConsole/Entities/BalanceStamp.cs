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

    public override string ToString()
    {
        return string.Format("Balance Stamp: {0} {1}", DateTime, Value);
    }
}
