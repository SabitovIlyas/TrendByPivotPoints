using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;

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
            var tradingSystem = new TradingSystemDonchian(securities, contractsManager,
                indicators, context, logger, nonTradingPeriods);

            tradingSystem.SetParameters(systemParameters);
            tradingSystem.Initialize();
            tradingSystems.Add(tradingSystem);            
        }

        public override void PrintResults()
        {
            var profit = double.NaN;

            foreach (var sec in securities)            
                profit += sec.GetProfit();
            
            var security = securities.First();
            var drawdown = Account.GetMaxDrawDown();

            var recoveryFactor = profit / drawdown;
            var deals = security.GetDeals();
            var metaDeals = security.GetMetaDeals();
            var profitableDeals = metaDeals.Where(deal => deal.GetProfit() > 0).ToList();

            var log = string.Format("Прибыль: {0}\r\nПросадка: {1}\r\nФактор восстановления: {2}\r\n" +
                "Количество сделок: {3}\r\nКоличество прибыльных сделок:{4}\r\n", profit, drawdown,
                recoveryFactor, metaDeals.Count, profitableDeals);

            logger.Log(profit.ToString());
        }
    }
}