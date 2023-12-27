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
    }
}