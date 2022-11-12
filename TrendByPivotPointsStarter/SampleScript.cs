﻿using System;
using TrendByPivotPointsStrategy;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TrendByPivotPointsStarter
{
    public class SampleScript : IExternalScript
    {
        public OptimProperty rateUSD = new OptimProperty(108, 1, 1000, 1);
        public OptimProperty positionSide = new OptimProperty(0, 1, 2, 1);
        public OptimProperty comission = new OptimProperty(0.565, 0.001, 100, 0.001);
        public OptimProperty mode = new OptimProperty(2, 0, 1, 1);
        public OptimProperty riskValuePrcnt = new OptimProperty(0.5, 0, 2, 0.5);
        public OptimProperty isPaint = new OptimProperty(0, 0, 1, 1);
        public OptimProperty isLoggerOn = new OptimProperty(1, 0, 1, 1);
        public OptimProperty shares = new OptimProperty(1, 0, 1, 1);
        public OptimProperty isUSD = new OptimProperty(0, 0, 1, 1);

        public void Execute(IContext context, ISecurity security)
        {
            var logger = new LoggerSystem(context);
            MainSystem system = new SampleMainSystem();

            if ((int)isLoggerOn == 1)
                system.Logger = new LoggerSystem(context);

            var systemParameters = new SystemParameters();

            systemParameters.Add("rateUSD", rateUSD);
            systemParameters.Add("positionSide", positionSide);
            systemParameters.Add("comission", comission);
            systemParameters.Add("riskValuePrcnt", riskValuePrcnt);
            systemParameters.Add("shares", shares);
            systemParameters.Add("isUSD", isUSD);

            var securities = new ISecurity[1];
            securities[0] = security;

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