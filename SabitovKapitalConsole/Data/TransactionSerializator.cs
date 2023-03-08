using SabitovCapitalConsole.Entities;
using System.Globalization;

namespace SabitovCapitalConsole.Data
{
    public class TransactionSerializator : Serializator
    {
        public Transaction? Transaction { get; set; }
        public BalanceStamp? BalanceStamp { get; set; }
        public Account? Account { get; set; }
        public string? SerializedTransaction { get; set; }
        public Balance? Balance { get; set; }
        public Portfolio Portfolio { get; set; }

        public static TransactionSerializator Create(Transaction transaction, 
            BalanceStamp balanceStamp, Account account) 
        {
            return new TransactionSerializator(transaction, balanceStamp, account);
        }

        private TransactionSerializator(Transaction transaction, BalanceStamp balanceStamp,
            Account account) 
        {
            Transaction = transaction;
            BalanceStamp = balanceStamp;
            Account = account;
        }

        public static TransactionSerializator Create(string serializedTransaction, 
            Balance balance, Portfolio portfolio)
        {
            return new TransactionSerializator(serializedTransaction, balance, portfolio);
        }

        private TransactionSerializator(string serializedTransaction, Balance balance, 
            Portfolio portfolio)
        {
            SerializedTransaction = serializedTransaction;
            Balance = balance;
            Portfolio = portfolio;            
        }

        public string Serialize()
        {
            return string.Format("Transaction;Id\t{0};DateTime\t{1};Operation\t{2};" +
                "Value\t{3};BalanceStampId\t{4};AccountId\t{5}",
                Transaction?.Id, Transaction?.DateTime, Transaction?.Operation, 
                Transaction.Value.ToString("0.00"), 
                BalanceStamp?.Id, Account?.Id);
        }

        public object Deserialize()
        {
            if (SerializedTransaction == null)
                throw new NullReferenceException("Строка с сериализованным объектом " +
                    "не определена.");

            if (SerializedTransaction == string.Empty)
                throw new InvalidDataException("Строка с сериализованным объектом пуста.");

            try
            {
                var info = SerializedTransaction.Split(';');
                var id = int.Parse(info[1].Split('\t')[1]);
                var dateTime = DateTime.Parse(info[2].Split('\t')[1]);
                var operation = (Operation)Enum.Parse(typeof(Operation), info[3].Split('\t')[1]);
                var value = decimal.Parse(info[4].Split('\t')[1]);
                var balanceStampId = int.Parse(info[5].Split('\t')[1]);
                var balanceStamp = Balance.GetBalanceStampById(balanceStampId);
                var accountId = int.Parse(info[6].Split('\t')[1]);
                var account = Portfolio.GetAccountById(accountId);
                return Transaction.Create(operation, value, dateTime, balanceStamp, account, id);
            }
            catch(Exception e)
            {
                throw new InvalidDataException("Не удалось десериализовать транзакцию: " + e.Message);
            } 
        }
    }
}