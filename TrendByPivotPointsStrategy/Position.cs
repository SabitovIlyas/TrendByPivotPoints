namespace TradingSystems
{
    public interface Position
    {
        int BarNumber { get; set; }
        double EntryPrice { get; set; }
        double Profit { get; set; }
        Security Security { get; set; }
        string OpenPositionSignalName { get; set; }
        string ClosePositionSignalName { get; set; }

        void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition);
        void CloseAtMarket(int barNumber, string signalNameForClosePosition);
    }
}