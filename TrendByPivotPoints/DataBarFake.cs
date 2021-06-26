using System;
using TSLab.DataSource;
using System.IO;

namespace TrendByPivotPointsStrategy.Tests
{
    class DataBarFake : IDataBar
    {
        DateTime date;
        double open;
        double low;
        double high;
        double close;        

        public double Volume => throw new NotImplementedException();

        public double Interest => throw new NotImplementedException();

        public TradeNumber FirstTradeId => throw new NotImplementedException();

        public double PotensialOpen => throw new NotImplementedException();

        public double Open { get => open; set => open = value; }

        public double Low { get => low; set => low = value; }

        public double High { get => high; set => high = value; }

        public double Close { get => close; set => close = value; }

        public bool IsAdditional => throw new NotImplementedException();

        public bool IsReadonly => throw new NotImplementedException();

        public int TicksCount => throw new NotImplementedException();

        public int OriginalFirstIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int OriginalLastIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime Date { get => date; set => date = value; }
        public long Ticks { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Add(IBaseBar b2)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public IDataBar MakeAdditional(DateTime newTime, bool byOpen)
        {
            throw new NotImplementedException();
        }

        public void Restore(BinaryReader stream, int version)
        {
            throw new NotImplementedException();
        }

        public void Store(BinaryWriter stream)
        {
            throw new NotImplementedException();
        }
    }
}