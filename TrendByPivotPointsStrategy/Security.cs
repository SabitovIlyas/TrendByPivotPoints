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
        int GetSecurityCount();
        List<int> GetBarsBaseFromBarCompressed(int barNumber);
    }
}