using System.Collections.Generic;

namespace TradingSystems
{
    public class PatternPivotPoints_1l2g3 : PatternPivotPoints
    {
        public bool Check(List<double> extremums, bool isConverted = false)
        {
            var last3extremums = new List<double>();
            var countExtremumsInPattern = 3;

            if (extremums.Count < countExtremumsInPattern)
                return false;

            var lastIndex = extremums.Count - 1;
            var startIndex = lastIndex - (countExtremumsInPattern - 1);            

            for (var i = startIndex; i <= lastIndex; i++)
                last3extremums.Add(extremums[i]);

            return (last3extremums[1] < last3extremums[0]) && (last3extremums[2] > last3extremums[1]);
        }
    }
}