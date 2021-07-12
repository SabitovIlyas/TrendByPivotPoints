using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TsLabColor = TSLab.Script.Color;
using TSLab.Script.GraphPane;

namespace TrendByPivotPointsStrategy
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
