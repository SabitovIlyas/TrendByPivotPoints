using System;
using System.Collections.Generic;
using TSLab.DataSource;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TradingSystems
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
            if ((int)fastDonchian > (int)slowDonchian)
                return;

            Logger logger = new TsLabLogger(context);
            Starter system;

            if ((int)isLoggerOn == 1)
                logger = new TsLabLogger(context);
            else
                logger = new LoggerNull();

            switch ((int)mode)
            {
                case 0:
                    {
                        //Реалтайм тестирование
                        break;
                    }
                case 1:
                    {
                        //Оптимизация
                        break;
                    }
                default:
                    {
                        //Реалтайм торговля
                        break;
                    }
            }

            system = new StarterDonchianTradingSystemTsLab(context, securities, logger);
            var systemParameters = new SystemParameters();       
            
            systemParameters.Add("slowDonchian", (int)slowDonchian);
            systemParameters.Add("fastDonchian", (int)fastDonchian);
            systemParameters.Add("kAtr", (double)kAtr);
            systemParameters.Add("atrPeriod", (int)atrPeriod);

            systemParameters.Add("limitOpenedPositions", (int)limitOpenedPositions);
            systemParameters.Add("rateUSD", (double)rateUSD);
            systemParameters.Add("positionSide", (int)positionSide);
            systemParameters.Add("comission", (double)comission);

            systemParameters.Add("riskValuePrcnt", (double)riskValuePrcnt);
            systemParameters.Add("securityNumber", (int)securityNumber);
            systemParameters.Add("instrumentsGroup", (int)instrumentsGroup);
            systemParameters.Add("shares", (int)shares);

            systemParameters.Add("isUSD", (int)isUSD);

            try
            {
                system.SetParameters(systemParameters);
                system.Initialize();
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