using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class MathTests
    {
        [TestMethod()]
        public void MathRoundTest1()
        {
            var expected = 3;
            var actual = Math.Floor(3.6d);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MathRoundTest2()
        {
            var expected = 5;
            var actual = Math.Ceiling(4.4d);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MathRoundTest3()
        {
            var expected = 5;
            var actual = Math.Round(4.5d, MidpointRounding.AwayFromZero);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MathRoundTest4()
        {
            var expected = 4;
            var actual = Math.Round(4.4d);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MathRoundTest5()
        {
            var expected = 5;
            var actual = Math.Round(4.6d);
            Assert.AreEqual(expected, actual);
        }
    }
}