using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public class GraphPaneTsLab : GraphPane
    {
        private IGraphPane graphPane;
        public GraphPaneTsLab(IGraphPane graphPane) 
        {
            this.graphPane = graphPane;
        }

        public void AddList(string name, Security sec, CandleStyles listStyle, Color color, PaneSides side)
        {
            var s = sec as SecurityTSLab;
            graphPane.AddList(name, s.security, listStyle, color, side);
        }

        public void AddList(string name, IList<double> candles, ListStyles listStyle, Color color, LineStyles lineStyle, PaneSides side)
        {
            graphPane.AddList(name, candles, listStyle, color, lineStyle, side);
        }
    }
}
