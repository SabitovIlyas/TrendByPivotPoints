using System;
using System.Globalization;
using System.IO;
using TSLab.DataSource;

namespace TradingSystems
{
    public class Bar : IDataBar
    {
        public string Ticker { get; set; }
        public string Period { get; set; }
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public int DigitsAfterPoint { get; set; }       

        public static Bar Create(DateTime dateTime, double open, double high, double low, double close,
            double volume = 0, string ticker = "", string period = "", int digitsAfterPoint = 0)
        {
            return new Bar(dateTime, open, high, low, close, volume, ticker, period, digitsAfterPoint);
        }

        private Bar(DateTime dateTime, double open, double high, double low, double close,
            double volume, string ticker, string period, int digitsAfterPoint)
        {
            Ticker = ticker;
            Period = period;
            Date = dateTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            DigitsAfterPoint = digitsAfterPoint;
        }

        public Bar()
        {
        }

        public static Bar operator -(Bar a, Bar b)
        {
            var open = Math.Round(a.Open - b.Open, 3);
            var close = Math.Round(a.Close - b.Close, 3);
            var high = Math.Max(open, close);
            var low = Math.Min(open, close);
            return Create(a.Date, Math.Round(a.Open - b.Open, 3), high, low,
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
                Date == bar.Date &&
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
                Date.Date.Year.ToString() + Date.Date.Month.ToString("D2") +
                Date.Date.Day.ToString("D2"),
                Date.Hour.ToString("D2") + Date.Minute.ToString("D2") +
                Date.Second.ToString("D2"),
                Open.ToString("N7", culture), High.ToString("N7", culture), Low.ToString("N7", culture),
                Close.ToString("N7", culture), Volume.ToString(culture));
        }

        public IDataBar MakeAdditional(DateTime newTime, bool byOpen)
        {
            throw new NotImplementedException();
        }

        public void Add(IBaseBar b2)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void Store(BinaryWriter stream)
        {
            throw new NotImplementedException();
        }

        public void Restore(BinaryReader stream, int version)
        {
            throw new NotImplementedException();
        }

        public double Interest => throw new NotImplementedException();

        public TradeNumber FirstTradeId => throw new NotImplementedException();

        public double PotensialOpen => throw new NotImplementedException();

        public bool IsAdditional => throw new NotImplementedException();

        public bool IsReadonly => throw new NotImplementedException();

        public int TicksCount => throw new NotImplementedException();

        public int OriginalFirstIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int OriginalLastIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }        
        public long Ticks { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
