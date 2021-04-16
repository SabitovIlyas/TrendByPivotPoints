using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TrendByPivotPoints
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var mm = new MoneyManager();
            mm.GetQntContracts();
            //Первый коммит
        }
    }
}
