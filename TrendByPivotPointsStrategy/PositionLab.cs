namespace TradingSystems
{
    public class PositionLab : Position
    {
        public int BarNumberOpenPosition { get; private set; } = int.MaxValue;
        public double EntryPrice => openOrder.Price;
        public double ExitPrice { get; private set; }
        public PositionSide PositionSide => openOrder.PositionSide;
        public int Contracts => openOrder.Contracts;        
        public string SignalNameForOpenPosition => openOrder.SignalName;
        public string SignalNameForClosePosition { get; private set; }
        public Security Security { get; private set; }
        public int BarNumberClosePosition { get; set; } = int.MaxValue;

        private Order openOrder;
        private Converter converter;
        private double profit = double.MinValue;

        public PositionLab(int barNumber, Order openOrder, Security security)
        {
            BarNumberOpenPosition = barNumber;
            this.openOrder = openOrder;
            Security = security;
            converter = new Converter(isConverted: PositionSide != PositionSide.Long);
        }

        public PositionLab(int barNumber, PositionSide positionSide, Security security)
        {
            BarNumberOpenPosition = barNumber;            
            Security = security;
            converter = new Converter(isConverted: positionSide != PositionSide.Long);
        }

        public PositionLab(PositionSide positionSide, Security security, double profit, 
            int barNumberOpenPosition, int barNumberClosePosition) : this(barNumberOpenPosition, positionSide, security)
        {

            BarNumberClosePosition = barNumberClosePosition;
            this.profit = profit;
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            BarNumberClosePosition = barNumber;
            ExitPrice = stopPrice;
            SignalNameForClosePosition = signalNameForClosePosition;
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            var bar = Security.GetBar(barNumber);
            CloseAtMarket(barNumber, bar.Open, signalNameForClosePosition);
        }

        public void CloseAtMarket(int barNumber, double price, string signalNameForClosePosition)
        {
            BarNumberClosePosition = barNumber;
            ExitPrice = price;
            SignalNameForClosePosition = signalNameForClosePosition;
        }

        public double GetProfit(int barNumber)
        {
            if (profit != double.MinValue)
                return profit;

            if (barNumber >= BarNumberClosePosition)
            {
                return GetFixedProfit();
            }
            else if (barNumber >= BarNumberOpenPosition)
            {
                return GetUnfixedProfit(barNumber);
            }
            return 0;
        }


        public double GetFixedProfit()
        {            
            return converter.Difference(ExitPrice, EntryPrice) * Contracts;
        }

        public double GetUnfixedProfit(int barNmber)
        {
            var barClose = Security.GetBarClose(barNmber);            
            return converter.Difference(barClose, EntryPrice) * Contracts;
        }        
    }
}