using System.Collections.Generic;

namespace TrendByPivotPoints
{
    public class PatternPivotPoints_1g2l3 : PatternPivotPoints
    {
        public bool Check(List<double> extremums)
        {
            var last3highs = new List<double>();
            var countHighsInPattern = 3;
            var lastIndex = extremums.Count - 1;
            var startIndex = lastIndex - (countHighsInPattern - 1); //

            for (var i = startIndex; i <= lastIndex; i++)
                last3highs.Add(extremums[i]);

            return (last3highs[1] > last3highs[0]) && (last3highs[2] < last3highs[1]);
        }
    }
}
