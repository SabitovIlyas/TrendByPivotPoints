using System.Collections.Generic;
using TSLab.DataSource;
using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    public interface Security
    {
        int BarNumber { get; set; }
        double? SellDeposit { get; }
        double? StepPrice { get; }
        double? BuyDeposit { get; }
        Bar LastBar { get; }
        List<Bar> GetBars(int barNumber);             
        double GetBarHigh(int barNumber);
        double GetBarLow(int barNumber);
        double GetBarOpen(int barNumber);
        int GetBarsCount();
        List<int> GetBarsBaseFromBarCompressed(int barNumber);
        bool IsLaboratory { get;}
        bool IsRealTimeTrading { get; }

        int GetBarCompressedNumberFromBarBaseNumber(int barNumber);
    }
}