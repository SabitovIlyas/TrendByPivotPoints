using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TSLab.DataSource;
using TSLab.Script.Helpers;

namespace TradingSystems
{
    /// <summary>
    /// Ряды, посчитанные по окну баров: цены, свечи и индикаторы.
    ///
    /// Оптимизатор прогоняет по одному и тому же окну сотни наборов параметров за
    /// поколение, и каждый прогон раньше считал ряды заново: массивы цен, список
    /// свечей, каналы Дончиана, ATR. При двадцати с лишним тысячах баров это
    /// массивы по двести килобайт — то есть сразу в кучу больших объектов, — да
    /// ещё связные списки на двадцать тысяч узлов. Отсюда и брались пятая часть
    /// времени в сборке мусора и распухание процесса до двадцати гигабайт.
    ///
    /// Ряд зависит только от окна и от периода индикатора, поэтому считается один
    /// раз и раздаётся всем прогонам. Кэш привязан к самому списку баров через
    /// ConditionalWeakTable: сменилось окно — прежние ряды освобождаются сами,
    /// чистить кэш руками не нужно.
    ///
    /// Отданные ряды менять нельзя: их одновременно читают несколько потоков.
    /// По той же причине список баров, на который заведён кэш, нельзя дополнять
    /// или править на месте — ряды к нему уже посчитаны.
    /// </summary>
    public class BarSeriesCache
    {
        private static readonly ConditionalWeakTable<List<Bar>, BarSeriesCache> caches =
            new ConditionalWeakTable<List<Bar>, BarSeriesCache>();

        private readonly List<Bar> bars;

        private readonly Lazy<double[]> highPrices;
        private readonly Lazy<double[]> lowPrices;
        private readonly Lazy<List<double>> closePrices;
        private readonly Lazy<IReadOnlyList<IDataBar>> candles;

        private readonly ConcurrentDictionary<int, Lazy<IList<double>>> highestOfHighs =
            new ConcurrentDictionary<int, Lazy<IList<double>>>();
        private readonly ConcurrentDictionary<int, Lazy<IList<double>>> lowestOfLows =
            new ConcurrentDictionary<int, Lazy<IList<double>>>();
        private readonly ConcurrentDictionary<int, Lazy<IList<double>>> averageTrueRanges =
            new ConcurrentDictionary<int, Lazy<IList<double>>>();
        private readonly ConcurrentDictionary<int, Lazy<IList<double>>> movingAverages =
            new ConcurrentDictionary<int, Lazy<IList<double>>>();
        private readonly ConcurrentDictionary<int, Lazy<IList<double>>> relativeStrengths =
            new ConcurrentDictionary<int, Lazy<IList<double>>>();

        /// <summary>Кэш окна. Одно и то же окно — один и тот же кэш.</summary>
        public static BarSeriesCache For(List<Bar> bars)
        {
            if (bars == null)
                throw new ArgumentNullException("bars");

            return caches.GetValue(bars, key => new BarSeriesCache(key));
        }

        private BarSeriesCache(List<Bar> bars)
        {
            this.bars = bars;

            highPrices = Lazily(() => Extract(bar => bar.High));
            lowPrices = Lazily(() => Extract(bar => bar.Low));
            closePrices = Lazily(() => new List<double>(Extract(bar => bar.Close)));
            candles = Lazily(BuildCandles);
        }

        /// <summary>Максимумы баров окна.</summary>
        public double[] HighPrices { get { return highPrices.Value; } }

        /// <summary>Минимумы баров окна.</summary>
        public double[] LowPrices { get { return lowPrices.Value; } }

        /// <summary>Цены закрытия баров окна.</summary>
        public List<double> ClosePrices { get { return closePrices.Value; } }

        /// <summary>Бары окна в виде, который понимают индикаторы TSLab.</summary>
        public IReadOnlyList<IDataBar> Candles { get { return candles.Value; } }

        /// <summary>Верхняя граница канала Дончиана: максимум максимумов за период.</summary>
        public IList<double> HighestOfHighs(int period)
        {
            return Get(highestOfHighs, period, () => Series.Highest(HighPrices, period));
        }

        /// <summary>Нижняя граница канала Дончиана: минимум минимумов за период.</summary>
        public IList<double> LowestOfLows(int period)
        {
            return Get(lowestOfLows, period, () => Series.Lowest(LowPrices, period));
        }

        public IList<double> AverageTrueRange(int period)
        {
            return Get(averageTrueRanges, period,
                () => Series.AverageTrueRange(Candles, period));
        }

        public IList<double> Sma(int period)
        {
            return Get(movingAverages, period, () => Series.SMA(ClosePrices, period));
        }

        public IList<double> Rsi(int period)
        {
            return Get(relativeStrengths, period, () => Series.RSI(ClosePrices, period));
        }

        /// <summary>
        /// Ряд из кэша, а если его там нет — посчитанный один раз. Lazy нужен именно
        /// для «один раз»: без него два потока, спросившие ряд одновременно, посчитали
        /// бы его оба, и вся экономия свелась бы к нулю на самых ходовых периодах.
        /// </summary>
        private IList<double> Get(ConcurrentDictionary<int, Lazy<IList<double>>> cache,
            int period, Func<IList<double>> calculate)
        {
            return cache.GetOrAdd(period, _ => new Lazy<IList<double>>(calculate,
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        private double[] Extract(Func<Bar, double> value)
        {
            var result = new double[bars.Count];
            var i = 0;

            foreach (var bar in bars)
                result[i++] = value(bar);

            return result;
        }

        private IReadOnlyList<IDataBar> BuildCandles()
        {
            var result = new ReadAndAddList<IDataBar>();

            foreach (var bar in bars)
                result.Add(bar);

            return result;
        }

        private static Lazy<T> Lazily<T>(Func<T> create)
        {
            return new Lazy<T>(create, LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
