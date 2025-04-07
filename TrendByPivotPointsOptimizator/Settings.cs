using System.Collections.Generic;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator
{
    public struct Settings
    {
        public List<PositionSide> Sides;
        public List<Interval> TimeFrames;
    }    
}