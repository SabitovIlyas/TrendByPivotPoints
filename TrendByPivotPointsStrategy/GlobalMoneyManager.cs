namespace TrendByPivotPointsStrategy
{
    public interface GlobalMoneyManager
    {
        double RiskValuePrcnt { get; }

        double GetMoney();
    }
}