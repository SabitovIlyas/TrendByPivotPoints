using System;
using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsStarter
{
    public class StarterDonchianTradingSystemLab : Starter
    {
        private double kAtr;
        private int limitOpenedPositions;

        public StarterDonchianTradingSystemLab(Context context, List<Security> securities,
            Logger logger)
        {
            this.context = context;
            this.securities = securities;
            this.logger = logger;
        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            this.systemParameters = systemParameters;
            try
            {
                base.SetParameters(systemParameters);
                kAtr = (double)systemParameters.GetValue("kAtr");
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
            account = new AccountLab(initDeposit: 1000000, baseCurrency, securities, logger);
            var riskValuePrcntCalc = kAtr * limitOpenedPositions;           

            riskValuePrcnt = kAtr;
            var riskManager = new RiskManagerReal(account, logger, riskValuePrcnt);
            var currencyConverter = new CurrencyConverter(baseCurrency);
            currencyConverter.AddCurrencyRate(Currency.USD, rateUSD);

            currency = securityFirst.Currency;
            var contractsManager = new ContractsManager(riskManager, account, currency,
                currencyConverter, shares, logger);
            var indicators = new IndicatorsTsLab();

            tradingSystems = new List<TradingSystem>();
            var tradingSystem = new TradingSystemDonchian(securities, contractsManager,
                indicators, context, logger);

            tradingSystem.SetParameters(systemParameters);
            tradingSystem.Initialize();
            tradingSystems.Add(tradingSystem);            
        }        
    }
}