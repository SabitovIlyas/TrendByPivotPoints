using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPointsStrategy;
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
        GlobalMoneyManagerReal globalMoneyManager;

        [TestInitialize]
        public void TestInitialize()
        {
            account = new AccountFake();
            account.InitDeposit = 1000.0;
            globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 5.00); ;
        }

        [TestMethod()]
        public void GetMoneyTest_deposit1000_riskValuePrcnt5_freeBalance100_returned50()
        {
            //arrange            
            var expected = 50;
            account.Equity = 100;

            //act
            var actual = globalMoneyManager.GetMoneyForDeal();
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetMoneyTest_deposit1000_riskValuePrcnt5_freeBalance25_returned25()
        {
            //arrange            
            var expected = 25;
            account.Equity = 25;

            //act
            var actual = globalMoneyManager.GetMoneyForDeal();
            //assert
            Assert.AreEqual(expected, actual);
        }

    //    [TestMethod()]
    //    public void GetMoneyTest_deposit1000_riskValuePrcnt5_freeBalance25_returned25_1()
    //    {
    //        //arrange      

    //        var startDeposit = 1000d;
    //        var stopDeposit = 1100d;

    //        var profit = stopDeposit - startDeposit;

    //        if (profit >= 0)
    //            startDeposit = startDeposit + 0.5 * profit;
    //        else
    //            startDeposit = stopDeposit;


    //        var expected = 25;
    //        account.FreeBalance = 25;

    //        //act
    //        var actual = globalMoneyManager.GetMoneyForDeal();
    //        //assert
    //        Assert.AreEqual(expected, actual);
    //    }

    //    public void UpdateDeposit(DateTime currentDate, double currentBalance)
    //    {
    //        var odlDate = deposit.dateTime;
    //        if (currentDate.Month != odlDate.Month)
    //        {
    //            var profit = currentBalance - deposit.balance;

    //            if (profit >= 0)
    //                deposit.balance = deposit.balance + 0.5 * profit;
    //            else
    //                deposit.balance = currentBalance;             

    //            deposit.dateTime = currentDate;
    //        }
    //    }

    //    public void InitializeDeposit()
    //    {
    //        deposit = new Deposit();
    //    }

    //    Deposit deposit ;        
    }

    //public class Deposit
    //{
    //    public DateTime dateTime;
    //    public double balance;

    //    public void Update()
    //    {

    //    }
    //}
}