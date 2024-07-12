using TSLab.Script;

namespace TradingSystems
{
    public class PositionTSLab : Position
    {
        //TODO: Позакрывать свойства от изменения

        public double EntryPrice { get; set; }
        public double Profit { get; set; }
        public int BarNumber { get; set; }
        public Security Security { get; set; }

        string SignalNameForOpenPosition { get; set; }
        string SignalNameForClosePosition { get ; set ; }

        private IPosition position;

        public PositionTSLab(IPosition position)
        {
            EntryPrice = position.EntryPrice;
            this.position = position;
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            position.CloseAtStop(barNumber, stopPrice, signalNameForClosePosition);
            SignalNameForClosePosition = signalNameForClosePosition;
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            position.CloseAtMarket(barNumber, signalNameForClosePosition);
            SignalNameForClosePosition = signalNameForClosePosition;
        }
    }
}