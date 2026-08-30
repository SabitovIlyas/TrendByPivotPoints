using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script.Helpers;

namespace TradingSystems.Tests
{
    /// <summary>
    /// Кэш рядов заменил собой пересчёт индикаторов на каждый прогон стратегии.
    /// Проверяем главное: значения остались прежними — теми, что раньше получались
    /// через Converter, — включая зеркальную сторону шорта, а ряды действительно
    /// считаются один раз.
    /// </summary>
    [TestClass()]
    public class BarSeriesCacheTests
    {
        private List<Bar> bars;

        [TestInitialize]
        public void TestInitialize()
        {
            bars = CreateBars(
                new[] { 100d, 105, 107, 111, 117, 112, 108, 115, 121, 119 },
                new[] { 110d, 112, 115, 118, 120, 116, 114, 119, 125, 124 },
                new[] { 95d, 104, 106, 109, 113, 107, 103, 110, 116, 115 },
                new[] { 102d, 108, 110, 116, 114, 109, 111, 118, 122, 117 });
        }

        [TestMethod()]
        public void HighestOfHighs_SovpadaetSoStarymRaschetomDlyaLonga()
        {
            var security = CreateSecurity(bars);
            var converter = new Converter(isConverted: false);

            //Так канал считался до появления кэша.
            var expected = converter.GetHighest(
                converter.GetHighPrices(security).ToList(), period: 3).ToList();

            var actual = BarSeriesCache.For(bars).HighestOfHighs(3);

            CollectionAssert.AreEqual(expected, actual.ToList());
        }

        [TestMethod()]
        public void LowestOfLows_SovpadaetSoStarymRaschetomDlyaLonga()
        {
            var security = CreateSecurity(bars);
            var converter = new Converter(isConverted: false);

            var expected = converter.GetLowest(
                converter.GetLowPrices(security).ToList(), period: 3).ToList();

            var actual = BarSeriesCache.For(bars).LowestOfLows(3);

            CollectionAssert.AreEqual(expected, actual.ToList());
        }

        [TestMethod()]
        public void DlyaShorta_VerkhnyayaGranitsaEtoMinimumMinimumov()
        {
            //Стратегия для шорта берёт LowestOfLows там, где для лонга брала
            //HighestOfHighs. Раньше это зеркалил Converter — значения должны совпасть.
            var security = CreateSecurity(bars);
            var converter = new Converter(isConverted: true);

            var expected = converter.GetHighest(
                converter.GetHighPrices(security).ToList(), period: 3).ToList();

            var actual = BarSeriesCache.For(bars).LowestOfLows(3);

            CollectionAssert.AreEqual(expected, actual.ToList());
        }

        [TestMethod()]
        public void DlyaShorta_NizhnyayaGranitsaEtoMaksimumMaksimumov()
        {
            var security = CreateSecurity(bars);
            var converter = new Converter(isConverted: true);

            var expected = converter.GetLowest(
                converter.GetLowPrices(security).ToList(), period: 2).ToList();

            var actual = BarSeriesCache.For(bars).HighestOfHighs(2);

            CollectionAssert.AreEqual(expected, actual.ToList());
        }

        [TestMethod()]
        public void AverageTrueRange_SovpadaetSPryamymRaschetom()
        {
            var candles = new ReadAndAddList<TSLab.DataSource.IDataBar>();
            foreach (var bar in bars)
                candles.Add(bar);

            var expected = Series.AverageTrueRange(candles, 3).ToList();
            var actual = BarSeriesCache.For(bars).AverageTrueRange(3);

            CollectionAssert.AreEqual(expected, actual.ToList());
        }

        [TestMethod()]
        public void SmaIRsi_SovpadayutSPryamymRaschetom()
        {
            var closes = bars.Select(b => b.Close).ToList();
            var cache = BarSeriesCache.For(bars);

            CollectionAssert.AreEqual(Series.SMA(closes, 4).ToList(), cache.Sma(4).ToList());
            CollectionAssert.AreEqual(Series.RSI(closes, 4).ToList(), cache.Rsi(4).ToList());
        }

        [TestMethod()]
        public void RyadSchitaetsyaOdinRaz_PovtornyyZaprosOtdaetTotZheObekt()
        {
            var cache = BarSeriesCache.For(bars);

            Assert.AreSame(cache.HighestOfHighs(3), cache.HighestOfHighs(3),
                "Ряд с тем же периодом пересчитан заново — кэш не работает.");
            Assert.AreSame(cache.AverageTrueRange(3), cache.AverageTrueRange(3));
            Assert.AreSame(cache.HighPrices, cache.HighPrices);
        }

        [TestMethod()]
        public void RyadyRazlichayutsyaPoPeriodu()
        {
            var cache = BarSeriesCache.For(bars);

            Assert.AreNotSame(cache.HighestOfHighs(2), cache.HighestOfHighs(3));
        }

        [TestMethod()]
        public void KeshPrivyazanKOknu_UKazhdogoSpiskaBarovSvoy()
        {
            var otherBars = new List<Bar>(bars);

            Assert.AreSame(BarSeriesCache.For(bars), BarSeriesCache.For(bars),
                "Одно и то же окно должно получать один и тот же кэш.");
            Assert.AreNotSame(BarSeriesCache.For(bars), BarSeriesCache.For(otherBars),
                "Разные окна не должны делить один кэш.");
        }

        [TestMethod()]
        public void BumagaBeretTsenyIzKesha()
        {
            var security = CreateSecurity(bars);
            var cache = BarSeriesCache.For(bars);

            //Бумаг на одном окне создаются сотни за поколение — массивы цен у них
            //должны быть общие, а не свои у каждой.
            Assert.AreSame(cache.HighPrices, security.HighPrices);
            Assert.AreSame(cache.LowPrices, security.LowPrices);

            CollectionAssert.AreEqual(bars.Select(b => b.High).ToList(),
                security.HighPrices.ToList());
            CollectionAssert.AreEqual(bars.Select(b => b.Low).ToList(),
                security.LowPrices.ToList());
        }

        private SecurityLab CreateSecurity(List<Bar> bars)
        {
            return new SecurityLab(Currency.RUB, shares: 1, bars, new LoggerNull(),
                commissionRate: 0);
        }

        private List<Bar> CreateBars(double[] opens, double[] highs, double[] lows,
            double[] closes)
        {
            var result = new List<Bar>();
            var date = new DateTime(2026, 7, 29, 10, 00, 00);

            for (var i = 0; i < opens.Length; i++)
                result.Add(Bar.Create(date.AddHours(i), opens[i], highs[i], lows[i],
                    closes[i], 1, "SPFB.TEST", "1", 0));

            return result;
        }
    }
}
