using System.Xml.Linq;

namespace SabitovCapitalConsole.Entities
{
    public class BalanceStamp
    {
        public int Id { get; private set; }
        public DateTime DateTime { get; private set; }
        public decimal Value { get; private set; }
        public static BalanceStamp Create(DateTime dateTime, decimal value, Balance balance)
        {
            return new BalanceStamp(dateTime, value, balance);
        }

        private BalanceStamp(DateTime dateTime, decimal value, Balance balance)
        {
            Id = balance.GetBalanceStampId();
            DateTime = dateTime;
            Value = value;                        
        }

        public static BalanceStamp Create(DateTime dateTime, decimal value, Balance balance, 
            int id)
        {
            if (!balance.IsThisIdAvailable(id))
                throw new ArgumentException("Такой Id уже используется");

            return new BalanceStamp(dateTime, value, balance, id);
        }

        private BalanceStamp(DateTime dateTime, decimal value, Balance balance, int id)
        {
            Id = id;
            DateTime = dateTime;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Balance Stamp: {0} {1}", DateTime, Value);
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj.GetType() == GetType())
            {
                var balanceStamp = obj as BalanceStamp;
                if (Id == balanceStamp.Id && DateTime == balanceStamp.DateTime &&
                    Value == balanceStamp.Value)
                    return true;
            }

            return false;
        }
    }
}