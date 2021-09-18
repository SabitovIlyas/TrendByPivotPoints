using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TrendByPivotPointsStrategy
{
    public class MainSystemForRealTimeTesting : MainSystem
    {
        TradingSystemPivotPointsEMA tradingSystem1;
        Security securityFirst;
        IContext ctx;
        ContextTSLab context;
        Account account;
        List<TradingSystemPivotPointsEMA> tradingSystems;
        Logger logger;
        int securityNumber;

        static DateTime lastClosedBarDateTime = DateTime.MinValue;
        public void Initialize(ISecurity[] securities, IContext ctx)
        {
            logger = new LoggerSystem(ctx);
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);
            
            account.Rate = rateUSD;

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.USD);            

            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Long);   //sv-5min            
            ts.Logger = logger;
            tradingSystems.Add(ts);            
            ts.SetParameters(5, 5, 10, 50);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[1]), PositionSide.Long); //br-5min
            ts.Logger = logger;
            tradingSystems.Add(ts);            
            ts.SetParameters(5, 5, 10, 50);
           
            account.Logger = logger;
            this.ctx = ctx;
            context = new ContextTSLab(ctx);
            account.Initialize(securityList);
        }

        bool leSeNullPreviousBar = false;
        public void Run()
        {
            logger.SwitchOff();
            var localLogger = new LoggerSystem(ctx);

            foreach (var tradingSystem in tradingSystems)
                tradingSystem.CalculateIndicators();

            var lastBarNumber = securityFirst.GetBarsCount() - 1;
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

                foreach (var tradingSystem in tradingSystems)                
                    tradingSystem.CheckPositionCloseCase(lastBarNumber);                
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

        public void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide,
            double rateUSD, double positionSide, double comission, double riskValuePrcnt, int securityNumber, int instrumentsGroup)
        {
            this.leftLocalSide = leftLocalSide;
            this.rightLocalSide = rightLocalSide;
            this.pivotPointBreakDownSide = pivotPointBreakDownSide;
            this.EmaPeriodSide = EmaPeriodSide;
            this.rateUSD = rateUSD;
            this.positionSide = positionSide;
            this.comission = comission;
            this.riskValuePrcnt = riskValuePrcnt;
            this.securityNumber = securityNumber;
        }

        private double leftLocalSide;
        private double rightLocalSide;
        private double pivotPointBreakDownSide;
        private double EmaPeriodSide;
        private double rateUSD;
        private double positionSide;
        private double comission;
        private double riskValuePrcnt;
    }
}