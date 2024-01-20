namespace TradingSystems
{
    public class Indicator
    {
        public int BarNumber { get { return barNumber; } set { barNumber = value; } }
        public double Value { get { return value; } set { this.value = value; } }

        private int barNumber;
        private double value;

        public Indicator()
        {         
        }

        public Indicator(int barNumber, double value)
        {
            this.barNumber = barNumber;
            this.value = value;
        }
    }
}
