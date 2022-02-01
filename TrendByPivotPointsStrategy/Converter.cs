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

        public bool IsGreater(double a, double b)
        {
            if (!isConverted)
                return a > b;
            else
                return b < a;
        }

        public bool IsLess(double a, double b)
        {
            isConverted = !isConverted;
            var result = IsGreater(a, b);
            isConverted = !isConverted;
            return result;
        }
    }
}
