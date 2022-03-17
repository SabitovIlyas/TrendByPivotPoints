using System;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TrendByPivotPointsStrategy
{
    public class ScriptTrendByDonchian : IExternalScriptMultiSec
    {        
        public OptimProperty slowDonchian = new OptimProperty(55, 20, 60, 1);
        public OptimProperty fastDonchian = new OptimProperty(20, 10, 20, 1);
        public OptimProperty kAtr = new OptimProperty(2, 1, 2, 0.5);
        public OptimProperty atrPeriod = new OptimProperty(20, 14, 20, 1);
        public OptimProperty limitOpenedPositions = new OptimProperty(2, 1, 4, 1);

        public OptimProperty rateUSD = new OptimProperty(108, 1, 1000, 1);
        public OptimProperty positionSide = new OptimProperty(0, 1, 2, 1);
        public OptimProperty comission = new OptimProperty(0.565, 0.001, 100, 0.001);
        public OptimProperty mode = new OptimProperty(2, 0, 1, 1);
        public OptimProperty riskValuePrcnt = new OptimProperty(0.5, 0, 2, 0.5);
        public OptimProperty securityNumber = new OptimProperty(0, 0, 1, 1);
        public OptimProperty instrumentsGroup = new OptimProperty(0, 0, 3, 1);
        public OptimProperty isPaint = new OptimProperty(0, 0, 1, 1);
        public OptimProperty isLoggerOn = new OptimProperty(1, 0, 1, 1);
        public OptimProperty shares = new OptimProperty(1, 0, 1, 1);
        public OptimProperty isUSD = new OptimProperty(0, 0, 1, 1);

        public void Execute(IContext context, ISecurity[] securities)        
        {
            var logger = new LoggerSystem(context);
            logger.Log("Hello!");
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
                        system = new MainSystemForOptimizationDonchian();
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
            systemParameters.Add("slowDonchian", slowDonchian);
            systemParameters.Add("fastDonchian", fastDonchian);
            systemParameters.Add("kAtr", kAtr);
            systemParameters.Add("atrPeriod", atrPeriod);
            systemParameters.Add("limitOpenedPositions", limitOpenedPositions);

            systemParameters.Add("rateUSD", rateUSD);
            systemParameters.Add("positionSide", positionSide);
            systemParameters.Add("comission", comission);

            systemParameters.Add("riskValuePrcnt", riskValuePrcnt);
            systemParameters.Add("securityNumber", securityNumber);
            systemParameters.Add("instrumentsGroup", instrumentsGroup);
            systemParameters.Add("shares", shares);
            systemParameters.Add("isUSD", isUSD);

            try
            {
                system.SetParameters(systemParameters);
                system.Initialize(securities, context);
                system.Run();

                if ((int)isPaint == 1)
                    system.Paint();
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
        }
    }
}