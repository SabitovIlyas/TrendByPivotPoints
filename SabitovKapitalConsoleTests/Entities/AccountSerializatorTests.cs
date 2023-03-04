using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class AccountSerializatorTests
    {
        [TestMethod()]
        public void SerializeTest()
        {
            var expected = "Account;Id:0;Name:Пятанов Иван Вадимович";
            var balance = Balance.Create();
            var account = Account.Create("Пятанов Иван Вадимович", balance);
            var accountSerializator = AccountSerializator.Create(account);
            var actual = accountSerializator.Serialize();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DeserealizationTest()
        {            
            var balance = Balance.Create();
            var expected = Account.Create("Пятанов Иван Вадимович", balance);
            var serializedAccount = "Account;Id:0;Name:Пятанов Иван Вадимович";

            var accountSerializator = AccountSerializator.Create(serializedAccount);
            var actual = accountSerializator.Deserialize();

            Assert.AreEqual(expected, actual);
        }
    }
}