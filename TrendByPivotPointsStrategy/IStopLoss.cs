using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public interface IStopLoss
    {
        IContext ctx { get; set; }
        bool IsStopLossUpdateWhenBarIsClosedOnly { get; set; }
        Logger Logger { get; set; }

        void CreateStopLoss(double stopLoss, double breakdown);
        //void UpdateStopLossLongPosition(int barNumber, List<Indicator> lows, Indicator lastLow, IPosition le);
    }
}