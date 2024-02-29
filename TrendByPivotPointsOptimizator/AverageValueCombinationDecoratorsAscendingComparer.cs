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
                if (x.Combination.GetAverageValue() > y.Combination.GetAverageValue())
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