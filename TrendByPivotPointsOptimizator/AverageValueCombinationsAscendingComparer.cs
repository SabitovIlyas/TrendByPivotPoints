using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class AverageValueCombinationsAscendingComparer : IComparer<Combination>
    {
        public static AverageValueCombinationsAscendingComparer Create()
        {
            return new AverageValueCombinationsAscendingComparer();
        }

        private AverageValueCombinationsAscendingComparer() { }
        
        public int Compare(Combination x, Combination y)
        {
            try
            {
                if (x.GetAverageValue() > y.GetAverageValue())
                    return 1;
                else if (x.GetAverageValue() < y.GetAverageValue())
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