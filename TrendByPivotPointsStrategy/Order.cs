namespace TradingSystems
{
    public class Order
    {
        public int BarNumber {  get; private set; }
        public PositionSide PositionSide { get; private set; }
        public double Price { get; private set; }
        public int Contracts { get; private set; }        
        public string SignalName { get; private set; }
        

        public Order(int barNumber, PositionSide positionSide, double price, int contracts, 
            string signalName) 
        {            
            BarNumber = barNumber;
            PositionSide = positionSide;
            Price = price;
            Contracts = contracts;
            SignalName = signalName;
        }
    }
}