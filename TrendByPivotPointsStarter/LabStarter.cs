using System;
using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsStarter
{
    public class LabStarter : Starter
    {        
        public LabStarter(List<TradingSystem> tradingSystems, SystemParameters systemParameters, List<Security> securities, Logger logger)
        {
            this.tradingSystems = tradingSystems;
            this.logger = logger;
            this.securities = securities;
            SetParameters(systemParameters);
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();

            Security security = new SecurityLab(GetCurrency(), shares);
            List<Security> securities = new List<Security> { security };
            Logger logger = new ConsoleLogger();
            Account account = new AccountLab(initDeposit: 1000000, logger);
            RiskManager riskManager = new RiskManagerReal(account);
            ContractsManager contractsManager = new ContractsManager(riskManager, account, securities);
            Indicators indicators = new IndicatorsTsLab();

            SystemParameters systemParameters = new SystemParameters();

            //systemParameters.Add("positionSide", PositionSide.Long);
            //systemParameters.Add("isUSD", true);
            //systemParameters.Add("rateUSD", 90);
            //systemParameters.Add("shares", 10);
            //systemParameters.Add("SMA", 9);
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

        public override void SetParameters(SystemParameters systemParameters)
        {
            base.SetParameters(systemParameters);
            var sma = systemParameters.GetValue("SMA");           
        }
    }
}