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

        public override void Initialize()
        {
            var security = new SecurityLab(currency, shares, logger);
            securities = new List<Security> { security };
            var context = new ContextLab();
            var baseCurrency = Currency.Ruble;

            base.Initialize();

            account = new AccountLab(initDeposit: 1000000, baseCurrency, securities, logger);
            var riskManager = new RiskManagerReal(account, logger);

            var currencyConverter = new CurrencyConverter(baseCurrency);
            currencyConverter.AddCurrencyRate(Currency.USD, rateUSD);
            var contractsManager = new ContractsManager(riskManager, account, currency,
                currencyConverter, logger);
            var indicators = new IndicatorsTsLab();

            tradingSystems = new List<TradingSystem>();
            var tradingSystem = new TradingSystemSMA(securities, contractsManager,
                indicators, context, logger);
            tradingSystem.PositionSide = positionSide;
            tradingSystem.SMAperiod = sma;
            tradingSystem.Initialize();
            tradingSystems.Add(tradingSystem);            
        }

        public override void Paint()
        {
            throw new NotImplementedException();
        }        
    }
}