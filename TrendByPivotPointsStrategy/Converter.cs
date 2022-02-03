namespace TrendByPivotPointsStrategy
{
    public class Converter
    {
        bool isConverted = false;

        public Converter(bool isConverted)
        {
            this.isConverted = isConverted;
        }

        public bool IsGreater(double greaterValue, double lessValue)
        {
            if (!isConverted)
                return greaterValue > lessValue;
            return greaterValue < lessValue;
        }

        public bool IsLess(double lessValue, double greaterValue)
        {
            isConverted = !isConverted;
            var result = IsGreater(lessValue, greaterValue);
            isConverted = !isConverted;
            return result;
        }

        public double GetBarLow(Security security, int barNumber)
        {            
            if (!isConverted)
                return security.GetBarLow(barNumber);
            return security.GetBarHigh(barNumber);
        }

        public double GetBarHigh(Security security, int barNumber)
        {
            isConverted = !isConverted;
            var result = GetBarLow(security, barNumber);
            isConverted = !isConverted;
            return result;            
        }
    }
}
