using System;
using System.Collections.Generic;
using TradingSystems;
using TSLab.Script;

namespace TrendByPivotPointsStarter
{
    public class StarterDonchianTradingSystemLab : Starter
    {
        private double kAtr;
        private int limitOpenedPositions;
        private List<NonTradingPeriod> nonTradingPeriods;

        public StarterDonchianTradingSystemLab(Context context, List<Security> securities,
            Logger logger, List<NonTradingPeriod> nonTradingPeriods = null)
        {
            this.context = context;
            this.securities = securities;
            this.logger = logger;
            this.nonTradingPeriods = nonTradingPeriods;            
        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            this.systemParameters = systemParameters;
            try
            {
                base.SetParameters(systemParameters);                
                limitOpenedPositions = (int)systemParameters.GetValue("limitOpenedPositions");                
            }
            catch (KeyNotFoundException e)
            {
                logger.Log("Прекращаем работу, так как не установлен параметр: ", e.Message);
                throw new ApplicationException("Не удалось установить основные параметры для торговой системы.");
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            
            var baseCurrency = Currency.Ruble;
            account = new AccountLab(initDeposit: equity, baseCurrency, securities, logger);            
            
            var currencyConverter = new CurrencyConverter(baseCurrency);
            currencyConverter.AddCurrencyRate(Currency.USD, rateUSD);

            currency = securityFirst.Currency;
            ContractsManager contractsManager;
            if (contracts <= 0)
            {
                var riskManager = new RiskManagerReal(account, logger, riskValuePrcnt);
                contractsManager = new ContractsManager(riskManager, account, currency,
                currencyConverter, shares, logger);
            }
            else
            {
                contractsManager = new ContractsManager(contracts, account, currency,
                currencyConverter, shares, logger);
            }

            var indicators = new IndicatorsTsLab();

            tradingSystems = new List<TradingSystem>();
            var tradingSystem = new TradingSystemDonchian(securities, contractsManager,
                indicators, context, logger, nonTradingPeriods);

            tradingSystem.SetParameters(systemParameters);
            tradingSystem.Initialize();
            tradingSystems.Add(tradingSystem);            
        }       
    }
}