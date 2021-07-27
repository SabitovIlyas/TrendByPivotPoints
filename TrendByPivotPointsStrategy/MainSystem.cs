﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.DataSource;
using TSLab.Script.Realtime;

namespace TrendByPivotPointsStrategy
{
    public class MainSystem
    {
        //TradingSystemPivotPointsTwoTimeFrames tradingSystem1;
        TradingSystemPivotPointsEMA tradingSystem1;
        Security securityFirst;
        IContext ctx;
        ContextTSLab context;
        Account account;
        //List<TradingSystemPivotPointsTwoTimeFrames> tradingSystems;
        List<TradingSystemPivotPointsEMA> tradingSystems;
        public void Initialize(ISecurity[] securities, IContext ctx)
        {
            var logger = new LoggerSystem(ctx);
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 1.00);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
                        
            //tradingSystems = new List<TradingSystemPivotPointsTwoTimeFrames>();
            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double comission;
            AbsolutCommission comis;
            TradingSystemPivotPointsEMA ts;

            //tradingSystems.Add(new TradingSystemPivotPointsTwoTimeFrames(localMoneyManagerRuble, account, this.securityFirst));            
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst);//si-5min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            comission = 1.15 * 2;
            //comission = 1 * 2;
            comis = new AbsolutCommission() { Commission = comission };
            comis.Execute(securities[0]);

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
            var acc = account as AccountLab;
            acc.Initialize(securityList);
        }

        bool leSeNullPreviousBar = false;        
        public void Run()
        {
            foreach(var tradingSystem in tradingSystems)            
                tradingSystem.CalculateIndicators();            
            
            var lastBarNmber = securityFirst.GetBarsCount() - 1;

            for (var i = 0; i < lastBarNmber; i++)
            {
                foreach (var tradingSystem in tradingSystems)
                {                    
                    tradingSystem.Update(i);

                    var sec = securityFirst as SecurityTSlab;

                    var le = sec.security.Positions.GetLastActiveForSignal("LE", i);
                    var se = sec.security.Positions.GetLastActiveForSignal("SE", i);


                    var leSeNullCurrentBar = (le == null) && (se == null);
                    var moneyBefore = account.Equity;

                    account.Update(i);

                    var moneyAfter = account.Equity;
                    var br = (leSeNullCurrentBar && leSeNullPreviousBar && (moneyAfter != moneyBefore));

                    leSeNullPreviousBar = leSeNullCurrentBar;
                }
            }

            if (IsRealTimeTrading())
            {
                foreach (var tradingSystem in tradingSystems)
                    tradingSystem.CheckPositionCloseCase(lastBarNmber);

                if (IsLastBarClosed())                
                    foreach (var tradingSystem in tradingSystems)
                        tradingSystem.Update(lastBarNmber);                
            }
            else
                foreach (var tradingSystem in tradingSystems)
                    tradingSystem.Update(lastBarNmber);

            account.Update(lastBarNmber);
        }

        public void Paint(IContext ctx, ISecurity sec)
        {
            var firstTradingSystem = tradingSystems.First();
            firstTradingSystem.Paint(context);
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
    }
}