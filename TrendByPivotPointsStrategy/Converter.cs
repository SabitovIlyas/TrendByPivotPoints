namespace TrendByPivotPointsStrategy
{
    public class Converter
    {
        public bool IsConverted { get { return isConverted; } }

        private bool isConverted = false;

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

        public bool IsGreaterOrEqual(double greaterValue, double lessValue)
        {
            return IsGreater(greaterValue, lessValue) || (greaterValue == lessValue);
        }

        public bool IsLessOrEqual(double lessValue, double greaterValue)
        {
            return IsLess(lessValue, greaterValue) || (greaterValue == lessValue);
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
        
        public double Minus(double a, double b)
        {
            if (!isConverted)
                return a - b;
            return a + b;
        }

        public double Plus(double a, double b)
        {
            isConverted = !isConverted;
            var result = Minus(a, b);
            isConverted = !isConverted;
            return result;
        }

        public string Long
        {
            get
            {
                if (!isConverted)
                    return "long";
                return "short";
            }
        }

        public string Short
        {
            get
            {
                isConverted = !isConverted;
                var result = Long;
                isConverted = !isConverted;
                return result;
            }
        }

        public string Minimum
        {
            get
            {
                if (!isConverted)
                    return "минимум";
                return "максимум";
            }
        }

        public string Maximum
        {
            get
            {
                isConverted = !isConverted;
                var result = Long;
                isConverted = !isConverted;
                return result;
            }
        }

        public string Above
        {
            get
            {
                if (!isConverted)
                    return "выше";
                return "ниже";
            }
        }

        public string Under
        {
            get
            {
                isConverted = !isConverted;
                var result = Long;
                isConverted = !isConverted;
                return result;
            }
        }

        public string Rising
        {
            get
            {
                if (!isConverted)
                    return "повышающ";
                return "понижающ";
            }
        }

        public string Descending
        {
            get
            {
                isConverted = !isConverted;
                var result = Long;
                isConverted = !isConverted;
                return result;
            }
        }

        public double GetBarLow(Bar bar)
        {
            if (!isConverted)
                return bar.Low;
            return bar.High;
        }

        public double GetBarHigh(Bar bar)
        {
            isConverted = !isConverted;
            var result = GetBarLow(bar);
            isConverted = !isConverted;
            return result;
        }
    }
}