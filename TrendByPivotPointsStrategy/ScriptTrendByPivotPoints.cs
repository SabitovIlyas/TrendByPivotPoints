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
    public class ScriptTrendByPivotPoints : IExternalScriptMultiSec
    {
        //public OptimProperty leftLocalSideLong = new OptimProperty(10, 4, 10, 2);
        //public OptimProperty rightLocalSideLong = new OptimProperty(10, 4, 10, 2);
        //public OptimProperty pivotPointBreakDownSideLong = new OptimProperty(100, 100, 200, 100);        
        //public OptimProperty EmaPeriodSideLong = new OptimProperty(200, 50, 200, 50);

        //public OptimProperty leftLocalSideShort = new OptimProperty(10, 4, 10, 2);
        //public OptimProperty rightLocalSideShort = new OptimProperty(10, 4, 10, 2);
        //public OptimProperty pivotPointBreakDownSideShort = new OptimProperty(100, 100, 200, 100);
        //public OptimProperty EmaPeriodSideShort = new OptimProperty(200, 50, 200, 50);


        public OptimProperty leftLocalSide = new OptimProperty(10, 5, 15, 5);
        public OptimProperty rightLocalSide = new OptimProperty(10, 5, 15, 5);
        public OptimProperty pivotPointBreakDownSide = new OptimProperty(2, 1, 3, 100);
        public OptimProperty EmaPeriodSide = new OptimProperty(200, 50, 200, 50);

        public void Execute(IContext context, ISecurity[] securities)        
        {            
            //var timeStart = DateTime.Now;
            //var logger = new LoggerSystem(context);

            var system = new MainSystem();
            system.SetParameters(leftLocalSide, rightLocalSide, pivotPointBreakDownSide, EmaPeriodSide);
            system.Initialize(securities, context);
            system.Run();

            //system.Paint(context, securities.First());
            //var timeStop = DateTime.Now;
            //var time = timeStop - timeStart;
            //logger.Log(time.ToString());
        }        
    }
}
