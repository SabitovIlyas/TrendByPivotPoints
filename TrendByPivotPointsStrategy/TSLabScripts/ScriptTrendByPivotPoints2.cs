﻿using System;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TradingSystems
{
    public class ScriptTrendByPivotPoints2 : IExternalScriptMultiSec
    {        
        public OptimProperty emaFast = new OptimProperty(20, 20, 200, 20);
        public OptimProperty emaSlow = new OptimProperty(20, 20, 200, 20);
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
            var logger = new LoggerSystem(context);
            logger.Log("Hello!");
            PivotPointsMainSystem system;

            switch ((int)mode)
            {
                case 0:
                    {
                        system = new MainSystemForRealTimeTesting();
                        break;
                    }
                case 1:
                    {
                        system = new MainSystemForOptimization2();
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

            systemParameters.Add("emaFast", emaFast);
            systemParameters.Add("emaSlow", emaSlow);
            
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
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
        }
    }
}