namespace TradingSystems
{
    public interface Position
    {
        int BarNumberOpenPosition { get; }
        double EntryPrice { get; }
        double ExitPrice { get; }
        string SignalNameForOpenPosition { get; }
        int BarNumberClosePosition { get; }
        string SignalNameForClosePosition { get; }
        PositionSide PositionSide { get; }
        int Contracts { get; }

        void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition);
        void CloseAtMarket(int barNumber, double price, string signalNameForClosePosition);
        void CloseAtMarket(int barNumber, string signalNameForClosePosition);
        double GetProfit(int barNumber);        
    }
}