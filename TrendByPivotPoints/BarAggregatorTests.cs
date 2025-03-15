using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystems.Tests
{
    [TestClass]
    public class BarAggregatorTests
    {
        [TestMethod]
        public void TestAggregate_5_Minutes()
        {
            var inputBars = new List<BarC>
        {
            new BarC { Time = new DateTime(2020, 1, 1, 9, 30, 0), Open = 100, High = 105, Low = 95, Close = 102 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 31, 0), Open = 101, High = 106, Low = 96, Close = 103 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 32, 0), Open = 102, High = 107, Low = 97, Close = 104 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 33, 0), Open = 103, High = 108, Low = 98, Close = 105 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 34, 0), Open = 104, High = 109, Low = 99, Close = 106 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 35, 0), Open = 105, High = 110, Low = 100, Close = 107 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 36, 0), Open = 106, High = 111, Low = 101, Close = 108 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 37, 0), Open = 107, High = 112, Low = 102, Close = 109 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 38, 0), Open = 108, High = 113, Low = 103, Close = 110 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 39, 0), Open = 109, High = 114, Low = 104, Close = 111 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 40, 0), Open = 110, High = 115, Low = 105, Close = 112 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 41, 0), Open = 111, High = 116, Low = 106, Close = 113 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 42, 0), Open = 112, High = 117, Low = 107, Close = 114 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 43, 0), Open = 113, High = 118, Low = 108, Close = 115 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 44, 0), Open = 114, High = 119, Low = 109, Close = 116 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 45, 0), Open = 115, High = 120, Low = 110, Close = 117 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 46, 0), Open = 116, High = 121, Low = 111, Close = 118 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 47, 0), Open = 117, High = 122, Low = 112, Close = 119 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 48, 0), Open = 118, High = 123, Low = 113, Close = 120 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 49, 0), Open = 119, High = 124, Low = 114, Close = 121 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 50, 0), Open = 120, High = 125, Low = 115, Close = 122 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 51, 0), Open = 121, High = 126, Low = 116, Close = 123 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 52, 0), Open = 122, High = 127, Low = 117, Close = 124 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 53, 0), Open = 123, High = 128, Low = 118, Close = 125 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 54, 0), Open = 124, High = 129, Low = 119, Close = 126 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 55, 0), Open = 125, High = 130, Low = 120, Close = 127 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 56, 0), Open = 126, High = 131, Low = 121, Close = 128 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 57, 0), Open = 127, High = 132, Low = 122, Close = 129 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 58, 0), Open = 128, High = 133, Low = 123, Close = 130 },
            new BarC { Time = new DateTime(2020, 1, 1, 9, 59, 0), Open = 129, High = 134, Low = 124, Close = 131 },
        };

            var expectedOutput = new List<BarC>
        {
            new BarC { Time = new DateTime(2020, 1, 1, 9, 30, 0), Open = 100, High = 134, Low = 95, Close = 131 },
        };

            var actualOutput = BarAggregator.AggregateBars(inputBars, TimeSpan.FromMinutes(30));

            CollectionAssert.AreEqual(expectedOutput, actualOutput);
        }
    }

    public class BarC
    {
        public DateTime Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
    }

    public static class BarAggregator
    {
        // Сжимаем бары в нужный таймфрейм
        public static List<BarC> AggregateBars(List<BarC> bars, TimeSpan timeFrame)
        {
            var result = new List<BarC>();

            foreach (var group in bars.GroupBy(b => RoundToNearestInterval(b.Time, timeFrame)))
            {
                var firstBarInGroup = group.First();

                result.Add(new BarC
                {
                    Time = group.Key,
                    Open = firstBarInGroup.Open,
                    High = group.Max(b => b.High),
                    Low = group.Min(b => b.Low),
                    Close = group.Last().Close
                });
            }

            return result;
        }

        private static DateTime RoundToNearestInterval(DateTime dateTime, TimeSpan interval)
        {
            long ticks = (dateTime.Ticks + interval.Ticks / 2) / interval.Ticks * interval.Ticks;
            return new DateTime(ticks);
        }
    }
}