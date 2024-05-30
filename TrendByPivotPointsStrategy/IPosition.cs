namespace TradingSystems
{
    public interface IPosition
    {
        int BarNumber { get; set; }
        double EntryPrice { get; set; }
        double Profit { get; set; }
        Security Security { get; set; }
        void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition);
    }
}