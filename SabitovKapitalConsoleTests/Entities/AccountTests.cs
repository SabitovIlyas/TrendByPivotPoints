using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class AccountTests
    {
        [TestMethod()]
        public void SortAccountsTest()
        {
            var balance = Balance.Create();            
            var account1 = Account.Create("Сабитов Ильяс Ильдарович", balance, 2);
            var account2 = Account.Create("Пятанов Иван Вадимович", balance, 3);
            var account3 = Account.Create("Ати", balance, 1);

            var expected = new List<Account>() { account3, account1, account2 };
            var actual = Account.Accounts;

            if (expected.Count != actual.Count)
                Assert.Fail();

            for (var i = 0; i < expected.Count; i++)            
                if (expected[i] != actual[i])
                    Assert.Fail();
        }
    }
}