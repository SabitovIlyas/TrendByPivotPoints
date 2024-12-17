namespace TradingSystems
{
    public interface Position
    {
        int BarNumberOpenPosition { get; }
        double EntryPrice { get; }
        double Profit { get; }
        string SignalNameForOpenPosition { get; }
        int BarNumberClosePosition { get; }
        string SignalNameForClosePosition { get; }

        void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition);
        void CloseAtMarket(int barNumber, string signalNameForClosePosition);
    }
}