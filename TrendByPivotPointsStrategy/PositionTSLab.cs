using TSLab.Script;

namespace TradingSystems
{
    public class PositionTSLab : Position
    {
        public double EntryPrice { get; }
        public double Profit { get; }
        public int BarNumber { get; }
        public Security Security { get; }

        public string SignalNameForOpenPosition { get { return position.EntrySignalName; }}
        public string SignalNameForClosePosition { get { return position.ExitSignalName; }}        

        private IPosition position;

        public PositionTSLab(IPosition position)
        {
            EntryPrice = position.EntryPrice;
            this.position = position;
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            position.CloseAtStop(barNumber, stopPrice, signalNameForClosePosition);
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            position.CloseAtMarket(barNumber, signalNameForClosePosition);
        }
    }
}