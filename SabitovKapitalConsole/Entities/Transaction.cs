namespace SabitovCapitalConsole.Entities
{
    public class Transaction
    {
        public int Id { get; private set; }
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
        public Account Account { get; private set; }

        public static Transaction Create(Operation operation, decimal value, DateTime dateTime,
            BalanceStamp balanceStampBeforeTransaction, Account account)
        {
            var transaction = new Transaction(operation, value, dateTime, balanceStampBeforeTransaction,
                account);
            account.AddTransaction(transaction);
            return transaction;
        }

        private Transaction(Operation operation, decimal value, DateTime dateTime,
            BalanceStamp balanceStampBeforeTransaction, Account account)
        {
            Id = account.GetTransactionId();
            Operation = operation;
            Value = value;
            DateTime = dateTime;
            BalanceStampBeforeTransaction = balanceStampBeforeTransaction;
            this.Account = account;
        }

        public static Transaction Create(Operation operation, decimal value, DateTime dateTime,
           BalanceStamp balanceStampBeforeTransaction, Account account, int id)
        {
            if (!account.IsThisTransactionIdAvailable(id))
                throw new ArgumentException("Такой Id уже используется");

            var transaction = new Transaction(operation, value, dateTime, balanceStampBeforeTransaction,
                account, id);
            account.AddTransaction(transaction);
            return transaction;
        }

        private Transaction(Operation operation, decimal value, DateTime dateTime,
            BalanceStamp balanceStampBeforeTransaction, Account account, int id)
        {
            Id = id;
            Operation = operation;
            Value = value;
            DateTime = dateTime;
            BalanceStampBeforeTransaction = balanceStampBeforeTransaction;
            this.Account = account;
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
            return string.Format("Transaction: {0} {1} {2}. {3} (balance before transaction). " +
                "{4}", DateTime, Operation, Value, BalanceBeforeTransaction, Account.Name);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())            
                return false;
            
            var transaction = obj as Transaction;
            if (Id == transaction.Id && DateTime == transaction.DateTime &&
                Operation == transaction.Operation && Value == transaction.Value &&
                BalanceStampBeforeTransaction.Equals(transaction.BalanceStampBeforeTransaction) &&
                Account.Equals(transaction.Account))
                return true;

            return false;
        }
    }
}