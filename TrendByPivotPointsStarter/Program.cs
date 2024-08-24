using System;
using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsStarter
{
    class Program
    {
        static void Main(string[] args)
        {            
            var logger = new ConsoleLogger();            

            try
            {
                var context = new ContextLab();
                var security = new SecurityLab(Currency.USD, shares: 10,
                    5000, 4500);
                var securities = new List<Security>() { security };
                var system = new LabStarterDonchianTradingSystem(context, securities, logger);
                var systemParameters = GetSystemParameters();
                system.SetParameters(systemParameters);
                system.Initialize();
                system.Run();                
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
        }

        private static SystemParameters GetSystemParameters()
        {
            var systemParameters = new SystemParameters();

            systemParameters.Add("slowDonchian", 50);
            systemParameters.Add("fastDonchian", 20);
            systemParameters.Add("kAtr", 2);
            systemParameters.Add("atrPeriod", 20);

            systemParameters.Add("limitOpenedPositions", 4);
            systemParameters.Add("rateUSD", 100);
            systemParameters.Add("positionSide", 0);

            return systemParameters;
        }
    }
}