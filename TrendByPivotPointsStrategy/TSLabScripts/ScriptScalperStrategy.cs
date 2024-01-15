using System;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;
using TSLab.Script;

namespace TradingSystems
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
        public OptimProperty periodRsiAndAtr = new OptimProperty(14, 5, 15, 1);
        public OptimProperty bandRsi = new OptimProperty(30, 25, 35, 1);       
        public OptimProperty periodAdx = new OptimProperty(20, 20, 40, 1);
        public OptimProperty bandAdx = new OptimProperty(20, 20, 40, 1);

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
            systemParameters.Add("periodRsiAndAtr", periodRsiAndAtr);
            systemParameters.Add("bandRsi", bandRsi);
            systemParameters.Add("periodAdx", periodAdx);
            systemParameters.Add("bandAdx", bandAdx);

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