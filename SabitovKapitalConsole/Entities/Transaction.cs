public class Transaction
{
    public Operation Operation { get; private set; }
    public decimal Value { get; private set; }
    public decimal ValueWithSign
    {
        get
        {
            switch (Operation)
            {
                case Operation.Deposit:
                    return Value;
                case Operation.WithdrawProfit:
                    return -Value;
            }
            return 0;
        }
    }
    public DateTime DateTime { get; private set; }
    public BalanceStamp BalanceStampBeforeTransaction { get; private set; }
    public decimal BalanceBeforeTransaction => BalanceStampBeforeTransaction.Value;
    public decimal BalanceAfterTransaction => BalanceStampBeforeTransaction.Value + ValueWithSign;

    public static Transaction Create(Operation operation, decimal value, DateTime dateTime,
        BalanceStamp balanceStampBeforeTransaction)
    {
        return new Transaction(operation, value, dateTime, balanceStampBeforeTransaction);
    }

    private Transaction(Operation operation, decimal value, DateTime dateTime, 
        BalanceStamp balanceStampBeforeTransaction)
    {
        Operation = operation;
        Value = value;
        DateTime = dateTime;
        BalanceStampBeforeTransaction = balanceStampBeforeTransaction;
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

    public override string ToString()
    {
        return string.Format("Transaction: {0} {1} {2}. {3} (balance before transaction).",
            DateTime, Operation, Value, BalanceBeforeTransaction);        
    }
}
