namespace TradingSystems
{
    public class PositionLab : Position
    {
        public int BarNumber { get; private set; }
        public double EntryPrice => openOrder.Price;
        public PositionSide PositionSide => openOrder.PositionSide;
        public int Contracts => openOrder.Contracts;
        public double Profit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string SignalNameForOpenPosition => openOrder.SignalName;
        public string SignalNameForClosePosition => throw new System.NotImplementedException();

        private Order openOrder;
        private Order closeOrder;

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            closeOrder = new Order(barNumber, PositionSide, )
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            closeOrder = new Order(barNumber, PositionSide, stopPrice, Contracts, 
                signalNameForClosePosition);
        }

        public PositionLab(int barNumber, Order openOrder)
        {
            BarNumber = barNumber;
            this.openOrder = openOrder;
        }

        public void Update(int barNumber)
        {
            
        }
    }
}
