using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            else
                return greaterValue < lessValue;
        }

        public bool IsLess(double lessValue, double greaterValue)
        {
            isConverted = !isConverted;
            var result = IsGreater(lessValue, greaterValue);
            isConverted = !isConverted;
            return result;
        }
    }
}
