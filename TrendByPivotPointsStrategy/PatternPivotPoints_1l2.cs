using System.Collections.Generic;

namespace TrendByPivotPointsStrategy
{
    public class PatternPivotPoints_1l2 : PatternPivotPoints
    {
        public bool Check(List<double> extremums, bool isConverted = false)
       {
            var pattern = new PatternPivotPoints_1g2();
            return pattern.Check(extremums, isConverted: true);
        }
    }
}