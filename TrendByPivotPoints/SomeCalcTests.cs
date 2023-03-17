using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TrendByPivotPointsStrategy.Tests
{
    [TestClass()]
    public class SomeCalcTests
    {
        [DataTestMethod]
        [DataRow(10, 2, 3, false)]
        [DataRow(10, 2, 7, true)]
        [DataRow(5, 1, 3, true)]

        [DataRow(2, 10, 3, true)]
        [DataRow(2, 10, 7, false)]
        [DataRow(1, 5, 3, true)]
        public void IsTakeProfitPriceNearestThanChangePositionPriceForCurrentPrice(double priceTakeProfit,
            double priceChangePosition, double currentPrice, bool expected)
        {   
            var actual = Math.Abs(priceTakeProfit - currentPrice) <=
                Math.Abs(priceChangePosition - currentPrice);
            Assert.AreEqual(expected, actual);
        }
    }   
}