using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class AverageValueCombinationDecoratorsAscendingComparer : IComparer<CombinationDecorator>
    {
        public static AverageValueCombinationDecoratorsAscendingComparer Create()
        {
            return new AverageValueCombinationDecoratorsAscendingComparer();
        }

        private AverageValueCombinationDecoratorsAscendingComparer() { }

        public int Compare(CombinationDecorator x, CombinationDecorator y)
        {
            try
            {
                if (x.AverageValue > y.AverageValue)
                    return 1;
                else if (x.AverageValue < y.AverageValue)
                    return -1;

                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}