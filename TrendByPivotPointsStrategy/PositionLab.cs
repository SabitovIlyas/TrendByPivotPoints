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
        private Converter converter;

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            closeOrder = new Order(barNumber, PositionSide, double.NaN, Contracts, signalNameForClosePosition);
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
            converter = new Converter(isConverted: PositionSide == PositionSide.Long);
        }

        public void Update(int barNumber, Bar bar)
        {
            closeOrder.Execute(bar);
            Profit = converter.Difference(closeOrder.ExecutedPrice, openOrder.ExecutedPrice);
        }
    }
}