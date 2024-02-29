using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class AverageValueCombinationsDescendingComparer : IComparer<Combination>
    {
        public static AverageValueCombinationsDescendingComparer Create()
        {
            return new AverageValueCombinationsDescendingComparer();
        }

        private AverageValueCombinationsDescendingComparer() { }

        public int Compare(Combination x, Combination y)
        {
            var comparer = AverageValueCombinationsAscendingComparer.Create();
            return -comparer.Compare(x, y);
        }
    }
}