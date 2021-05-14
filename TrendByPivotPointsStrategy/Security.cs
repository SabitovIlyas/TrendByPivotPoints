namespace TrendByPivotPointsStrategy
{
    public interface Security
    {
        int BarNumber { get; set; }
        double? SellDeposit { get; }
        double? StepPrice { get; }

        double GetBarClose(int barNumber);
        double GetBarHigh(int barNumber);
        double GetBarLow(int barNumber);
        double GetBarOpen(int barNumber);
    }
}