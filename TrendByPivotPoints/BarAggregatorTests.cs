using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystems.Tests
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
            new Bar { Date = new DateTime(2025, 1, 1, 10, 0, 0), Open = 100, High = 101, Low = 99, Close = 100, Volume = 1000 }
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
            new Bar { Date = new DateTime(2025, 1, 1, 10, 0, 0), Open = 100, High = 101, Low = 99, Close = 100, Volume = 1000 },
            new Bar { Date = new DateTime(2025, 1, 1, 10, 2, 0), Open = 100, High = 102, Low = 98, Close = 101, Volume = 1500 },
            new Bar { Date = new DateTime(2025, 1, 1, 10, 6, 0), Open = 101, High = 103, Low = 100, Close = 102, Volume = 2000 }
        };

            var result = compressor.To5Minute(bars);
            Assert.AreEqual(2, result.Count);

            // First 5-minute bar (10:00-10:04)
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 0, 0), result[0].Date);
            Assert.AreEqual(100m, result[0].Open);
            Assert.AreEqual(102m, result[0].High);
            Assert.AreEqual(98m, result[0].Low);
            Assert.AreEqual(101m, result[0].Close);
            Assert.AreEqual(2500, result[0].Volume);

            // Second 5-minute bar (10:05-10:09)
            Assert.AreEqual(new DateTime(2025, 1, 1, 10, 5, 0), result[1].Date);
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
            new Bar { Date = new DateTime(2025, 1, 1, 9, 0, 0), Open = 100, High = 101, Low = 99, Close = 100, Volume = 1000 },
            new Bar { Date = new DateTime(2025, 1, 1, 15, 0, 0), Open = 100, High = 102, Low = 98, Close = 101, Volume = 1500 },
            new Bar { Date = new DateTime(2025, 1, 2, 9, 0, 0), Open = 101, High = 103, Low = 100, Close = 102, Volume = 2000 }
        };

            var result = compressor.ToDaily(bars);

            Assert.AreEqual(2, result.Count);

            // First day
            Assert.AreEqual(new DateTime(2025, 1, 1, 0, 0, 0), result[0].Date);
            Assert.AreEqual(100m, result[0].Open);
            Assert.AreEqual(102m, result[0].High);
            Assert.AreEqual(98m, result[0].Low);
            Assert.AreEqual(101m, result[0].Close);
            Assert.AreEqual(2500, result[0].Volume);

            // Second day
            Assert.AreEqual(new DateTime(2025, 1, 2, 0, 0, 0), result[1].Date);
            Assert.AreEqual(101m, result[1].Open);
            Assert.AreEqual(103m, result[1].High);
            Assert.AreEqual(100m, result[1].Low);
            Assert.AreEqual(102m, result[1].Close);
            Assert.AreEqual(2000, result[1].Volume);
        }
    }   

    public class BarCompressor
    {
        public List<Bar> CompressBars(List<Bar> bars, TimeSpan timeframe)
        {
            if (bars == null || !bars.Any()) return new List<Bar>();

            var result = new List<Bar>();
            var orderedBars = bars.OrderBy(b => b.Date).ToList();

            DateTime periodStart = TruncateToTimeframe(orderedBars[0].Date, timeframe);
            Bar currentBar = null;

            foreach (var bar in orderedBars)
            {
                var barPeriod = TruncateToTimeframe(bar.Date, timeframe);

                if (barPeriod > periodStart || currentBar == null)
                {
                    if (currentBar != null)
                        result.Add(currentBar);

                    currentBar = new Bar
                    {
                        Date = barPeriod,
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