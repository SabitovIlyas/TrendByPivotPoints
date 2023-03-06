using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole.Data;
using SabitovCapitalConsole.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class AccountSerializatorTests
    {
        [TestMethod()]
        public void SerializeTest()
        {
            var expected = "Account;Id:0;Name:Пятанов Иван Вадимович";
            var balance = Balance.Create();
            var portfolio = Portfolio.Create(balance);
            var account = Account.Create("Пятанов Иван Вадимович", portfolio);
            var accountSerializator = AccountSerializator.Create(account);
            var actual = accountSerializator.Serialize();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DeserealizationTest()
        {
            var balance = Balance.Create();
            var portfolio = Portfolio.Create(balance);
            var expected = Account.Create("Пятанов Иван Вадимович", portfolio);
            var serializedAccount = "Account;Id:0;Name:Пятанов Иван Вадимович";

            var anotherPortfolio = Portfolio.Create(balance);
            var accountSerializator = AccountSerializator.Create(serializedAccount, 
                anotherPortfolio);                
            var actual = accountSerializator.Deserialize();

            Assert.AreEqual(expected, actual);
        }
    }
}