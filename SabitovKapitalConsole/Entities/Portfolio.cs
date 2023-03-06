using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace SabitovCapitalConsole.Entities
{
    public class Portfolio
    {
        public List<Account> Accounts { get; private set; } = new List<Account>();
        public Balance Balance { get; private set; }

        public static Portfolio Create(Balance balance)
        {
            return new Portfolio(balance);
        }

        private Portfolio(Balance balance) 
        {
            Balance = balance;
        }

        public void AddAccount(Account account)
        {
            Accounts.Add(account);
            Accounts = SortAccounts();
        }

        public override string ToString()
        {
            var result = string.Empty;
            foreach (var account in Accounts)
                result += string.Format("{0}\r\n", account);

            return result;
        }

        public bool IsThisIdAvailable(int id)
        {
            if (Accounts.Find(p => p.Id == id) == null)
                return true;
            return false;
        }

        private List<Account> SortAccounts()
        {
            return (from account in Accounts
                    orderby account.Id
                    select account).ToList();
        }

        public int GetAccountId()
        {
            if (Accounts.Count == 0)
                return 0;
            return Accounts.Last().Id + 1;
        }
    }
}