using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public class StarterDonchianTradingSystem : Starter
    {
        private IContext ctx;
        private double kAtr;
        private double limitOpenedPositions;

        public StarterDonchianTradingSystem(Logger logger)
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
            var baseCurrency = Currency.Ruble;
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst, baseCurrency);
            else
                account = new AccountTsLabRt(securityFirst, baseCurrency);

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
            //Остановился здесь. Надо связать CurrenceConverter, наверное, с Security.
            var localMoneyManagerRuble = new ContractsManager(riskManager, account, currency, shares);

            tradingSystems = new List<TradingSystem>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystem ts;

            ts = new TradingSystemDonchian(localMoneyManagerRuble, account, this.securityFirst, (PositionSide)((int)positionSide));//si-5min            
            localMoneyManagerRuble.Logger = logger;
            ts.Logger = logger;
            tradingSystems.Add(ts);
            ts.Initialize(ctx);
            ts.SetParameters();

            totalComission = comission * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            securities[0].Commission = CalculateCommission;

            account.Logger = logger;
            this.ctx = ctx;
            context = ContextTSLab.Create(ctx);
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