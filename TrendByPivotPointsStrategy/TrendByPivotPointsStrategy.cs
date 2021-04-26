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
            var system = new MainSystem();
            var bars = GetBars(sec);
            system.Initialize(sec, bars);
            system.Run();
            system.Paint();            
        }

        public List<Bar> GetBars(ISecurity sec)
        {
            var bars = new List<Bar>();
            foreach (var bar in sec.Bars)
                bars.Add(new Bar() { Open = bar.Open, High = bar.High, Low = bar.Low, Close = bar.Close });
            return bars;
        }
    }
}
