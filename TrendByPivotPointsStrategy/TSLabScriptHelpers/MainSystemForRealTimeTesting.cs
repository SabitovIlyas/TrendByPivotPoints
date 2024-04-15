using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public class MainSystemForRealTimeTesting : PivotPointsMainSystem
    {        
        Security securityFirst;
        IContext ctx;                

        static DateTime lastClosedBarDateTime = DateTime.MinValue;
        public override void Initialize(ISecurity[] securities, IContext ctx)
        {
            logger = new TsLabLogger(ctx);
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            account.Rate = rateUSD;

            var securityList = new List<Security>();

            this.securityFirst = new TSLabSecurity(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.USD);

            tradingSystems = new List<TradingSystem>();
            
            TradingSystemPivotPointsEMA ts;

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Long);   //sv-5min            
            ts.Logger = logger;
            tradingSystems.Add(ts);
            ts.SetParameters(5, 5, 10, 50);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new TSLabSecurity(securities[1]), PositionSide.Long); //br-5min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            ts.SetParameters(5, 5, 10, 50);

            account.Logger = logger;
            this.ctx = ctx;
            context = ContextTSLab.Create(ctx);
            account.Initialize(securityList);
        }
        
        public override void Run()
        {
            logger.SwitchOff();
            var localLogger = new TsLabLogger(ctx);

            foreach (var tradingSystem in tradingSystems)
                tradingSystem.CalculateIndicators();

            var lastBarNumber = securityFirst.GetBarsCountReal() - 1;
            if (lastBarNumber < 1)
                return;

            for (var i = 1; i < lastBarNumber; i++)
            {
                var lastClosedBarNumberInRealTrading = i - 1;
                foreach (var tradingSystem in tradingSystems)
                {
                    tradingSystem.Update(lastClosedBarNumberInRealTrading);
                    account.Update(lastClosedBarNumberInRealTrading);
                }
            }

            if (IsRealTimeTrading())
            {
                var prevLastBarNumber = lastBarNumber - 1;

                UpdateLoggerStatus(prevLastBarNumber);
                foreach (var tradingSystem in tradingSystems)
                    tradingSystem.Update(prevLastBarNumber);

                if (IsLastBarClosed())
                {
                    UpdateLoggerStatus(lastBarNumber);
                    foreach (var tradingSystem in tradingSystems)
                        tradingSystem.Update(lastBarNumber);
                }
            }
            else
            {
                foreach (var tradingSystem in tradingSystems)
                    tradingSystem.Update(lastBarNumber);

                account.Update(lastBarNumber);
            }
        }

        private void UpdateLoggerStatus(int barNumber)
        {
            securityFirst.BarNumber = barNumber;
            var dateTimePreviousLastBar = securityFirst.GetBarDateTime(barNumber);

            if (lastClosedBarDateTime < dateTimePreviousLastBar)
                logger.SwitchOn();
            else
                logger.SwitchOff();

            lastClosedBarDateTime = dateTimePreviousLastBar;
        }

        public void Paint(IContext ctx, ISecurity sec)
        {
            var tradingSystem = tradingSystems[securityNumber];
            tradingSystem.Paint(context);
        }

        private bool IsLaboratory(ISecurity security)
        {
            var realTimeSecurity = security as ISecurityRt;
            return realTimeSecurity == null;
        }

        private bool IsLastBarClosed()
        {
            return ctx.IsLastBarClosed;
        }
        private bool IsRealTimeTrading()
        {
            return securityFirst.IsRealTimeTrading;
        }       
    }
}