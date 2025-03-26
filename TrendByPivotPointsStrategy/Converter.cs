using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Helpers;

namespace TradingSystems
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

        public string WordMinimum
        {
            get
            {
                if (!isConverted)
                    return "минимум";
                return "максимум";
            }
        }

        public string WordMaximum
        {
            get
            {
                isConverted = !isConverted;
                var result = WordMinimum;
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
                var result = Above;
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
                var result = Rising;
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

        public string SymbolMinus
        {
            get
            {
                if (!isConverted)
                    return "-";
                return "+";
            }
        }

        public string SymbolPlus
        {
            get
            {
                isConverted = !isConverted;
                var result = SymbolMinus;
                isConverted = !isConverted;
                return result;
            }
        }

        public double Nil
        {
            get
            {
                if (!isConverted)
                    return 0;
                return double.MaxValue;
            }
        }

        public double MaxValue
        {
            get
            {
                isConverted = !isConverted;
                var result = Nil;
                isConverted = !isConverted;
                return result;
            }
        }

        public LinkedList<double> GetHighest(IList<double> values, int period)
        {
            if (!isConverted)
                return new LinkedList<double>(Series.Highest(values, period));
            return new LinkedList<double>(Series.Lowest(values, period));
        }

        public LinkedList<double> GetLowest(IList<double> values, int period)
        {
            isConverted = !isConverted;
            var result = GetHighest(values, period);
            isConverted = !isConverted;
            return result;
        }

        public IList<double> GetHighPrices(ISecurity security)
        {
            if (!isConverted)
                return security.HighPrices;
            return security.LowPrices;
        }

        public LinkedList<double> GetHighPrices(Security security)
        {
            if (!isConverted)                
                return security.HighPrices;
            return security.LowPrices;
        }

        public IList<double> GetLowPrices(ISecurity security)
        {
            isConverted = !isConverted;
            var result = GetHighPrices(security);
            isConverted = !isConverted;
            return result;
        }

        public LinkedList<double> GetLowPrices(Security security)
        {
            isConverted = !isConverted;
            var result = GetHighPrices(security);
            isConverted = !isConverted;
            return result;
        }

        public double Minimum(double a, double b)
        {
            if (!isConverted)
                return Math.Min(a, b);
            return Math.Max(a, b);
        }

        public double Maximum(double a, double b)
        {
            isConverted = !isConverted;
            var result = Minimum(a, b);
            isConverted = !isConverted;
            return result;
        }

        public double Difference(double a, double b)
        {
            if (isConverted)
                return b - a;
            return a - b;
        }
    }
}