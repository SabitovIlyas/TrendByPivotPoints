using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Data
{
    public class AccountSerializator : Serializator
    {
        private static int lastId;
        private Account account;
        private string serializedAccount;
        private Portfolio portfolio;

        public static AccountSerializator Create(Account account)
        {
            return new AccountSerializator(account);
        }

        public static AccountSerializator Create(string serializedAccount, Portfolio portfolio)
        {
            return new AccountSerializator(serializedAccount, portfolio);
        }

        private AccountSerializator(Account account)
        {
            this.account = account;
        }

        private AccountSerializator(string serializedAccount, Portfolio portfolio)
        {
            this.serializedAccount = serializedAccount;
            this.portfolio = portfolio;
        }

        private static int GetNewId()
        {
            return lastId++;
        }

        public object Deserialize()
        {
            var info = serializedAccount.Split(';');
            var id = int.Parse(info[1].Split(':')[1]);
            var name = info[2].Split(':')[1];
            return Account.Create(name, portfolio, id);
        }

        public string Serialize()
        {
            return string.Format("Account;Id:{0};Name:{1}", GetNewId(), account.Name);
        }
    }
}