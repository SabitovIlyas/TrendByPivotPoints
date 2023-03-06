using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Data
{
    public class AccountSerializator : Serializator
    {
        private Account? account;
        private string? serializedAccount;
        private Portfolio? portfolio;

        public static AccountSerializator Create(Account account)
        {
            return new AccountSerializator(account);
        }

        private AccountSerializator(Account account)
        {
            this.account = account;
        }

        public static AccountSerializator Create(string serializedAccount, Portfolio portfolio)
        {
            return new AccountSerializator(serializedAccount, portfolio);
        }        

        private AccountSerializator(string serializedAccount, Portfolio portfolio)
        {
            this.serializedAccount = serializedAccount;
            this.portfolio = portfolio;
        }

        public object Deserialize()
        {
            if (serializedAccount == null)
                throw new NullReferenceException("Строка с сериализованным объектом " +
                    "не определена.");

            if (portfolio == null)
                throw new NullReferenceException("Портфель не определён.");

            if (serializedAccount == string.Empty)
                throw new InvalidDataException("Строка с сериализованным объектом пуста.");

            try
            {
                var info = serializedAccount.Split(';');
                var id = int.Parse(info[1].Split('\t')[1]);
                var name = info[2].Split('\t')[1];
                return Account.Create(name, portfolio, id);
            }
            catch
            {
                throw new InvalidDataException("Строка с сериализованным объектом неверного " +
                    "формата.");
            }
        }

        public string Serialize()
        {
            return string.Format("Account;Id\t{0};Name\t{1}", account.Id, account.Name);
        }
    }
}