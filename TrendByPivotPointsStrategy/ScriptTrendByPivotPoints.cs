using System;
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

        //long: 4, 7, 25, 120

        public OptimProperty leftLocalSide = new OptimProperty(1, 1, 16, 3);
        public OptimProperty rightLocalSide = new OptimProperty(1, 1, 16, 3);
        public OptimProperty pivotPointBreakDownSide = new OptimProperty(10, 10, 100, 10);
        public OptimProperty emaPeriodSide = new OptimProperty(20, 20, 200, 20);
        public OptimProperty rateUSD = new OptimProperty(75, 1, 1000, 1);
        public OptimProperty positionSide = new OptimProperty(0, 1, 2, 1);
        public OptimProperty comission = new OptimProperty(0.565, 0.001, 100, 0.001);
        public OptimProperty mode = new OptimProperty(2, 0, 1, 1);
        public OptimProperty riskValuePrcnt = new OptimProperty(0.25, 0, 1, 100);
        public OptimProperty securityNumber = new OptimProperty(0, 0, 1, 1);
        public OptimProperty instrumentsGroup = new OptimProperty(0, 0, 3, 1);
        public OptimProperty isPaint = new OptimProperty(0, 0, 1, 1);
        public OptimProperty isLoggerOn = new OptimProperty(1, 0, 1, 1);
        public OptimProperty shares = new OptimProperty(1, 0, 1, 1);

        public void Execute(IContext context, ISecurity[] securities)        
        {
            //var timeStart = DateTime.Now;
            var logger = new LoggerSystem(context);

            MainSystem system;

            switch ((int)mode)
                {
                case 0:
                    {
                        system = new MainSystemForRealTimeTesting();
                        break;
                    }
                case 1:
                    {
                        system = new MainSystemForOptimization();
                        break;
                    }
                default:
                    {
                        system = new MainSystemForTrading();
                        break;
                    }                    
            }

            if ((int)isLoggerOn == 1)
                system.Logger = new LoggerSystem(context);

            var systemParameters = new SystemParameters();

            systemParameters.Add("leftLocalSide", leftLocalSide);
            systemParameters.Add("rightLocalSide", rightLocalSide);
            systemParameters.Add("pivotPointBreakDownSide", pivotPointBreakDownSide);
            systemParameters.Add("emaPeriodSide", emaPeriodSide);
            systemParameters.Add("rateUSD", rateUSD);
            systemParameters.Add("positionSide", positionSide);
            systemParameters.Add("comission", comission);
            systemParameters.Add("riskValuePrcnt", riskValuePrcnt);
            systemParameters.Add("securityNumber", securityNumber);
            systemParameters.Add("instrumentsGroup", instrumentsGroup);
            systemParameters.Add("shares", shares);
                       
            try
            {
                system.SetParameters(systemParameters);
                system.Initialize(securities, context);
                system.Run();

                if ((int)isPaint == 1)
                    system.Paint();
            }
            catch(Exception e)
            {
                logger.Log(e.ToString());
            }

            //var timeStop = DateTime.Now;
            //var time = timeStop - timeStart;
            //logger.Log(time.ToString());
        }
    }
}
