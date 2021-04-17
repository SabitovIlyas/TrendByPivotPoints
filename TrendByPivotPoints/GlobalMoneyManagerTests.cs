using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPoints.Tests
{
    [TestClass()]
    public class GlobalMoneyManagerTests
    {
        AccountFake account;

        GlobalMoneyManager globalMoneyManager;
        [TestInitialize]
        public void TestInitialize()
        {
            account = new AccountFake();
            account.Deposit = 1000.0;
            globalMoneyManager = new GlobalMoneyManager(account, riskValuePrcnt: 5.00); ;
        }

        [TestCleanup]
        public void TestCleanup()
        {

        }

        [TestMethod()]
        public void GetMoneyTest_deposit1000_riskValuePrcnt5_freeBalance100_returned50()
        {
            //arrange            
            var expected = 50;
            account.FreeBalance = 100;

            //act
            var actual = globalMoneyManager.GetMoney();
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetMoneyTest_deposit1000_riskValuePrcnt5_freeBalance25_returned25()
        {
            //arrange            
            var expected = 25;
            account.FreeBalance = 25;

            //act
            var actual = globalMoneyManager.GetMoney();
            //assert
            Assert.AreEqual(expected, actual);
        }
    }
}