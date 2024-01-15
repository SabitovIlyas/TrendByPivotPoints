using System.Collections.Generic;
using TradingSystems;

namespace CorrelationCalculator
{
    public class BarsComparer : IComparer<Bar>
    {
        public int Compare(Bar x, Bar y)
        {
            try
            {
                if (x.Date > y.Date)
                    return 1;
                else if (x.Date < y.Date)
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