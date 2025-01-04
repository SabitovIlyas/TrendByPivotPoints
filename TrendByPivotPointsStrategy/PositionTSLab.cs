using TSLab.Script;

namespace TradingSystems
{
    public class PositionTSLab : Position
    {
        public double EntryPrice { get; }
        public double Profit { get; }
        public int BarNumberOpenPosition { get; }
        public int BarNumberClosePosition { get; }
        public string SignalNameForOpenPosition { get { return position.EntrySignalName; }}
        public string SignalNameForClosePosition { get { return position.ExitSignalName; }}
        public PositionSide PositionSide { get; }

        public int Contracts => throw new System.NotImplementedException();

        public double ExitPrice => throw new System.NotImplementedException();

        private IPosition position;

        public PositionTSLab(IPosition position, int barNumber)
        {
            this.position = position;
            EntryPrice = position.EntryPrice;
            BarNumberOpenPosition = position.EntryBarNum;
            if (!position.IsActiveForBar(barNumber))
                Profit = position.Profit();      
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            position.CloseAtStop(barNumber, stopPrice, signalNameForClosePosition);
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            position.CloseAtMarket(barNumber, signalNameForClosePosition);
        }

        public void CloseAtMarket(int barNumber, double price, string signalNameForClosePosition)
        {
            throw new System.NotImplementedException();
        }
    }
}