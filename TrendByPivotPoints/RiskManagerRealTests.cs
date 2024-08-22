using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradingSystems;

namespace TrendByPivotPoints.Tests
{
    [TestClass()]
    public class RiskManagerRealTests
    {
        Account account;
        RiskManagerReal riskManagerReal;

        [TestInitialize]
        public void TestInitialize()
        {
            var logger = new NullLogger();
            var security = new SecurityLab(currency: Currency.Ruble, shares: 1);
            account = new AccountFake(initDeposit: 1000.0, baseCurrency: Currency.Ruble, logger: logger);
            riskManagerReal = new RiskManagerReal(account, logger, riskValuePrcnt: 5.00); ;
        }

        [TestMethod()]
        public void GetMoneyTest_deposit1000_riskValuePrcnt5_freeBalance100_returned50()
        {
            //arrange            
            var expected = 50;
            ((AccountFake)account).FreeBalance = 100;

            //act
            var actual = riskManagerReal.GetMoneyForDeal();
            //assert
            Assert.AreEqual(expected, actual);
        }
    }
}