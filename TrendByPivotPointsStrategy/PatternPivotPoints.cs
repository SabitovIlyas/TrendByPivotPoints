using System.Collections.Generic;

namespace TrendByPivotPointsStrategy
{
    public interface PatternPivotPoints
    {
        bool Check(List<double> extremums, bool isConverted = false);
    }
}