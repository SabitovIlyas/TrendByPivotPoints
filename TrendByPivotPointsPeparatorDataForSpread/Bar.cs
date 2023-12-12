using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TrendByPivotPointsPeparatorDataForSpread
{
    public class Bar
    {
        public string Ticker { get; set; }
        public string Period { get; private set; }
        public DateTime DateTime { get; private set; }        
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public static Bar Create(DateTime dateTime, double open, double high, double low, double close,
            double volume = 0, string ticker = "", string period = "")
        {
            return new Bar(dateTime, open, high, low, close, volume, ticker, period);
        }

        private Bar(DateTime dateTime, double open, double high, double low, double close,
            double volume, string ticker, string period)
        {
            Ticker = ticker;
            Period = period;
            DateTime = dateTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        public static Bar operator -(Bar a, Bar b)
        {
            //var high = Math.Round(a.High - b.High, 2);
            //var low = Math.Round(a.Low - b.Low, 2);
            //var newHigh = Math.Max(high, low);
            //var newLow = Math.Min(high, low);
            //return Bar.Create(a.DateTime, Math.Round(a.Open - b.Open, 2), newHigh, newLow,
            //    Math.Round(a.Close - b.Close, 2), a.Volume, a.Ticker, a.Period);

            var open = Math.Round(a.Open - b.Open, 3);            
            var close = Math.Round(a.Close - b.Close, 3);
            var high = Math.Max(open, close);
            var low = Math.Min(open, close);
            return Bar.Create(a.DateTime, Math.Round(a.Open - b.Open, 3), high, low,
                Math.Round(a.Close - b.Close, 3), a.Volume, a.Ticker, a.Period);
        }

        public static bool operator ==(Bar a, Bar b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Bar a, Bar b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(Bar)) return false;
            var bar = obj as Bar;
            return Ticker == bar.Ticker &&
                Period == bar.Period &&
                DateTime == bar.DateTime &&
                Open == bar.Open &&
                High == bar.High &&
                Low == bar.Low &&
                Close == bar.Close &&
                Volume == bar.Volume;
        }

        public override string ToString()
        {
            var culture = new CultureInfo("en-US");
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", Ticker, Period,
                DateTime.Date.Year.ToString() + DateTime.Date.Month.ToString("D2") + 
                DateTime.Date.Day.ToString("D2"),
                DateTime.Hour.ToString("D2") + DateTime.Minute.ToString("D2") + 
                DateTime.Second.ToString("D2"), 
                Open.ToString("N7", culture), High.ToString("N7",culture), Low.ToString("N7", culture), 
                Close.ToString("N7", culture), Volume.ToString(culture));  
        }
    }
}