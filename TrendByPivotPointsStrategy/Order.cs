namespace TradingSystems
{
    public class Order
    {
        public int BarNumber {  get; private set; }
        public PositionSide PositionSide { get; private set; }
        public double Price { get; private set; }
        public int Contracts { get; private set; }        
        public string SignalName { get; private set; }
        public bool IsActive { get; private set; }
        private Converter converter;
        public double ExecutedPrice { get; private set; }
        
        public Order(int barNumber, PositionSide positionSide, double price, int contracts, 
            string signalName) 
        {            
            BarNumber = barNumber;
            PositionSide = positionSide;
            Price = price;
            Contracts = contracts;
            SignalName = signalName;
            IsActive = true;
            ExecutedPrice = double.NaN;
            converter = new Converter(isConverted: positionSide == PositionSide.Long);
        }

        public void Execute(Bar bar)
        {
            if (PositionSide == PositionSide.Null) return;

            if (converter.IsGreaterOrEqual(bar.Open, Price))
            {
                IsActive = false;
                ExecutedPrice = bar.Open;
            }
            else if(converter.IsGreaterOrEqual(converter.GetBarHigh(bar), Price))
            {
                IsActive = false;
                ExecutedPrice = Price;
            }            
        }
    }
}