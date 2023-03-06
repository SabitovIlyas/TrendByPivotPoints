using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Data
{
    public class BalanceStampSerializator : Serializator
    {
        public BalanceStamp? BalanceStamp { get; set; }
        public string? SerializedBalanceStamp { get; set; }
        public Balance? Balance { get; set; }

        public static BalanceStampSerializator Create(BalanceStamp balanceStamp)
        {
            return new BalanceStampSerializator(balanceStamp);
        }

        private BalanceStampSerializator(BalanceStamp balanceStamp)
        {
            BalanceStamp = balanceStamp;
        }

        public static BalanceStampSerializator Create(string serializedBalanceStamp, 
            Balance balance)
        {
            return new BalanceStampSerializator(serializedBalanceStamp, balance);
        }

        private BalanceStampSerializator(string serializedBalanceStamp, Balance balance)
        {
            SerializedBalanceStamp = serializedBalanceStamp;
            Balance = balance;
        }

        public static BalanceStampSerializator Create()
        {
            return new BalanceStampSerializator();
        }

        private BalanceStampSerializator() { }

        public object Deserialize()
        {
            if (SerializedBalanceStamp == null)
                throw new NullReferenceException("Строка с сериализованным объектом " +
                    "не определена.");

            if (SerializedBalanceStamp == string.Empty)
                throw new InvalidDataException("Строка с сериализованным объектом пуста.");

            try
            {
                var info = SerializedBalanceStamp.Split(';');
                var id = int.Parse(info[1].Split('\t')[1]);
                var dateTime = DateTime.Parse(info[2].Split('\t')[1]);
                var value = decimal.Parse(info[3].Split('\t')[1]);
                return BalanceStamp.Create(dateTime, value, Balance, id);
            }
            catch
            {
                throw new InvalidDataException("Строка с сериализованным объектом неверного " +
                    "формата.");
            }
        }

        public string Serialize()
        {
            return string.Format("BalanceStamp;Id\t{0};DateTime\t{1};Value\t{2}",
                BalanceStamp?.Id, BalanceStamp?.DateTime, BalanceStamp?.Value.ToString("0.00"));
        }
    }
}