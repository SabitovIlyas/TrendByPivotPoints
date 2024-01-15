using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TsLabColor = TSLab.Script.Color;
using TSLab.Script.GraphPane;
using System.Collections.Generic;

namespace TradingSystems
{
    public class PaneTSLab: Pane
    {
        private IGraphPane pane;

        public PaneTSLab(IGraphPane pane)
        {
            this.pane = pane;            
        }

        public void AddList(string name, Security security, CandleStyles listSlyle, SystemColor color, PaneSides side)
        {
            var colorTSlab = new TsLabColor(color.ToArgb());
            var securityTSLab = (SecurityTSlab)security;
            pane.AddList(name, securityTSLab.security, listSlyle, colorTSlab, side);
        }

        public void AddList(string name, IList<double> values, SystemColor color, PaneSides side)
        {
            var colorTSlab = new TsLabColor(color.ToArgb());
            var lst = pane.AddList(name, values, ListStyles.LINE, colorTSlab, LineStyles.SOLID, side);
            lst.Thickness = 3;
        }

        public void AddList(string name, IList<double> values, ListStyles listStyle, SystemColor color, LineStyles lineStyle, PaneSides side)
        {
            var colorTSlab = new TsLabColor(color.ToArgb());
            var lst = pane.AddList(name, values, listStyle, colorTSlab, lineStyle, side);
            lst.Thickness = 3;
        }             

        public void AddList(string name, IList<bool> values, ListStyles listStyle, SystemColor color, LineStyles lineStyle, PaneSides side)
        {
            var colorTSlab = new TsLabColor(color.ToArgb());
            var lst = pane.AddList(name, values, listStyle, colorTSlab, lineStyle, side);
            lst.Thickness = 3;
        }

        public void AddList(string name, ISecurity security, CandleStyles candleStyle, SystemColor color, PaneSides side)
        {
            var colorTSlab = new TsLabColor(color.ToArgb());
            var lst = pane.AddList(name, security, candleStyle, colorTSlab, side);
            lst.Thickness = 3;
        }       

        public void ClearInteractiveObjects()
        {
            pane.ClearInteractiveObjects();
        }

        public void AddInteractivePoint(string id, PaneSides side, bool isRemovable, SystemColor color, MarketPoint position)
        {
            var colorTSlab = new TsLabColor(color.ToArgb());
            pane.AddInteractivePoint(id, side, isRemovable, colorTSlab, position);
        }       
    }    
}
