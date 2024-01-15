using System.Collections.Generic;

namespace TradingSystems
{
    public interface PatternPivotPoints
    {
        bool Check(List<double> extremums, bool isConverted = false);
    }
}