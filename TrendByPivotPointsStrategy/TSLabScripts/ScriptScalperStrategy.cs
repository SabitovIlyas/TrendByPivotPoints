using System;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;
using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    public class ScriptScalperStrategy : IExternalScript
    {
        public OptimProperty rateUSD = new OptimProperty(61, 1, 200, 1);
        //TODO: надо сделать так, чтобы 0 был не для лонга, а для Null (None).
        public OptimProperty positionSide = new OptimProperty(0, 1, 2, 1);  //0 -- для лонга, 1 -- для шорта, 2 -- для null
        public OptimProperty comission = new OptimProperty(0.565, 0.001, 100, 0.001);
        public OptimProperty mode = new OptimProperty(1, 0, 1, 1);          //0 -- для тестов в реальном времени, 1 -- для оптимизации и 2 -- для торговли
        public OptimProperty isPaint = new OptimProperty(1, 0, 1, 1);
        public OptimProperty isLoggerOn = new OptimProperty(1, 0, 1, 1);
        public OptimProperty shares = new OptimProperty(1, 0, 1, 1);
        public OptimProperty isUSD = new OptimProperty(0, 0, 1, 1);
        public OptimProperty period = new OptimProperty(14, 5, 15, 1);
        public OptimProperty rsiBand = new OptimProperty(30, 25, 35, 1);

        public void Execute(IContext context, ISecurity security)
        {            
            var logger = new LoggerSystem(context);
            logger.Log("Запуск скрипта.");
            MainSystem system = new MainSystemScalper();

            if (isLoggerOn == 1)
                system.Logger = new LoggerSystem(context);

            var systemParameters = new SystemParameters();

            systemParameters.Add("rateUSD", rateUSD);
            systemParameters.Add("positionSide", positionSide);
            systemParameters.Add("comission", comission);
            systemParameters.Add("shares", shares);
            systemParameters.Add("isUSD", isUSD);
            systemParameters.Add("period", period);
            systemParameters.Add("rsiBand", rsiBand);

            var securities = new ISecurity[1];
            securities[0] = security;

            try
            {
                system.SetParameters(systemParameters);
                system.Initialize(securities, context);
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