using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.GraphPane;
using System.Collections.Generic;

namespace TrendByPivotPointsStrategy
{
    public interface Pane
    {
        void AddList(string name, Security security, CandleStyles listSlyle, SystemColor color, PaneSides side);
        void AddList(string name, IList<double> values, SystemColor color, PaneSides side);
        void AddInteractivePoint(string id, PaneSides side, bool isRemovable, SystemColor color, MarketPoint position);
        void ClearInteractiveObjects();
    }
}
