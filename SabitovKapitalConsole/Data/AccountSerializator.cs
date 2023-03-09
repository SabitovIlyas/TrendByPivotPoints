using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Data
{
    public class AccountSerializator : Serializator
    {
        public Account? Account { get; set; }
        public string? SerializedAccount { get; set; }
        public Portfolio? Portfolio { get; set; }

        public static AccountSerializator Create(Account account)
        {
            return new AccountSerializator(account);
        }

        private AccountSerializator(Account account)
        {
            this.Account = account;
        }

        public static AccountSerializator Create(string serializedAccount, Portfolio portfolio)
        {
            return new AccountSerializator(serializedAccount, portfolio);
        }        

        private AccountSerializator(string serializedAccount, Portfolio portfolio)
        {
            this.SerializedAccount = serializedAccount;
            this.Portfolio = portfolio;
        }

        public static AccountSerializator Create()
        {
            return new AccountSerializator();
        }

        private AccountSerializator()
        {
        }

        public object Deserialize()
        {
            if (SerializedAccount == null)
                throw new NullReferenceException("Строка с сериализованным объектом " +
                    "не определена.");

            if (Portfolio == null)
                throw new NullReferenceException("Портфель не определён.");

            if (SerializedAccount == string.Empty)
                throw new InvalidDataException("Строка с сериализованным объектом пуста.");

            try
            {
                var info = SerializedAccount.Split(';');
                var id = int.Parse(info[1].Split('\t')[1]);
                var name = info[2].Split('\t')[1];
                return Account.Create(name, Portfolio, id);
            }
            catch
            {
                throw new InvalidDataException("Строка с сериализованным объектом неверного " +
                    "формата.");
            }
        }

        public string Serialize()
        {
            return string.Format("Account;Id\t{0};Name\t{1}", Account.Id, Account.Name);
        }
    }
}