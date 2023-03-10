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
                var combination1 = x as Combination;
                var combination2 = y as Combination;

                if (combination1.GetAverageValue() > combination2.GetAverageValue())
                    return 1;
                else if (combination1.GetAverageValue() < combination2.GetAverageValue())
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