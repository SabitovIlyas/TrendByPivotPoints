using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TrendByPivotPointsStrategy
{
    public class MainSystemForTrading : MainSystem
    {
        //TradingSystemPivotPointsTwoTimeFrames tradingSystem1;
        TradingSystemPivotPointsEMA tradingSystem1;
        Security securityFirst;
        IContext ctx;
        ContextTSLab context;
        Account account;
        //List<TradingSystemPivotPointsTwoTimeFrames> tradingSystems;
        List<TradingSystemPivotPointsEMA> tradingSystems;
        Logger logger;
        int securityNumber;
        int instrumentsGroup;

        static DateTime lastClosedBarDateTime = DateTime.MinValue;

        public void Initialize(ISecurity[] securities, IContext ctx)
        {
            logger = new LoggerSystem(ctx);
            //var logger = new NullLogger();

            List<Security> securityList = null;
            
            switch (instrumentsGroup)
            {
                case 0:
                    {
                        securityList = Initialize5minRubleScript(securities);
                        break;
                    }
                case 1:
                    {
                        securityList = Initialize15minRubleScript(securities);
                        break;
                    }
                case 2:
                    {
                        securityList = Initialize5minUSDScript(securities);
                        break;
                    }
                case 3:
                    {
                        securityList = Initialize15minUSDScript(securities);
                        break;
                    }

            }

            //tradingSystem.Logger = logger;
            account.Logger = logger;
            account.Rate = rateUSD;
            this.ctx = ctx;
            context = new ContextTSLab(ctx);
            account.Initialize(securityList);
        }

        private List<Security> Initialize15minUSDScript(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new LocalMoneyManager(globalMoneyManager, account, Currency.USD);
            localMoneyManagerUSD.Logger = logger;

            var localMoneyManagerUSD10Shares = new LocalMoneyManager(globalMoneyManager, account, Currency.USD, 10);
            localMoneyManagerUSD10Shares.Logger = logger;

            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, this.securityFirst, PositionSide.Null);   //brent-15min long      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0033 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            ts.SetParameters(10, 13, 40, 100);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[1]), PositionSide.Null);   //gold-15min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.04 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[1]);
            ts.SetParameters(1, 13, 10, 120);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSlab(securities[2]), PositionSide.Null);   //silver-15min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0011 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[2]);
            ts.SetParameters(10, 13, 40, 100);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSlab(securities[3]), PositionSide.Null);   //brent-15min short      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0033 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[3]);
            ts.SetParameters(1, 16, 40, 40);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[4]), PositionSide.Null);   //gold-15min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.04 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[4]);
            ts.SetParameters(13, 1, 10, 40);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSlab(securities[5]), PositionSide.Null);   //silver-15min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0011 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[5]);
            ts.SetParameters(1, 16, 40, 40);

            return securityList;
        }

        private List<Security> Initialize5minUSDScript(ISecurity[] securities)
        {            
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new LocalMoneyManager(globalMoneyManager, account, Currency.USD); //заменить на USD
            localMoneyManagerUSD.Logger = logger;

            var localMoneyManagerUSD10Shares = new LocalMoneyManager(globalMoneyManager, account, Currency.USD, 10); //заменить на USD            
            localMoneyManagerUSD10Shares.Logger = logger;

            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, this.securityFirst, PositionSide.Null);   //brent-5min long      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0033 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            ts.SetParameters(4, 13, 70, 140);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[1]), PositionSide.Null);   //gold-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.04 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[1]);
            ts.SetParameters(7, 13, 70, 160);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSlab(securities[2]), PositionSide.Long);   //silver-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0011 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[2]);
            ts.SetParameters(10, 4, 90, 160);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSlab(securities[3]), PositionSide.Null);   //brent-5min short      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0033 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[3]);
            ts.SetParameters(13, 10, 70, 100);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[4]), PositionSide.Null);   //gold-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.04 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[4]);
            ts.SetParameters(16, 16, 10, 20);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSlab(securities[5]), PositionSide.Null);   //silver-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0011 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[5]);
            ts.SetParameters(13, 16, 90, 180);

            return securityList;
        }

        private List<Security> Initialize15minRubleScript(ISecurity[] securities)
        {            
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
                        
            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Long);   //eu-15min long      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            ts.SetParameters(4, 4, 40, 80);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[1]), PositionSide.Long);   //gz-15min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[1]);
            ts.SetParameters(4, 4, 10, 180);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[2]), PositionSide.Long);   //lkoh-15min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[2]);
            ts.SetParameters(4, 16, 30, 120);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[3]), PositionSide.Long); //sbrf-15min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[3]);
            ts.SetParameters(1, 16, 90, 100);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[4]), PositionSide.Long); //si-15min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[4]);
            ts.SetParameters(1, 16, 100, 160);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[5]), PositionSide.Long); //vtbr-15min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[5]);
            ts.SetParameters(7, 16, 10, 160);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[6]), PositionSide.Short);   //eu-15min short      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[6]);
            ts.SetParameters(16, 1, 50, 20);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[7]), PositionSide.Short);   //gz-15min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[7]);
            ts.SetParameters(19, 4, 40, 100);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[8]), PositionSide.Short);   //lkoh-15min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[8]);
            ts.SetParameters(4, 19, 20, 200);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[9]), PositionSide.Short); //sbrf-15min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[9]);
            ts.SetParameters(13, 1, 10, 20);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[10]), PositionSide.Short); //si-15min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[10]);
            ts.SetParameters(13, 1, 20, 20);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[11]), PositionSide.Short); //vtbr-15min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[11]);
            ts.SetParameters(19, 7, 10, 160);

            return securityList;
        }

        private List<Security> Initialize5minRubleScript(ISecurity[] securities)
        {            
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
                        
            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Null);   //eu-5min long      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            ts.SetParameters(10, 10, 10, 180);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[1]), PositionSide.Null);   //gz-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[1]);
            ts.SetParameters(16, 16, 80, 200);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[2]), PositionSide.Null);   //lkoh-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[2]);
            ts.SetParameters(10, 16, 80, 160);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[3]), PositionSide.Null); //sbrf-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[3]);
            ts.SetParameters(13, 13, 60, 20);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[4]), PositionSide.Long); //si-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[4]);
            ts.SetParameters(13, 1, 10, 60);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[5]), PositionSide.Null); //vtbr-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[5]);
            ts.SetParameters(16, 1, 80, 180);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[6]), PositionSide.Null);   //eu-5min short      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[6]);
            ts.SetParameters(13, 4, 100, 40);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[7]), PositionSide.Null);   //gz-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[7]);
            ts.SetParameters(16, 4, 10, 100);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[8]), PositionSide.Null);   //lkoh-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[8]);
            ts.SetParameters(13, 16, 30, 120);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[9]), PositionSide.Null); //sbrf-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[9]);
            ts.SetParameters(4, 16, 10, 120);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[10]), PositionSide.Null); //si-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[10]);
            ts.SetParameters(16, 4, 70, 40);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[11]), PositionSide.Null); //vtbr-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[11]);
            ts.SetParameters(16, 16, 50, 180);

            return securityList;
        }

        bool leSeNullPreviousBar = false;
        public void Run()
        {
            //logger.SwitchOff();
            //var localLogger = new LoggerSystem(ctx);

            foreach (var tradingSystem in tradingSystems)
                tradingSystem.CalculateIndicators();

            var lastBarNumber = securityFirst.GetBarsCount() - 1;
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
            //var firstTradingSystem = tradingSystems.First();
            //firstTradingSystem.Paint(context);

            //var tradingSystem = tradingSystems[0];
            //tradingSystem.Paint(context);

            //tradingSystem = tradingSystems[1];
            //tradingSystem.Paint(context);

            //var lastTradingSystem = tradingSystems.Last();
            //lastTradingSystem.Paint(context);

            foreach(var tradingSystem in tradingSystems)
            {
                if (tradingSystem.PositionSide == PositionSide.Long || tradingSystem.PositionSide == PositionSide.Short)
                    tradingSystem.Paint(context);
            }
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
            this.instrumentsGroup = instrumentsGroup;
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
