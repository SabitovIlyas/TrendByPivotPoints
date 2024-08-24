using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using TSLab.Script.Handlers;
using TSLab.Script;

namespace TrendByPivotPointsStarter
{
    public class LabStarterDonchianTradingSystem : Starter
    {
        private double kAtr;
        private int limitOpenedPositions;

        public LabStarterDonchianTradingSystem(Context context, List<Security> securities,
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
                var positionSide = (int)systemParameters.GetValue("positionSide");                
                rateUSD = (double)systemParameters.GetValue("rateUSD");
                kAtr = (double)systemParameters.GetValue("kAtr");
                limitOpenedPositions = (int)systemParameters.GetValue("limitOpenedPositions");

                if (positionSide == 0)
                    base.positionSide = PositionSide.Long;
                else if (positionSide == 1)
                    base.positionSide = PositionSide.Short;
                else
                    base.positionSide = PositionSide.Null;                
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

            tradingSystem.Initialize();
            tradingSystems.Add(tradingSystem);            
        }        
    }
}