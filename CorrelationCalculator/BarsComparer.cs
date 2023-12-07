using System.Collections.Generic;
using TrendByPivotPointsPeparatorDataForSpread;

namespace CorrelationCalculator
{
    public class BarsComparer : IComparer<Bar>
    {
        public int Compare(Bar x, Bar y)
        {
            try
            {
                if (x.DateTime > y.DateTime)
                    return 1;
                else if (x.DateTime < y.DateTime)
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