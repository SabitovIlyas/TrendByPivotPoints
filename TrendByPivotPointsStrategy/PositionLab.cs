namespace TradingSystems
{
    public class PositionLab : Position
    {
        public int BarNumber { get; private set; }
        public double EntryPrice => openOrder.Price;
        public PositionSide PositionSide => openOrder.PositionSide;
        public int Contracts => openOrder.Contracts;
        public double Profit { get; private set; }
        public string SignalNameForOpenPosition => openOrder.SignalName;
        public string SignalNameForClosePosition => throw new System.NotImplementedException();
        public bool IsActive { get { return closeOrder == null || closeOrder.IsActive; } }

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
            if (closeOrder == null || closeOrder.IsActive)            
                Profit = converter.Difference(bar.Close, openOrder.ExecutedPrice);            
            else            
                Profit = converter.Difference(closeOrder.ExecutedPrice, openOrder.ExecutedPrice);            
        }
    }
}