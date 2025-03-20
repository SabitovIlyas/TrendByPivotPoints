using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystems
{
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
            long ticks = dt.Ticks - dt.Ticks % timeframe.Ticks;
            return new DateTime(ticks);
        }

        public List<Bar> To5Minute(List<Bar> bars) => CompressBars(bars, TimeSpan.FromMinutes(5));
        public List<Bar> To15Minute(List<Bar> bars) => CompressBars(bars, TimeSpan.FromMinutes(15));
        public List<Bar> To30Minute(List<Bar> bars) => CompressBars(bars, TimeSpan.FromMinutes(30));
        public List<Bar> ToHourly(List<Bar> bars) => CompressBars(bars, TimeSpan.FromHours(1));
        public List<Bar> ToDaily(List<Bar> bars) => CompressBars(bars, TimeSpan.FromDays(1));
    }
}