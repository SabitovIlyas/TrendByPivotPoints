using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public interface Account
    {
        double Deposit { get;}
        double FreeBalance { get;}

        double GObying { get; }

        double GOselling { get; }

        double Rate { get; set; }

        ISecurity Security { get; }
    }
}
