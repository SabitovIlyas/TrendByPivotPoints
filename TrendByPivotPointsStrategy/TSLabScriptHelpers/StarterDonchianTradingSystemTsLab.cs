using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public class StarterDonchianTradingSystemTsLab : Starter
    {
        private IContext ctx;
        private double kAtr;
        private double limitOpenedPositions;

        public StarterDonchianTradingSystemTsLab(Logger logger)
        {
            this.logger = logger;            
            SetParameters(systemParameters);
            Initialize();
        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            kAtr = (double)systemParameters.GetValue("kAtr");
            limitOpenedPositions = (double)systemParameters.GetValue("limitOpenedPositions");
            base.SetParameters(systemParameters);
        }

        public override void Initialize()
        {
            var securityFirst = securities.First() as ISecurity;
            var context = new ContextTSLab(ctx);            
            var baseCurrency = Currency.Ruble;
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst, baseCurrency, logger);
            else
                account = new AccountTsLabRt(securityFirst, baseCurrency, logger);

            var securityList = new List<Security>();
            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var riskValuePrcntCalc = kAtr * limitOpenedPositions;
            if (riskValuePrcntCalc > riskValuePrcnt)
                throw new System.Exception("Превышен уровень риска");

            riskValuePrcnt = kAtr;
            var riskManager = new RiskManagerReal(account, logger, riskValuePrcnt);            
            var currencyConverter = new CurrencyConverter(baseCurrency);
            currencyConverter.AddCurrencyRate(Currency.USD, rateUSD);

            var contractsManager = new ContractsManager(riskManager, account, currency, 
                currencyConverter, shares, logger);            
            var indicators = new IndicatorsTsLab();

            tradingSystems = new List<TradingSystem>();
            var tradingSystem = new TradingSystemDonchian(securities, contractsManager,
                indicators, context, logger);

            tradingSystem.Initialize();            
            tradingSystems.Add(tradingSystem);

            //Остановился здесь.
            double totalComission;
            AbsolutCommission absoluteComission;
            totalComission = comission * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            securities[0].Commission = CalculateCommission;

            account.Initialize(securityList);
            logger.SwitchOff();
        }

        protected bool IsLaboratory(ISecurity security)
        {
            var realTimeSecurity = security as ISecurityRt;
            return realTimeSecurity == null;
        }

        private double CalculateCommission(IPosition pos, double price, double shares, bool isEntry, bool isPart)
        {
            var exchangeCommission = price * comission;
            var brokerCommission = exchangeCommission;
            var totalCommission = exchangeCommission + brokerCommission;
            var reserve = 0.25 * totalCommission;

            return totalCommission + reserve;
        }

        public override void Run()
        {
            foreach (var tradingSystem in tradingSystems)
                tradingSystem.CalculateIndicators();

            var lastBarNumber = securityFirst.GetBarsCountReal() - 1;
            if (lastBarNumber < 1)
                return;

            for (var i = 0; i <= lastBarNumber; i++)
            {
                foreach (var tradingSystem in tradingSystems)
                {
                    tradingSystem.Update(i);
                    account.Update(i);
                }
            }
        }

        public void Paint(IContext ctx, ISecurity sec)
        {
            var firstTradingSystem = tradingSystems.First();
            firstTradingSystem.Paint(context);
        }        

        private bool IsLastBarClosed()
        {
            return ctx.IsLastBarClosed;
        }
        private bool IsRealTimeTrading()
        {
            return securityFirst.IsRealTimeTrading;
        }        

        public override void Paint()
        {
            throw new System.NotImplementedException();
        }
    }
}