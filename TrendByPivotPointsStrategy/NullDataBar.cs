using System;
using TSLab.DataSource;
using System.IO;

namespace TradingSystems
{
    public class NullDataBar : IDataBar
    {
        public double Volume => 0;

        public double Interest => 0;

        public TradeNumber FirstTradeId => 0;

        public double PotensialOpen => 0;

        public double Open => 0;

        public double Low => 0;

        public double High => 0;

        public double Close => 0;

        public bool IsAdditional => false;

        public bool IsReadonly => false;

        public int TicksCount => 0;

        public int OriginalFirstIndex { get => 0; set => throw new NotImplementedException(); }
        public int OriginalLastIndex { get => 0; set => throw new NotImplementedException(); }
        public DateTime Date { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long Ticks { get => 0; set => throw new NotImplementedException(); }

        public void Add(IBaseBar b2)
        {            
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
        }

        public void Store(BinaryWriter stream)
        {            
        }
    }
}
