using System;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TrendByPivotPointsStarter
{
    public class SampleScript : IExternalScript
    {
        public OptimProperty rateUSD = new OptimProperty(61, 1, 200, 1);
        public OptimProperty positionSide = new OptimProperty(0, 1, 2, 1);  //0 -- для лонга, 1 -- для шорта, 2 -- для null
        public OptimProperty comission = new OptimProperty(0.565, 0.001, 100, 0.001);
        public OptimProperty mode = new OptimProperty(1, 0, 1, 1);          //0 -- для тестов в реальном времени, 1 -- для оптимизации и 2 -- для торговли
        public OptimProperty riskValuePrcnt = new OptimProperty(1, 0, 2, 0.5);
        public OptimProperty isPaint = new OptimProperty(0, 0, 1, 1);
        public OptimProperty isLoggerOn = new OptimProperty(0, 0, 1, 1);
        public OptimProperty shares = new OptimProperty(1, 0, 1, 1);
        public OptimProperty isUSD = new OptimProperty(0, 0, 1, 1);

        public void Execute(IContext context, ISecurity security)
        {            
            var logger = new LoggerSystem(context);
            MainSystem system = new SampleMainSystem();

            if (isLoggerOn == 1)
                system.Logger = new LoggerSystem(context);

            var systemParameters = new SystemParameters();

            systemParameters.Add("positionSide", positionSide);
            systemParameters.Add("isUSD", isUSD);
            systemParameters.Add("rateUSD", rateUSD);                        
            systemParameters.Add("shares", shares);
            systemParameters.Add("SMA", new OptimProperty(value: 13, minValue: 9, maxValue: 50, step: 1));

            var securities = new ISecurity[1];
            securities[0] = security;

            try
            {
                system.SetParameters(systemParameters);
                system.Initialize(securities, context); //подумать над тем, чтобы сюда передавать свой Security
                system.Run();

                if (isPaint == 1)
                    system.Paint();
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
        }
    }
}