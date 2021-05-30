using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class TrendByPivotPointsStrategy : IExternalScript
    {         
        public static IContext ctx;
    
        public void Execute(IContext context, ISecurity security)        
        {
            var timeStart = DateTime.Now;
            var logger = new LoggerSystem(context);            
            var system = new MainSystem();
            //var bars = GetBars(security);
            TrendByPivotPointsStrategy.ctx = context;
            system.Initialize(security, context);            
            system.Run();
            system.Paint(context, security);
            var timeStop = DateTime.Now;
            var time = timeStop - timeStart;
            logger.Log(time.ToString());
        }

        //public List<Bar> GetBars(ISecurity sec)
        //{
        //    var bars = new List<Bar>();
        //    foreach (var bar in sec.Bars)
        //        bars.Add(new Bar() { Open = bar.Open, High = bar.High, Low = bar.Low, Close = bar.Close, Date = bar.Date });
        //    return bars;
        //}
    }
}
