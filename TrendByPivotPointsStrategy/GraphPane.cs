using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public interface GraphPane
    {
        void AddList(string name, Security sec, CandleStyles listStyle, Color color, PaneSides side);
        void AddList(string name, IList<double> candles, ListStyles listStyle, Color color, LineStyles lineStyle, PaneSides side);
    }    
}