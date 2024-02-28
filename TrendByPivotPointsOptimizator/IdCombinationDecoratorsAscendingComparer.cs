using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class IdCombinationDecoratorsAscendingComparer : IComparer<CombinationDecorator>
    {
        public static IdCombinationDecoratorsAscendingComparer Create()
        {
            return new IdCombinationDecoratorsAscendingComparer();
        }

        private IdCombinationDecoratorsAscendingComparer() { }

        public int Compare(CombinationDecorator x, CombinationDecorator y)
        {
            try
            {
                if (x.Id > y.Id)
                    return 1;
                else if (x.Id < y.Id)
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