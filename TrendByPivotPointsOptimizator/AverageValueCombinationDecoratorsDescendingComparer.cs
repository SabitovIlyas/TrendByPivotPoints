using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class AverageValueCombinationDecoratorsDescendingComparer : IComparer<CombinationDecorator>
    {
        public static AverageValueCombinationDecoratorsDescendingComparer Create()
        {
            return new AverageValueCombinationDecoratorsDescendingComparer();
        }

        private AverageValueCombinationDecoratorsDescendingComparer() { }

        public int Compare(CombinationDecorator x, CombinationDecorator y)
        {
            var comparer = AverageValueCombinationDecoratorsAscendingComparer.Create();
            return -comparer.Compare(x, y);
        }
    }
}