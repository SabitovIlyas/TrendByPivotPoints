using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Data
{
    public class TransactionSerializator : Serializator
    {
        public static TransactionSerializator Create(Transaction transaction)
        {
            return new TransactionSerializator(transaction);
        }

        private TransactionSerializator(Transaction transaction) { }

        public string Serialize()
        {
            return "";
        }

        public object Deserialize()
        {
            return null;
        }
    }
}