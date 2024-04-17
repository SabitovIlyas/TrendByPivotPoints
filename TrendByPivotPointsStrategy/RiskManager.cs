namespace TradingSystems
{
    public interface RiskManager
    {
        double RiskValuePrcnt { get; }
        double GetMoneyForDeal();
        double Equity { get; }
        Logger Logger { get; set; }
        double FreeBalance { get; }
    }
}