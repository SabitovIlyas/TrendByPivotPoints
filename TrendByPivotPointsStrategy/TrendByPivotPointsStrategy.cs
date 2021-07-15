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
    public class TrendByPivotPointsStrategy : IExternalScriptMultiSec
    {   
        public OptimProperty leftLocal = new OptimProperty(3, 1, 10, 1);
        public OptimProperty rightLocal = new OptimProperty(3, 1, 10, 1);
        public OptimProperty pivotPointBreakDown = new OptimProperty(100, 10, 200, 10);        
        public OptimProperty EmaPeriod = new OptimProperty(200, 10, 300, 10);

        public void Execute(IContext context, ISecurity[] securities)        
        {
            var timeStart = DateTime.Now;
            var logger = new LoggerSystem(context);
            //var bars = GetBars(security);            
            MainSystem system = null;
            //foreach (var security in securities)
            //{
            //    system = new MainSystem();
            //    system.Initialize(security, context);
            //    system.Run();
            //    break;
            //}


            system = new MainSystem();
            system.Initialize(securities, context);
            system.Run();

            //system.Initialize(securities.First(), context);
            //system.Run();                

            system.Paint(context, securities.First());
            var timeStop = DateTime.Now;
            var time = timeStop - timeStart;
            logger.Log(time.ToString());
        }        
    }
}
