using System.Collections.Generic;

namespace TrendByPivotPoints
{
    public class PatternPivotPoints_1l2g3 : PatternPivotPoints
    {
        public bool Check(List<double> lows)
        {
            var last3lows = new List<double>();
            var countLowsInPattern = 3;
            var lastIndex = lows.Count - 1;
            var startIndex = lastIndex - (countLowsInPattern - 1); //

            for (var i = startIndex; i <= lastIndex; i++)
                last3lows.Add(lows[i]);

            return (last3lows[1] < last3lows[0]) && (last3lows[2] > last3lows[1]);
        }
    }
    public class PatternPivotPoints_1g2l3
    {
        public bool Check(List<double> highs)
        {
            var last3highs = new List<double>();
            var countHighsInPattern = 3;
            var lastIndex = highs.Count - 1;
            var startIndex = lastIndex - (countHighsInPattern - 1); //

            for (var i = startIndex; i <= lastIndex; i++)
                last3highs.Add(highs[i]);

            return (last3highs[1] > last3highs[0]) && (last3highs[2] < last3highs[1]);
        }
    }
}
