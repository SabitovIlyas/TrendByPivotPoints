using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TradingSystems;

namespace CorrelationCalculator.Tests
{
    [TestClass()]
    public class BarsCorrelationPreparatorTests
    {
        [TestMethod()]
        public void PrepareTest()
        {
            var siBars = new List<Bar>() {
                Bar.Create(new DateTime(2023, 11, 17), 90090, 91400, 89926, 91012),
                Bar.Create(new DateTime(2023, 11, 20), 91036, 91233, 88744, 89003),
                Bar.Create(new DateTime(2023, 11, 24), 89127, 90080, 88830, 89463) };
            var goldBars = new List<Bar>() {
                Bar.Create(new DateTime(2023, 11, 20), 1979.9, 1982.0, 1964.5, 1972.4, digitsAfterPoint:1),
                Bar.Create(new DateTime(2023, 11, 22), 1999.8, 2004.0, 1985.5, 1988.5, digitsAfterPoint:1),
                Bar.Create(new DateTime(2023, 11, 24), 1985.1, 2000.0, 1982.4, 1994.9, digitsAfterPoint:1),
                Bar.Create(new DateTime(2023, 11, 27), 1994.8, 2014.9, 1991.7, 2004.3, digitsAfterPoint:1),
            };

            var expectedSiBars = new List<Bar>() {
                Bar.Create(new DateTime(2023, 11, 17), 90090, 91400, 89926, 91012),
                Bar.Create(new DateTime(2023, 11, 20), 91036, 91233, 88744, 89003),
                Bar.Create(new DateTime(2023, 11, 22), 90082, 90657, 88787, 89233),
                Bar.Create(new DateTime(2023, 11, 24), 89127, 90080, 88830, 89463),
                Bar.Create(new DateTime(2023, 11, 27), 89127, 90080, 88830, 89463)
            };
            var expectedGoldBars = new List<Bar>() {
                Bar.Create(new DateTime(2023, 11, 17), 1979.9, 1982.0, 1964.5, 1972.4, digitsAfterPoint:1),
                Bar.Create(new DateTime(2023, 11, 20), 1979.9, 1982.0, 1964.5, 1972.4, digitsAfterPoint:1),
                Bar.Create(new DateTime(2023, 11, 22), 1999.8, 2004.0, 1985.5, 1988.5, digitsAfterPoint:1),
                Bar.Create(new DateTime(2023, 11, 24), 1985.1, 2000.0, 1982.4, 1994.9, digitsAfterPoint:1),
                Bar.Create(new DateTime(2023, 11, 27), 1994.8, 2014.9, 1991.7, 2004.3, digitsAfterPoint:1),
            };

            var barsCorrelationPreparator = new BarsCorrelationPreparator(siBars, goldBars);
            barsCorrelationPreparator.Prepare();            

            Assert.IsTrue(
                IsListBarEquals(expectedSiBars, siBars) &&
                IsListBarEquals(expectedGoldBars, goldBars));
        }

        private bool IsListBarEquals(List<Bar> expectedBars, List<Bar> actualBars)
        {
            var checkSiBars = false;
            if (expectedBars.Count == actualBars.Count)
            {
                int counter;
                for (counter = 0; counter < expectedBars.Count; counter++)
                {
                    if (expectedBars[counter].Date != actualBars[counter].Date ||
                        expectedBars[counter].Open != actualBars[counter].Open ||
                        expectedBars[counter].High != actualBars[counter].High ||
                        expectedBars[counter].Low != actualBars[counter].Low ||
                        expectedBars[counter].Close != actualBars[counter].Close)
                        break;
                }
                if (counter == expectedBars.Count)
                    checkSiBars = true;
            }

            return checkSiBars;
        }

        [TestMethod()]
        public void ComputeCoeffTest1()
        {
            var values1 = new List<double> { 2, 5, 7, 1, 8 };
            var values2 = new List<double> { 4, 3, 6, 1, 5 };
            var expected = 0.7927032095;

            var barsCorrelationPreparator = new BarsCorrelationPreparator();
            var actual = barsCorrelationPreparator.ComputeCoeff(values1.ToArray(), values2.ToArray());

            Assert.AreEqual(expected, actual, 0.00000001);
        }

        [TestMethod()]
        public void ComputeCoeffTest2()
        {
            var values1 = new List<double> { 4, 3, 6, 1, 5 };
            var values2 = new List<double> { 2, 5, 7, 1, 8 };            
            var expected = 0.7927032095;

            var barsCorrelationPreparator = new BarsCorrelationPreparator();
            var actual = barsCorrelationPreparator.ComputeCoeff(values1.ToArray(), values2.ToArray());

            Assert.AreEqual(expected, actual, 0.00000001);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest1()
        {
            var endDate = new DateTime(2023, 11, 30, 10, 0, 0);
            var expected = new DateTime(2022, 11, 1, 10, 0, 0);
            var diff = 13;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest2()
        {
            var endDate = new DateTime(2023, 11, 30, 10, 0, 0);//1-11 2-10 3-9 4-8 5-7 6-6 7-5 8-4 9-3 10-2 11-1 12-12 13-11 14-10 15 -9 16-8 17-7 18-6 19-5 20-4 21-3 22-2 23-1 24-12 
            var expected = new DateTime(2022, 12, 1, 10, 0, 0);
            var diff = 12;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest3()
        {
            var endDate = new DateTime(2023, 11, 30, 10, 0, 0);
            var expected = new DateTime(2023, 11, 1, 10, 0, 0);
            var diff = 1;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest4()
        {
            var endDate = new DateTime(2023, 12, 29, 10, 0, 0);
            var expected = new DateTime(2023, 01, 1, 10, 0, 0);
            var diff = 12;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest5()
        {
            var endDate = new DateTime(2023, 12, 31, 10, 0, 0);
            var expected = new DateTime(2023, 01, 1, 10, 0, 0);
            var diff = 12;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest6()
        {
            var endDate = new DateTime(2023, 12, 31, 10, 0, 0);
            var expected = new DateTime(2014, 01, 1, 10, 0, 0);
            var diff = 120;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest7()
        {
            var endDate = new DateTime(2023, 12, 31, 10, 0, 0);
            var expected = new DateTime(2022, 01, 1, 10, 0, 0);
            var diff = 24;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest8()
        {
            var endDate = new DateTime(2023, 11, 30, 10, 0, 0);
            var expected = new DateTime(2021, 12, 1, 10, 0, 0);
            var diff = 24;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest9()
        {
            var endDate = new DateTime(0002, 11, 30, 10, 0, 0);
            var expected = new DateTime(0001, 11, 1, 10, 0, 0);
            var diff = 13;
            var prep = new BarsCorrelationPreparator();

            var actual = prep.GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }
    }
}