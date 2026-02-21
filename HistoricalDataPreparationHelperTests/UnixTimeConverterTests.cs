using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static HistoricalDataPreparationHelper.Program;

namespace ForwardAnalysis.Tests
{
    [TestClass()]
    public class UnixTimeConverterTests
    {
        [TestMethod()]
        public void FromUnixTimeSecondsTest()
        {
            var timestamp = 1739990400;
            var converter = new UnixTimeConverter();
            DateTime dt = converter.FromUnixTimeSeconds(timestamp);
            var expected = "20250219,184000";
            var actual = dt.ToString("yyyyMMdd,HHmmss");
            Assert.AreEqual(expected, actual);
        }
    }
}