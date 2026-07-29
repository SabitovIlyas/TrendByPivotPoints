using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;

namespace TrendByPivotPointsStarter
{
    /// <summary>
    /// Лабораторный стартер стратегии возврата к среднему (RSI + SMA-фильтр + ATR-стоп).
    /// </summary>
    public class StarterMeanReversionTradingSystemLab : Starter
    {
        public StarterMeanReversionTradingSystemLab(Context context, List<Security> securities,
            Logger logger, List<NonTradingPeriod> nonTradingPeriods = null)
        {
            this.context = context;
            this.securities = securities;
            this.logger = logger;
            NonTradingPeriods = nonTradingPeriods;
        }

        public StarterMeanReversionTradingSystemLab GetClone()
        {
            var securities = new List<Security>();
            foreach (var security in this.securities)
                securities.Add(security.GetClone());

            var nonTradingPeriods = new List<NonTradingPeriod>();
            if (NonTradingPeriods != null)
                foreach (var period in NonTradingPeriods)
                    nonTradingPeriods.Add(period);

            return new StarterMeanReversionTradingSystemLab(context, securities, logger,
                nonTradingPeriods);
        }

        public override Starter CloneStarter()
        {
            return GetClone();
        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            this.systemParameters = systemParameters;
            try
            {
                base.SetParameters(systemParameters);
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

            var baseCurrency = Currency.RUB;
            Account = new AccountLab(initDeposit: equity, baseCurrency, securities, logger);

            var currencyConverter = new CurrencyConverter(baseCurrency);
            currencyConverter.AddCurrencyRate(Currency.USD, rateUSD);

            currency = securityFirst.Currency;
            ContractsManager contractsManager;
            if (contracts <= 0)
            {
                var riskManager = new RiskManagerReal(Account, logger, riskValuePrcnt);
                contractsManager = new ContractsManager(riskManager, Account, currency,
                    currencyConverter, shares, logger);
            }
            else
            {
                contractsManager = new ContractsManager(contracts, Account, currency,
                    currencyConverter, shares, logger);
            }

            var indicators = new IndicatorsTsLab();

            tradingSystems = new List<TradingSystem>();
            var tradingSystem = new TradingSystemMeanReversion(securities, contractsManager,
                indicators, context, logger, NonTradingPeriods);

            tradingSystem.SetParameters(systemParameters);
            tradingSystem.Initialize();
            tradingSystems.Add(tradingSystem);
        }

        public override void PrintResults()
        {
            var profit = 0d;
            foreach (var sec in securities)
                profit += sec.GetProfit();

            logger.Log(profit.ToString());
        }
    }
}
