using System.Collections.Generic;

namespace TrendByPivotPoints
{
    public interface PatternPivotPoints
    {
        bool Check(List<double> extremums);
    }
}