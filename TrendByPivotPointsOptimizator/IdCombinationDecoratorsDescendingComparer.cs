using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class IdCombinationDecoratorsDescendingComparer : IComparer<CombinationDecorator>
    {
        public static IdCombinationDecoratorsDescendingComparer Create()
        {
            return new IdCombinationDecoratorsDescendingComparer();
        }

        private IdCombinationDecoratorsDescendingComparer() { }

        public int Compare(CombinationDecorator x, CombinationDecorator y)
        {
            var comparer = IdCombinationDecoratorsAscendingComparer.Create();
            return -comparer.Compare(x, y);
        }
    }
}