using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TrendByPivotPoints
{
    [TestClass]
    public class GlobalMoneyManagerTests
    {
        [TestMethod]
        public void GetMoney_Deposit1000RiskValuePrcnt5FreeBalance100_50returned()
        {
            var mm = new LocalMoneyManager();
            var res = mm.GetQntContracts();
            //Assert.AreEqual();
        }

        public void GetMoney_Deposit1000RiskValuePrcnt5FreeBalance25_25returned()
        {
            var mm = new LocalMoneyManager();
            var res = mm.GetQntContracts();
            //Assert.AreEqual();
        }
    }
}
