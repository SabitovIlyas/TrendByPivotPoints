// See https://aka.ms/new-console-template for more information
public class Transaction
{
    public Operation Operation { get; private set; }
    public decimal Value { get; private set; }
    public DateTime DateTime { get; private set; }

    public static Transaction Create(Operation operation, decimal value, DateTime dateTime)
    {
        return new Transaction(operation, value, dateTime);
    }

    private Transaction(Operation operation, decimal value, DateTime dateTime)
    {
        Operation = operation;
        Value = value;
        DateTime = dateTime;
    }
    
    public decimal GetDeposit()
    {
        switch (Operation)
        {
            case Operation.Deposit:
                return Value;
            case Operation.WithdrawDeposit:
                return -Value;            
        }
        return 0;
    }

    public decimal GetProfit()
    {
        switch (Operation)
        {
            case Operation.WithdrawProfit:
                return Value;
            case Operation.WithdrawDeposit:
                return -Value;
        }
        return 0;
    }
}
