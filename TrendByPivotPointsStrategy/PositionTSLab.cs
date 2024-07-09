using IPositionTSLab = TSLab.Script.IPosition;

namespace TradingSystems
{
    public class PositionTSLab : Position
    {
        //TODO: Позакрывать свойства от изменения

        public double EntryPrice { get; set; }
        public double Profit { get; set; }
        public int BarNumber { get; set; }
        public Security Security { get; set; }        
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
    }

    public class PositionLab : Position
    {
        public int BarNumber { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public double EntryPrice { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public double Profit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Security Security { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            throw new System.NotImplementedException();
        }
    }
}
