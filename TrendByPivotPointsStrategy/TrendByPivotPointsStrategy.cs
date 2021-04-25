using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    class TrendByPivotPointsStrategy : IExternalScript

    {
        public void Execute(IContext ctx, ISecurity sec)
        {
            var b = sec.Bars[0];
            throw new NotImplementedException();
        }
    }
}
