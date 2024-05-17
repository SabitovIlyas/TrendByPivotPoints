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

            tradingSystems = new List<TradingSystem>();
            var tradingSystem = new TradingSystemSMA(securities, contractsManager, indicators, logger);
            tradingSystem.PositionSide = positionSide; //Остановился здесь. positionSide нужен для Converter'а. Подумать, где его передать.
            tradingSystem.SMAperiod = sma;
            tradingSystems.Add(tradingSystem);
        }

        public override void Paint()
        {
            throw new NotImplementedException();
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