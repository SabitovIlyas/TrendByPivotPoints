using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class MainSystemForTradingDonchian : PivotPointsStarter
    {
        private IContext ctx;
        private double kAtr;
        private double limitOpenedPositions;

        public override void Initialize(ISecurity[] securities, IContext ctx)
        {
            var logger = Logger;
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var riskValuePrcntCalc = kAtr * limitOpenedPositions;
            if (riskValuePrcntCalc > riskValuePrcnt)
                throw new System.Exception("Превышен уровень риска");

            riskValuePrcnt = kAtr;
            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;

            Currency currency;
            if (isUSD == 0)
                currency = Currency.Ruble;
            else
                currency = Currency.USD;

            account.Rate = rateUSD;
            var localMoneyManagerRuble = new ContractsManager(globalMoneyManager, account, currency, shares);

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

        public override void SetParameters(SystemParameters systemParameters)
        {
            kAtr = systemParameters.GetDouble("kAtr");
            limitOpenedPositions = systemParameters.GetDouble("limitOpenedPositions");
            base.SetParameters(systemParameters);
        }
    }
}