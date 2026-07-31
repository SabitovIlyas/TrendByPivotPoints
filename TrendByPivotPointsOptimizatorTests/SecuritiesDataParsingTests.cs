using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using TradingSystems;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class SecuritiesDataParsingTests
    {
        [TestMethod()]
        public void GetSecuritiesData_ReadsSecurityAndIgnoresEmptyLines()
        {
            var fileName = Path.GetTempFileName();
            File.WriteAllLines(fileName, new[]
            {
                "Si;RUB;1;0,0000462;0;1",
                "",
            });

            try
            {
                var starter = new OptimizatorGeneticAlgorithmStarter();
                var securities = starter.GetSecuritiesData(fileName);

                Assert.AreEqual(1, securities.Count);
                Assert.AreEqual("Si", securities[0].Name);
                Assert.AreEqual(Currency.RUB, securities[0].Currency);
                Assert.IsFalse(securities[0].IsUSD);
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod()]
        public void GetSecuritiesData_ExplainsWhenQuotesFileGivenInsteadOfSecurities()
        {
            //Частая ошибка: вместо !Securities_Si.txt указан файл с котировками Si.txt.
            var fileName = Path.GetTempFileName();
            File.WriteAllLines(fileName, new[]
            {
                "<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>",
                "Si,1,20170103,100000,62111,62111,61740,61852,14101",
            });

            try
            {
                var starter = new OptimizatorGeneticAlgorithmStarter();
                var exception = Assert.ThrowsException<Exception>(
                    () => starter.GetSecuritiesData(fileName));

                StringAssert.Contains(exception.Message, "строка 1");
                StringAssert.Contains(exception.Message, "!Securities");
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod()]
        public void GetSecuritiesData_ReportsLineWithBrokenNumber()
        {
            var fileName = Path.GetTempFileName();
            File.WriteAllLines(fileName, new[]
            {
                "Si;RUB;1;0,0000462;0;1",
                "GOLD;RUB;один;0,0000462;0;1",
            });

            try
            {
                var starter = new OptimizatorGeneticAlgorithmStarter();
                var exception = Assert.ThrowsException<Exception>(
                    () => starter.GetSecuritiesData(fileName));

                StringAssert.Contains(exception.Message, "строка 2");
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod()]
        public void GetSecuritiesData_ThrowsWhenFileIsEmpty()
        {
            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, string.Empty);

            try
            {
                var starter = new OptimizatorGeneticAlgorithmStarter();
                Assert.ThrowsException<Exception>(() => starter.GetSecuritiesData(fileName));
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
