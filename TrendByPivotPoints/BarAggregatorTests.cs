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

    [TestClass]
    public class BarCompressorTests
    {
        private List<BarA> GenerateTestBars()
        {
            return new List<BarA>
        {
            new BarA(new DateTime(2024, 3, 15, 10, 0, 0), 100, 105, 99, 102, 500),
            new BarA(new DateTime(2024, 3, 15, 10, 5, 0), 102, 108, 101, 106, 600),
            new BarA(new DateTime(2024, 3, 15, 10, 10, 0), 106, 110, 104, 108, 550),
            new BarA(new DateTime(2024, 3, 15, 10, 15, 0), 108, 112, 107, 111, 700),
            new BarA(new DateTime(2024, 3, 15, 10, 20, 0), 111, 115, 109, 114, 800),
            new BarA(new DateTime(2024, 3, 15, 10, 25, 0), 114, 118, 113, 117, 750),
            new BarA(new DateTime(2024, 3, 15, 10, 30, 0), 117, 120, 115, 119, 650)
        };
        }

        [TestMethod]
        public void Test_CompressBars_M15()
        {
            var bars = GenerateTestBars();
            var compressedBars = BarCompressor.CompressBars(bars, TimeFrame.M15);

            Assert.AreEqual(2, compressedBars.Count);
            Assert.AreEqual(new DateTime(2024, 3, 15, 10, 0, 0), compressedBars[0].Time);
            Assert.AreEqual(new DateTime(2024, 3, 15, 10, 15, 0), compressedBars[1].Time);

            Assert.AreEqual(100, compressedBars[0].Open);
            Assert.AreEqual(108, compressedBars[0].Close);
            Assert.AreEqual(110, compressedBars[0].High);
            Assert.AreEqual(99, compressedBars[0].Low);
            Assert.AreEqual(1650, compressedBars[0].Volume);
        }

        [TestMethod]
        public void Test_CompressBars_M30()
        {
            var bars = GenerateTestBars();
            var compressedBars = BarCompressor.CompressBars(bars, TimeFrame.M30);

            Assert.AreEqual(1, compressedBars.Count);
            Assert.AreEqual(new DateTime(2024, 3, 15, 10, 0, 0), compressedBars[0].Time);
            Assert.AreEqual(100, compressedBars[0].Open);
            Assert.AreEqual(119, compressedBars[0].Close);
            Assert.AreEqual(120, compressedBars[0].High);
            Assert.AreEqual(99, compressedBars[0].Low);
            Assert.AreEqual(4550, compressedBars[0].Volume);
        }

        [TestMethod]
        public void Test_CompressBars_D1()
        {
            var bars = GenerateTestBars();
            var compressedBars = BarCompressor.CompressBars(bars, TimeFrame.D1);

            Assert.AreEqual(1, compressedBars.Count);
            Assert.AreEqual(new DateTime(2024, 3, 15, 0, 0, 0), compressedBars[0].Time);
            Assert.AreEqual(100, compressedBars[0].Open);
            Assert.AreEqual(119, compressedBars[0].Close);
            Assert.AreEqual(120, compressedBars[0].High);
            Assert.AreEqual(99, compressedBars[0].Low);
            Assert.AreEqual(4550, compressedBars[0].Volume);
        }

        [TestMethod]
        public void Test_EmptyInput()
        {
            var compressedBars = BarCompressor.CompressBars(new List<BarA>(), TimeFrame.M15);
            Assert.AreEqual(0, compressedBars.Count);
        }

        [TestMethod]
        public void Test_NullInput()
        {
            var compressedBars = BarCompressor.CompressBars(null, TimeFrame.M15);
            Assert.AreEqual(0, compressedBars.Count);
        }
    }

    public class BarA
    {
        public DateTime Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }

        public BarA(DateTime time, decimal open, decimal high, decimal low, decimal close, long volume)
        {
            Time = time;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }
    }

    public enum TimeFrame
    {
        M5 = 5,
        M15 = 15,
        M30 = 30,
        H1 = 60,
        D1 = 1440
    }

    public static class BarCompressor
    {
        public static List<BarA> CompressBars(List<BarA> inputBars, TimeFrame targetTimeFrame)
        {
            if (inputBars == null || inputBars.Count == 0)
                return new List<BarA>();

            int targetMinutes = (int)targetTimeFrame;

            return inputBars
                .GroupBy(bar => GetCompressionTime(bar.Time, targetMinutes))
                .Select(group => new BarA(
                    time: group.Key,
                    open: group.First().Open,
                    high: group.Max(b => b.High),
                    low: group.Min(b => b.Low),
                    close: group.Last().Close,
                    volume: group.Sum(b => b.Volume)
                ))
                .OrderBy(bar => bar.Time)
                .ToList();
        }

        private static DateTime GetCompressionTime(DateTime barTime, int targetMinutes)
        {
            if (targetMinutes == 1440) // Дневной таймфрейм
            {
                return new DateTime(barTime.Year, barTime.Month, barTime.Day, 0, 0, 0);
            }

            int compressedMinute = (barTime.Hour * 60 + barTime.Minute) / targetMinutes * targetMinutes;
            return new DateTime(barTime.Year, barTime.Month, barTime.Day, compressedMinute / 60, compressedMinute % 60, 0);
        }
    }
}

namespace TradingSystems.Tests1
{
    [TestClass]
    public class BarCompressorTests
    {
        private BarCompressor compressor;

        [TestInitialize]
        public void Setup()
        {
            compressor = new BarCompressor();
        }

        [TestMethod]
        public void CompressBars_EmptyList_ReturnsEmptyList()
        {
            var result = compressor.To5Minute(new List<Bar>());
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CompressBars_SingleBar_ReturnsSingleBar()
        {
            var bars = new List<Bar>
        {
            new Bar { Time = new DateTime(2025, 1, 1, 10, 0, 0), Open = 100, High = 101, Low = 99, Close = 100, Volume = 1000 }
        };

            var result = compressor.To5Minute(bars);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(bars[0].Open, result[0].Open);
            Assert.AreEqual(bars[0].High, result[0].High);
            Assert.AreEqual(bars[0].Low, result[0].Low);
            Assert.AreEqual(bars[0].Close, result[0].Close);
            Assert.AreEqual(bars[0].Volume, result[0].Volume);
        }

        [TestMethod]
        public void CompressBars_5MinuteCompression_WorksCorrectly()
        {
            var bars = new List<Bar>
        {
            new Bar { Time = new DateTime(2025, 1, 1, 10, 0, 0), Open = 100, High = 101, Low = 99, Close = 100, Volume = 1000 },
            new Bar { Time = new DateTime(2025, 1, 1, 10, 2, 0), Open = 100, High = 102, Low = 98, Close = 101, Volume = 1500 },
            new Bar { Time = new DateTime(2025, 1, 1, 10, 6, 0), Open = 101, High = 103, Low = 100, Close = 102, Volume = 2000 }
        };

            var result = compressor.To5Minute(bars);
            Assert.AreEqual(2, result.Count);

            // First 5-minute bar (10:00-10:04)
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 0, 0), result[0].Time);
            Assert.AreEqual(100m, result[0].Open);
            Assert.AreEqual(102m, result[0].High);
            Assert.AreEqual(98m, result[0].Low);
            Assert.AreEqual(101m, result[0].Close);
            Assert.AreEqual(2500, result[0].Volume);

            // Second 5-minute bar (10:05-10:09)
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 5, 0), result[1].Time);
            Assert.AreEqual(101m, result[1].Open);
            Assert.AreEqual(103m, result[1].High);
            Assert.AreEqual(100m, result[1].Low);
            Assert.AreEqual(102m, result[1].Close);
            Assert.AreEqual(2000, result[1].Volume);
        }

        [TestMethod]
        public void CompressBars_DailyCompression_WorksCorrectly()
        {
            var bars = new List<Bar>
        {
            new Bar { Time = new DateTime(2025, 1, 1, 9, 0, 0), Open = 100, High = 101, Low = 99, Close = 100, Volume = 1000 },
            new Bar { Time = new DateTime(2025, 1, 1, 15, 0, 0), Open = 100, High = 102, Low = 98, Close = 101, Volume = 1500 },
            new Bar { Time = new DateTime(2025, 1, 2, 9, 0, 0), Open = 101, High = 103, Low = 100, Close = 102, Volume = 2000 }
        };

            var result = compressor.ToDaily(bars);

            Assert.AreEqual(2, result.Count);

            // First day
            Assert.AreEqual(new DateTime(2025, 1, 1, 0, 0, 0), result[0].Time);
            Assert.AreEqual(100m, result[0].Open);
            Assert.AreEqual(102m, result[0].High);
            Assert.AreEqual(98m, result[0].Low);
            Assert.AreEqual(101m, result[0].Close);
            Assert.AreEqual(2500, result[0].Volume);

            // Second day
            Assert.AreEqual(new DateTime(2025, 1, 2, 0, 0, 0), result[1].Time);
            Assert.AreEqual(101m, result[1].Open);
            Assert.AreEqual(103m, result[1].High);
            Assert.AreEqual(100m, result[1].Low);
            Assert.AreEqual(102m, result[1].Close);
            Assert.AreEqual(2000, result[1].Volume);
        }
    }

    public class Bar
    {
        public DateTime Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }

    public class BarCompressor
    {
        public List<Bar> CompressBars(List<Bar> bars, TimeSpan timeframe)
        {
            if (bars == null || !bars.Any()) return new List<Bar>();

            var result = new List<Bar>();
            var orderedBars = bars.OrderBy(b => b.Time).ToList();

            DateTime periodStart = TruncateToTimeframe(orderedBars[0].Time, timeframe);
            Bar currentBar = null;

            foreach (var bar in orderedBars)
            {
                var barPeriod = TruncateToTimeframe(bar.Time, timeframe);

                if (barPeriod > periodStart || currentBar == null)
                {
                    if (currentBar != null)
                        result.Add(currentBar);

                    currentBar = new Bar
                    {
                        Time = barPeriod,
                        Open = bar.Open,
                        High = bar.High,
                        Low = bar.Low,
                        Close = bar.Close,
                        Volume = bar.Volume
                    };
                    periodStart = barPeriod;
                }
                else
                {
                    currentBar.High = Math.Max(currentBar.High, bar.High);
                    currentBar.Low = Math.Min(currentBar.Low, bar.Low);
                    currentBar.Close = bar.Close;
                    currentBar.Volume += bar.Volume;
                }
            }

            if (currentBar != null)
                result.Add(currentBar);

            return result;
        }

        private DateTime TruncateToTimeframe(DateTime dt, TimeSpan timeframe)
        {
            long ticks = dt.Ticks - (dt.Ticks % timeframe.Ticks);
            return new DateTime(ticks);
        }

        public List<Bar> To5Minute(List<Bar> bars) => CompressBars(bars, TimeSpan.FromMinutes(5));
        public List<Bar> To15Minute(List<Bar> bars) => CompressBars(bars, TimeSpan.FromMinutes(15));
        public List<Bar> To30Minute(List<Bar> bars) => CompressBars(bars, TimeSpan.FromMinutes(30));
        public List<Bar> ToHourly(List<Bar> bars) => CompressBars(bars, TimeSpan.FromHours(1));
        public List<Bar> ToDaily(List<Bar> bars) => CompressBars(bars, TimeSpan.FromDays(1));
    }
}