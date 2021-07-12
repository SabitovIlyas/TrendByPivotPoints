using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TrendByPivotPointsStrategy
{
    public class TrendByPivotPointsStrategy : IExternalScript
    {   
        public OptimProperty leftLocal = new OptimProperty(3, 1, 10, 1);
        public OptimProperty rightLocal = new OptimProperty(3, 1, 10, 1);
        public OptimProperty pivotPointBreakDown = new OptimProperty(100, 10, 200, 10);        
        public OptimProperty EmaPeriod = new OptimProperty(200, 10, 300, 10);

        public void Execute(IContext context, ISecurity security)        
        {
            var timeStart = DateTime.Now;
            var logger = new LoggerSystem(context);            
            var system = new MainSystem();
            //var bars = GetBars(security);            
            system.Initialize(security, context);            
            system.Run();
            system.Paint(context, security);
            var timeStop = DateTime.Now;
            var time = timeStop - timeStart;
            logger.Log(time.ToString());
        }        
    }
}
