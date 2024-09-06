namespace TradingSystems
{
    public class PositionLab : Position
    {
        public int BarNumber { get; private set; }
        public double EntryPrice => order.Price;
        public PositionSide PositionSide => order.PositionSide;
        public int Contracts => order.Contracts;
        public double Profit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public string SignalNameForOpenPosition => order.SignalName;

        public string SignalNameForClosePosition => throw new System.NotImplementedException();

        private Order order;

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            throw new System.NotImplementedException();
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            throw new System.NotImplementedException();
        }

        public PositionLab(int barNumber, Order order)
        {
            BarNumber = barNumber;
            this.order = order;
        }
    }
}
