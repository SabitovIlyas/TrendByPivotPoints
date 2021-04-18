using System.Collections.Generic;

namespace TrendByPivotPoints
{
    public interface Account
    {
        double Deposit { get;}
        double FreeBalance { get;}

        double GObying { get; }

        double GOselling { get; }

        double Rate { get; set; }
    }
}
