using System;
using TSLab.DataSource;
using System.IO;

namespace TrendByPivotPointsStrategy.Tests
{
    class DataBarFake : IDataBar
    {
        DateTime date;
        public DataBarFake(DateTime date)
        {
            this.date = date;
        }

        public double Volume => throw new NotImplementedException();

        public double Interest => throw new NotImplementedException();

        public TradeNumber FirstTradeId => throw new NotImplementedException();

        public double PotensialOpen => throw new NotImplementedException();

        public double Open => throw new NotImplementedException();

        public double Low => throw new NotImplementedException();

        public double High => throw new NotImplementedException();

        public double Close => throw new NotImplementedException();

        public bool IsAdditional => throw new NotImplementedException();

        public bool IsReadonly => throw new NotImplementedException();

        public int TicksCount => throw new NotImplementedException();

        public int OriginalFirstIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int OriginalLastIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime Date { get => date; set => throw new NotImplementedException(); }
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