namespace TrendByPivotPointsStrategy
{
    public interface GlobalMoneyManager
    {
        double RiskValuePrcnt { get; }

        double GetMoneyForDeal();
        double FreeBalance { get; }

    }
}