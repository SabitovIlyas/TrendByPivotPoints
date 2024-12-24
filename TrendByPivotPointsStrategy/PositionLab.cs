using System.Net.Http.Headers;

namespace TradingSystems
{
    public class PositionLab : Position
    {
        public int BarNumberOpenPosition { get; private set; } = int.MaxValue;
        public double EntryPrice => openOrder.Price;
        public double ExitPrice { get; private set; }
        public PositionSide PositionSide => openOrder.PositionSide;
        public int Contracts => openOrder.Contracts;
        public double Profit { get; private set; }
        public string SignalNameForOpenPosition => openOrder.SignalName;
        public string SignalNameForClosePosition { get; private set; }
        public bool IsActive { get { return closeOrder == null || closeOrder.IsActive; } }
        public Security Security { get; private set; }
        public int BarNumberClosePosition { get; set; } = int.MaxValue;

        private Order openOrder;
        private Order closeOrder;
        private Converter converter;
       

        public PositionLab(int barNumber, Order openOrder, Security security)
        {
            BarNumberOpenPosition = barNumber;
            this.openOrder = openOrder;
            Security = security;
            converter = new Converter(isConverted: PositionSide == PositionSide.Long);
        }

        public void Update(int barNumber, Bar bar)
        {
            if (closeOrder == null || closeOrder.IsActive)            
                Profit = converter.Difference(bar.Close, openOrder.ExecutedPrice);            
            else            
                Profit = converter.Difference(closeOrder.ExecutedPrice, openOrder.ExecutedPrice);            
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            BarNumberClosePosition = barNumber;
            ExitPrice = stopPrice;
            SignalNameForClosePosition = signalNameForClosePosition;
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            throw new System.NotImplementedException();
        }
    }
}