namespace TradingSystems
{
    public interface GlobalMoneyManager
    {
        double RiskValuePrcnt { get; }
        double GetMoneyForDeal();
        double Equity { get; }
        ILogger Logger { get; set; }
        double FreeBalance { get; }
    }
}