using System;
using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsStarter
{
    public class LabStarterSMATradingSystem : Starter
    {
        private int sma;
        public LabStarterSMATradingSystem(Logger logger)
        {            
            this.logger = logger;            
            SetParameters(systemParameters);
            Initialize();
        }

        public override void Initialize()
        {
            var security = new SecurityLab(currency, shares);
            var securities = new List<Security> { security };

            base.Initialize();

            var account = new AccountLab(initDeposit: 1000000, logger);
            var riskManager = new RiskManagerReal(account);
            var contractsManager = new ContractsManager(riskManager, account, securities);
            var indicators = new IndicatorsTsLab();
            
            //остановился здесь. Надо создать tradingSystems
        }

        public override void Paint()
        {
            throw new NotImplementedException();
        }

        public override void Run() //можно будет потом выделить этот метод в отдельный чистый класс Runner
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

        public void SetParameters()
        {
            systemParameters = new SystemParameters();
            systemParameters.Add("positionSide", PositionSide.Long);
            systemParameters.Add("isUSD", true);
            systemParameters.Add("rateUSD", 90);
            systemParameters.Add("shares", 10);            

            base.SetParameters(systemParameters);
            sma = 9;
        }
    }
}