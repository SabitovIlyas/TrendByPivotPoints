using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class DateTimeTests
    {
        [TestMethod()]
        public void DateTimeWithdrawTest1()
        {
            var endDate = new DateTime(2023, 11, 30);
            var tS = TimeSpan.FromDays(30);
            var expected = new DateTime(2023, 10, 31);

            var actual = endDate - tS;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest2()
        {
            var endDate = new DateTime(2023, 11, 30);
            var expected = new DateTime(2023, 11, 01);
            var actual = (endDate - TimeSpan.FromDays(30 - 1)).Date;
            Assert.AreEqual(expected, actual);
        }
    }
}