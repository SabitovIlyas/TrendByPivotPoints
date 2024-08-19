namespace TradingSystems
{
    public interface Position
    {
        int BarNumber { get; }
        double EntryPrice { get; }
        double Profit { get; }
        string SignalNameForOpenPosition { get; }
        string SignalNameForClosePosition { get; }

        void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition);
        void CloseAtMarket(int barNumber, string signalNameForClosePosition);
    }
}