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

        public static Portfolio Create()
        {
            return new Portfolio();
        }

        private Portfolio() { }

        public void AddAccount(Account account)
        {
            Accounts.Add(account);
        }

        public override string ToString()
        {
            var result = string.Empty;
            foreach (var account in Accounts)
                result += string.Format("{0}\r\n", account);

            return result;
        }
    }
}