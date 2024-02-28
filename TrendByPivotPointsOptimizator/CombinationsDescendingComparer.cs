using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class CombinationsDescendingComparer : IComparer<Combination>
    {
        public static CombinationsDescendingComparer Create()
        {
            return new CombinationsDescendingComparer();
        }

        private CombinationsDescendingComparer() { }

        public int Compare(Combination x, Combination y)
        {
            var comparer = AverageValueCombinationsAscendingComparer.Create();
            return -comparer.Compare(x, y);
        }
    }
}