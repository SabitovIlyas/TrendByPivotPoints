using System.Collections.Generic;

namespace TrendByPivotPointsStrategy
{
    public class PatternPivotPoints_1g2 : PatternPivotPoints
    {
        public bool Check(List<double> extremums)
        {
            var last2extremums = new List<double>();
            var countExtremumsInPattern = 2;
            var lastIndex = extremums.Count - 1;

            if (extremums.Count < countExtremumsInPattern)
                return false;

            var startIndex = lastIndex - (countExtremumsInPattern - 1);

            for (var i = startIndex; i <= lastIndex; i++)
                last2extremums.Add(extremums[i]);

            return (last2extremums[1] > last2extremums[0]);
        }
    }

    public class PatternPivotPoints_1g2g3 : PatternPivotPoints
    {
        public bool Check(List<double> extremums)
        {
            var last3extremums = new List<double>();
            var countExtremumsInPattern = 3;
            var lastIndex = extremums.Count - 1;

            if (extremums.Count < countExtremumsInPattern)
                return false;

            var startIndex = lastIndex - (countExtremumsInPattern - 1);

            for (var i = startIndex; i <= lastIndex; i++)
                last3extremums.Add(extremums[i]);

            return (last3extremums[1] > last3extremums[0] && last3extremums[2] > last3extremums[1]);
        }
    }

    public class PatternPivotPoints_1l2l3 : PatternPivotPoints
    {
        public bool Check(List<double> extremums)
        {
            var last3extremums = new List<double>();
            var countExtremumsInPattern = 3;
            var lastIndex = extremums.Count - 1;

            if (extremums.Count < countExtremumsInPattern)
                return false;

            var startIndex = lastIndex - (countExtremumsInPattern - 1);

            for (var i = startIndex; i <= lastIndex; i++)
                last3extremums.Add(extremums[i]);

            return (last3extremums[1] < last3extremums[0] && last3extremums[2] < last3extremums[1]);
        }
    }
}
