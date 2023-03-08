using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Data
{
    public class BalanceSerializator : Serializator
    {
        private Balance? balance;
        private BalanceStampSerializator balanceStampSerializator;
        private string serializedBalance;
        public static BalanceSerializator Create(Balance balance)
        {
            return new BalanceSerializator(balance);
        }

        private BalanceSerializator(Balance balance)
        {
            this.balance = balance;
            balanceStampSerializator = BalanceStampSerializator.Create();
        }

        public static BalanceSerializator Create(string serializedBalance)
        {
            return new BalanceSerializator(serializedBalance);
        }

        private BalanceSerializator(string serializedBalance) 
        {
            this.serializedBalance = serializedBalance;
            balanceStampSerializator = BalanceStampSerializator.Create();
        }        

        public object Deserialize()
        {
            if (serializedBalance == null)
                throw new NullReferenceException("Строка с сериализованным объектом " +
                    "не определена.");

            if (serializedBalance == string.Empty)
                throw new InvalidDataException("Строка с сериализованным объектом пуста.");

            try
            {
                var balance = Balance.Create();
                var lines = serializedBalance.Split("\r\n");
                foreach (var line in lines)
                {
                    if (line == "")
                        continue;

                    balanceStampSerializator.SerializedBalanceStamp = line;
                    balanceStampSerializator.Balance = balance;
                    var balanceStamp = (BalanceStamp)balanceStampSerializator.Deserialize();
                }

                return balance;
            }
            catch
            {
                throw new InvalidDataException("Строка с сериализованным объектом неверного " +
                    "формата.");
            }            
        }

        public string Serialize()
        {
            var result = string.Empty;
            foreach(var balanceStamp in balance.BalanceStamps)
            {                
                balanceStampSerializator.BalanceStamp = balanceStamp;
                result += balanceStampSerializator.Serialize() + "\r\n";
            }

            return result;
        }
    }
}