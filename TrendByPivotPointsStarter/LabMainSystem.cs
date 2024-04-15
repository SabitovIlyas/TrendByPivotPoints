using System;
using System.Collections.Generic;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStarter
{
    public class LabMainSystem : MainSystem
    {        
        public LabMainSystem(List<TradingSystem> tradingSystems, SystemParameters systemParameters, List<Security> securities, Logger logger)
        {
            this.tradingSystems = tradingSystems;
            this.logger = logger;
            this.securities = securities;
            SetParameters(systemParameters);
            Initialize();
        }
        
        private void Initialize()
        {
            Initialize(securities);
        }

        public override void Initialize(List<Security> securities)
        {
            InitializeBase();            
        }

        public override void Paint()
        {
            throw new NotImplementedException();
        }

        public override void Run()
        {
            foreach (var tradingSystem in tradingSystems)
                tradingSystem.CalculateIndicators();

            var lastBarNumber = securityFirst.GetBarsCountReal() - 1;
            if (lastBarNumber < 1)
                return;

            for (var barNumber = 0; barNumber <= lastBarNumber; barNumber++)
            {
                foreach (var tradingSystem in tradingSystems)
                {
                    tradingSystem.Update(barNumber);
                    account.Update(barNumber);
                }
            }
        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            SetBaseParameters(systemParameters);
            var sma = systemParameters.GetValue("SMA");
        }
    }
}