using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class CombinationsAscendingComparer : IComparer<Combination>
    {
        public static CombinationsAscendingComparer Create()
        {
            return new CombinationsAscendingComparer();
        }

        private CombinationsAscendingComparer() { }
        
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