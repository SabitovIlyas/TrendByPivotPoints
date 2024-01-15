using System.Collections.Generic;

namespace TradingSystems
{
    public class PatternPivotPoints_1g2 : PatternPivotPoints
    {
        public bool Check(List<double> extremums, bool isConverted = false)
        {
            var convertable = new Converter(isConverted);
            var last2extremums = new List<double>();
            var countExtremumsInPattern = 2;
            var lastIndex = extremums.Count - 1;

            if (extremums.Count < countExtremumsInPattern)
                return false;

            var startIndex = lastIndex - (countExtremumsInPattern - 1);

            for (var i = startIndex; i <= lastIndex; i++)
                last2extremums.Add(extremums[i]);
                        
            return convertable.IsGreater(last2extremums[1], last2extremums[0]);
        }
    }
}