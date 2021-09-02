﻿using System.Collections.Generic;
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

        static int lastClosedBarNumber = -1;
        public void Initialize(ISecurity[] securities, IContext ctx)
        {
            logger = new LoggerSystem(ctx);
            //var logger = new NullLogger();
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

            //tradingSystems = new List<TradingSystemPivotPointsTwoTimeFrames>();
            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            //logger.Log("Создание торговой системы...");

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Long);   //si-5min            
            ts.Logger = logger;
            tradingSystems.Add(ts);            
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            ts.SetParameters(13, 1, 10, 60);

            //logger.Log("Торговая система успешно создана!");

            //ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[1]), PositionSide.Long); //sbrf-5min
            //ts.Logger = logger;
            //tradingSystems.Add(ts);
            //totalComission = 2.12 * 2;
            //absoluteComission = new AbsolutCommission() { Commission = totalComission };
            //absoluteComission.Execute(securities[1]);
            //ts.SetParameters(13, 13, 60, 20);


            ////tradingSystems.Add(new TradingSystemPivotPointsTwoTimeFrames(localMoneyManagerRuble, account, new Se)curityTSlab(securities[1])));
            //ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[1]));//sbrf-5min
            //tradingSystems.Add(ts);
            //ts.Logger = logger;
            //tradingSystems.Add(ts);
            //comission = 2.02 * 2;
            ////comission = 2 * 2;
            //comis = new AbsolutCommission() { Commission = comission };
            //comis.Execute(securities[1]);

            //ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[2]));//gazr-5min
            //tradingSystems.Add(ts);
            //ts.Logger = logger;
            //tradingSystems.Add(ts);
            //comission = 2.02 * 2;
            ////comission = 2 * 2;
            //comis = new AbsolutCommission() { Commission = comission };
            //comis.Execute(securities[1]);

            //ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[3]));//lkoh-5min
            //tradingSystems.Add(ts);
            //ts.Logger = logger;
            //tradingSystems.Add(ts);
            //comission = 2.02 * 2;
            ////comission = 2 * 2;
            //comis = new AbsolutCommission() { Commission = comission };
            //comis.Execute(securities[1]);

            //ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[4]));//mxi-5min
            //tradingSystems.Add(ts);
            //ts.Logger = logger;
            //tradingSystems.Add(ts);
            //comission = 2.02 * 2;
            ////comission = 2 * 2;
            //comis = new AbsolutCommission() { Commission = comission };
            //comis.Execute(securities[1]);

            //ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[5]));//silv-5min
            //tradingSystems.Add(ts);
            //ts.Logger = logger;
            //tradingSystems.Add(ts);
            //comission = 2.02 * 2;
            ////comission = 2 * 2;
            //comis = new AbsolutCommission() { Commission = comission };
            //comis.Execute(securities[1]);




            //tradingSystem.Logger = logger;
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
                //localLogger.Log("1: " + ((SecurityTSlab)securityFirst).security.Bars.Last().Close.ToString());
                //localLogger.Log("2: " + securityFirst.GetBarClose(securityFirst.GetBarsCount() - 2).ToString());
                //localLogger.Log(securityFirst.LastBar.Close.ToString());

                var lbc = ((SecurityTSlab)securityFirst).security.Bars.Count - 1;
                var lb = ((SecurityTSlab)securityFirst).security.Bars[lbc];
                localLogger.Log("low: " + lbc.ToString() + "; " + lb.Low.ToString());
                localLogger.Log("high: " + lbc.ToString() + "; " + lb.High.ToString());

                //securityFirst.BarNumber = 249;
                //var lbc2 = securityFirst.GetBarsCount() - 1;
                //var lbd2 = securityFirst.GetBarClose(lbc2).ToString();
                //localLogger.Log("2: " + lbc2.ToString() + "; " + lbd2.ToString());


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
                {
                    localLogger.Log("CheckPositionCloseCase");
                    tradingSystem.CheckPositionCloseCase(lastBarNumber); 
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
            //if (lastClosedBarNumber < barNumber)
            //    logger.SwitchOn();
            //else
            //    logger.SwitchOff();

            //lastClosedBarNumber = barNumber;
        }

       

        public void Paint(IContext ctx, ISecurity sec)
        {
            //var firstTradingSystem = tradingSystems.First();
            //firstTradingSystem.Paint(context);

            var tradingSystem = tradingSystems[securityNumber];
            tradingSystem.Paint(context);

            //var lastTradingSystem = tradingSystems.Last();
            //lastTradingSystem.Paint(context);
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
            double rateUSD, double positionSide, double comission, double riskValuePrcnt, int securityNumber)
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
