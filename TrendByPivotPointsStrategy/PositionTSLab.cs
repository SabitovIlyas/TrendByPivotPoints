using IPositionTSLab = TSLab.Script.IPosition;

namespace TradingSystems
{
    public class PositionTSLab : IPosition
    {
        //TODO: Позакрывать свойства от изменения

        public double EntryPrice { get; set; }
        public double Profit { get; set; }
        public int BarNumber { get; set; }
        public Security Security { get; set; }
        //TODO: iPosition спрятать за "адаптером"
        private IPositionTSLab position;

        public PositionTSLab()
        {
        }

        public PositionTSLab(IPositionTSLab position)
        {
            EntryPrice = position.EntryPrice;
            this.position = position;
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            position.CloseAtStop(barNumber, stopPrice, signalNameForClosePosition);
        }
    }
}
