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
            this.ctx = ctx;

            switch (instrumentsGroup)
            {
                case 0:
                    {
                        securityList = Initialize5minRubleScript(securities);   //не торгую
                        break;
                    }
                case 1:
                    {
                        securityList = Initialize15minRubleScript(securities);
                        break;
                    }
                case 2:
                    {
                        securityList = Initialize5minUSDScript(securities); //не торгую
                        break;
                    }
                case 3:
                    {
                        securityList = Initialize15minUSDScript(securities);
                        break;
                    }
                case 4:
                    {
                        securityList = Initialize15minRubleScript10Shares(securities);
                        break;
                    }
                case 5:
                    {
                        securityList = Initialize15minUSDScript10Shares(securities);
                        break;
                    }
                case 6:
                    {
                        securityList = Initialize15minUSDScript1000Shares(securities);
                        break;
                    }
            }

            //tradingSystem.Logger = logger;
            account.Logger = logger;
            account.Rate = rateUSD;            
            context = new ContextTSLab(ctx);
            account.Initialize(securityList);
        }

        private List<Security> Initialize15minUSDScript1000Shares(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 0.1);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new LocalMoneyManager(globalMoneyManager, account, Currency.USD, shares: 1000);
            localMoneyManagerUSD.Logger = logger;

            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            //Security: GBPU Long 15min; LeftLocalSide: 4; RightLocalSide: 4; PivotPointBreakDown: 10; EmaPeriod: 20;
            //optimizationResultBackward.ProfitDealsPrcnt: 30,5603; optimizationResultBackward.PML: 3,98069; optimizationResultBackward.Range: 9;
            //optimizationResultForward.ProfitDealsPrcnt: 30,1038; optimizationResultForward.PML: 2,63982; optimizationResultForward.Range: 29; optimizationResultTotal.Range: 38.

            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, this.securityFirst, PositionSide.Null);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 4, 10, 20);
            ts.ctx = ctx;

            
            //Security: GBPU Short 15min; LeftLocalSide: 13; RightLocalSide: 19; PivotPointBreakDown: 40; EmaPeriod: 40;
            //optimizationResultBackward.ProfitDealsPrcnt: 30,8943; optimizationResultBackward.PML: 2,04893; optimizationResultBackward.Range: 161;
            //optimizationResultForward.ProfitDealsPrcnt: 25,2874; optimizationResultForward.PML: -0,308342; optimizationResultForward.Range: 134; optimizationResultTotal.Range: 295.
            
            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(13, 19, 40, 40);
            ts.ctx = ctx;


            //Security: ED Long 15min; LeftLocalSide: 16; RightLocalSide: 7; PivotPointBreakDown: 10; EmaPeriod: 100;
            //optimizationResultBackward.ProfitDealsPrcnt: 28,4689; optimizationResultBackward.PML: 3,52304; optimizationResultBackward.Range: 66;
            //optimizationResultForward.ProfitDealsPrcnt: 27,9279; optimizationResultForward.PML: -0,179606; optimizationResultForward.Range: 166; optimizationResultTotal.Range: 232.

            securityNumber = 2;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(16, 7, 10, 100);
            ts.ctx = ctx;


            //Security: ED Short 15min; LeftLocalSide: 19; RightLocalSide: 4; PivotPointBreakDown: 10; EmaPeriod: 60;
            //optimizationResultBackward.ProfitDealsPrcnt: 27,4699; optimizationResultBackward.PML: 0,85454; optimizationResultBackward.Range: 172;
            //optimizationResultForward.ProfitDealsPrcnt: 28,7356; optimizationResultForward.PML: 0,334178; optimizationResultForward.Range: 57; optimizationResultTotal.Range: 229.

            securityNumber = 3;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(19, 4, 10, 60);
            ts.ctx = ctx;

            return securityList;
        }

        private List<Security> Initialize15minUSDScript10Shares(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 0.1);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new LocalMoneyManager(globalMoneyManager, account, Currency.USD, shares: 10);
            localMoneyManagerUSD.Logger = logger;

            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, this.securityFirst, PositionSide.Long);   //Security: Silver Long 15min; LeftLocalSide: 10; RightLocalSide: 7; PivotPointBreakDown: 60; EmaPeriod: 60; optimizationResultBackward.PML: 8,00695; optimizationResultBackward.Range: 104; optimizationResultForward.PML: 1,02302; optimizationResultForward.Range: 82; optimizationResultTotal.Range: 186.       
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(10, 7, 60, 60);
            ts.ctx = ctx;

            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short);   //Security: Silver Short 15min; LeftLocalSide: 1; RightLocalSide: 13; PivotPointBreakDown: 50; EmaPeriod: 120; optimizationResultBackward.PML: 5,79696; optimizationResultBackward.Range: 49; optimizationResultForward.PML: 1,79083; optimizationResultForward.Range: 186; optimizationResultTotal.Range: 235. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(1, 13, 50, 120);
            ts.ctx = ctx;

            securityNumber = 2;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long);   //Security: Brent Long 15min; LeftLocalSide: 10; RightLocalSide: 4; PivotPointBreakDown: 30; EmaPeriod: 20; optimizationResultBackward.PML: 11,0968; optimizationResultBackward.Range: 12; optimizationResultForward.PML: 14,2202; optimizationResultForward.Range: 24; optimizationResultTotal.Range: 36.
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(10, 4, 30, 20);
            ts.ctx = ctx;

            securityNumber = 3;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short);   //Security: Brent Short 15min; LeftLocalSide: 7; RightLocalSide: 19; PivotPointBreakDown: 80; EmaPeriod: 20; optimizationResultBackward.PML: 7,59971; optimizationResultBackward.Range: 47; optimizationResultForward.PML: 3,50507; optimizationResultForward.Range: 17; optimizationResultTotal.Range: 64.
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(7, 19, 80, 20);
            ts.ctx = ctx;

            return securityList;
        }

        private List<Security> Initialize15minRubleScript10Shares(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);
                        
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 0.1);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble, shares: 10);
            localMoneyManagerRuble.Logger = logger;

            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Long);   //Security: MXI Long 15min; LeftLocalSide: 7; RightLocalSide: 19; PivotPointBreakDown: 50; EmaPeriod: 120; optimizationResultBackward.PML: 6,62995; optimizationResultBackward.Range: 48; optimizationResultForward.PML: 13,3062; optimizationResultForward.Range: 27; optimizationResultTotal.Range: 75.       
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(7, 19, 50, 120);
            ts.ctx = ctx;

            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short);   //Security: MXI Short 15min; LeftLocalSide: 4; RightLocalSide: 13; PivotPointBreakDown: 20; EmaPeriod: 80; optimizationResultBackward.PML: 2,08451; optimizationResultBackward.Range: 96; optimizationResultForward.PML: 3,55623; optimizationResultForward.Range: 31; optimizationResultTotal.Range: 127.
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 13, 20, 80);
            ts.ctx = ctx;            

            return securityList;
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
                        
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 0.1);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new LocalMoneyManager(globalMoneyManager, account, Currency.USD);
            localMoneyManagerUSD.Logger = logger;

            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, this.securityFirst, PositionSide.Long);   //Security: GOLD Long 15min; LeftLocalSide: 7; RightLocalSide: 13; PivotPointBreakDown: 60; EmaPeriod: 120; optimizationResultBackward.PML: 11,1272; optimizationResultBackward.Range: 9; optimizationResultForward.PML: 4,18898; optimizationResultForward.Range: 79; optimizationResultTotal.Range: 88.  
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(7, 13, 60, 120);
            ts.ctx = ctx;

            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short);   //Security: GOLD Short 15min; LeftLocalSide: 4; RightLocalSide: 13; PivotPointBreakDown: 20; EmaPeriod: 140; optimizationResultBackward.PML: 1,68996; optimizationResultBackward.Range: 38; optimizationResultForward.PML: 4,23475; optimizationResultForward.Range: 107; optimizationResultTotal.Range: 145. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 13, 20, 140);
            ts.ctx = ctx;

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

            //var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 0.1);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
            localMoneyManagerRuble.Logger = logger;

            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;
            
            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Long);   //Security: Eu Long 15min; LeftLocalSide: 4; RightLocalSide: 4; PivotPointBreakDown: 40; EmaPeriod: 80; optimizationResultBackward.PML: 5,57296; optimizationResultBackward.Range: 124; optimizationResultForward.PML: 1,15587; optimizationResultForward.Range: 39; optimizationResultTotal.Range: 163.       
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 4, 40, 80);
            ts.ctx = ctx;

            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long);   //Security: GAZR Long 15min; LeftLocalSide: 4; RightLocalSide: 4; PivotPointBreakDown: 10; EmaPeriod: 180; optimizationResultBackward.PML: 3,98682; optimizationResultBackward.Range: 106; optimizationResultForward.PML: 19,0329; optimizationResultForward.Range: 27; optimizationResultTotal.Range: 133. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 4, 10, 180);
            ts.ctx = ctx;

            securityNumber = 2;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long);   //Security: Lkoh Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(7, 13, 10, 180);
            ts.ctx = ctx;

            securityNumber = 3;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long); //Security: Sbrf Long 15min; LeftLocalSide: 1; RightLocalSide: 16; PivotPointBreakDown: 90; EmaPeriod: 100; optimizationResultBackward.PML: 5,35436; optimizationResultBackward.Range: 51; optimizationResultForward.PML: 3,44417; optimizationResultForward.Range: 174; optimizationResultTotal.Range: 225. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(1, 16, 90, 100);
            ts.ctx = ctx;

            securityNumber = 4;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long); //Security: Si Long 15min; LeftLocalSide: 1; RightLocalSide: 16; PivotPointBreakDown: 100; EmaPeriod: 160; optimizationResultBackward.PML: 9,27828; optimizationResultBackward.Range: 88; optimizationResultForward.PML: 1,12731; optimizationResultForward.Range: 77; optimizationResultTotal.Range: 165. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(1, 16, 100, 160);
            ts.ctx = ctx;

            securityNumber = 5;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long); //Security: VTBR Long 15min; LeftLocalSide: 7; RightLocalSide: 16; PivotPointBreakDown: 10; EmaPeriod: 160; optimizationResultBackward.PML: 0,734876; optimizationResultBackward.Range: 129; optimizationResultForward.PML: 3,26112; optimizationResultForward.Range: 249; optimizationResultTotal.Range: 378. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(7, 16, 10, 160);
            ts.ctx = ctx;

            securityNumber = 6;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short);   //Security: Eu Short 15min; LeftLocalSide: 16; RightLocalSide: 1; PivotPointBreakDown: 50; EmaPeriod: 20; optimizationResultBackward.PML: 7,12197; optimizationResultBackward.Range: 8; optimizationResultForward.PML: 6,46363; optimizationResultForward.Range: 1; optimizationResultTotal.Range: 9. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(16, 1, 50, 20);
            ts.ctx = ctx;

            securityNumber = 7;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short);   //Security: GAZR Short 15min; LeftLocalSide: 19; RightLocalSide: 4; PivotPointBreakDown: 40; EmaPeriod: 100; optimizationResultBackward.PML: 2,63234; optimizationResultBackward.Range: 331; optimizationResultForward.PML: 0,755192; optimizationResultForward.Range: 36; optimizationResultTotal.Range: 367. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(19, 4, 40, 100);
            ts.ctx = ctx;

            securityNumber = 8;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null);   //!Security: LKOH Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(1, 16, 10, 180);
            ts.ctx = ctx;

            securityNumber = 9;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short); //?Security: SBRF Short 15min; LeftLocalSide: 13; RightLocalSide: 1; PivotPointBreakDown: 10; EmaPeriod: 20; optimizationResultBackward.PML: 0,505005; optimizationResultBackward.Range: 5; optimizationResultForward.PML: 0,0753111; optimizationResultForward.Range: 119; optimizationResultTotal.Range: 124. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(13, 1, 10, 20);
            ts.ctx = ctx;

            securityNumber = 10;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short); //Security: Si Short 15min; LeftLocalSide: 13; RightLocalSide: 1; PivotPointBreakDown: 20; EmaPeriod: 20; optimizationResultBackward.PML: 10,2312; optimizationResultBackward.Range: 6; optimizationResultForward.PML: 2,18651; optimizationResultForward.Range: 38; optimizationResultTotal.Range: 44. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(13, 1, 20, 20);
            ts.ctx = ctx;

            securityNumber = 11;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short); //Security: VTBR Short 15min; LeftLocalSide: 19; RightLocalSide: 7; PivotPointBreakDown: 10; EmaPeriod: 160; optimizationResultBackward.PML: 6,14407; optimizationResultBackward.Range: 99; optimizationResultForward.PML: 1,43859; optimizationResultForward.Range: 79; optimizationResultTotal.Range: 178. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(19, 7, 10, 160);
            ts.ctx = ctx;

            securityNumber = 12;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: TATN Long 15min; LeftLocalSide: 16; RightLocalSide: 16; PivotPointBreakDown: 10; EmaPeriod: 180; optimizationResultBackward.PML: 2,05025; optimizationResultBackward.Range: 63; optimizationResultForward.PML: 5,46902; optimizationResultForward.Range: 2; optimizationResultTotal.Range: 65. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(16, 16, 10, 180);
            ts.ctx = ctx;

            securityNumber = 13;                                                                                                                      //Запрет Трейдера на открытие позиций по клиентскому счету
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: TATN Short 15min; LeftLocalSide: 19; RightLocalSide: 16; PivotPointBreakDown: 10; EmaPeriod: 140; optimizationResultBackward.PML: 2,70446; optimizationResultBackward.Range: 14; optimizationResultForward.PML: 5,7412; optimizationResultForward.Range: 103; optimizationResultTotal.Range: 117. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(19, 16, 10, 140);
            ts.ctx = ctx;

            securityNumber = 14;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long); //Security: SBPR Long 15min; LeftLocalSide: 10; RightLocalSide: 10; PivotPointBreakDown: 60; EmaPeriod: 100; optimizationResultBackward.PML: 4,00686; optimizationResultBackward.Range: 134; optimizationResultForward.PML: 2,78388; optimizationResultForward.Range: 227; optimizationResultTotal.Range: 361. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(10, 10, 60, 100);
            ts.ctx = ctx;

            securityNumber = 15;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //!Security: SBPR Short 15min; LeftLocalSide: 4; RightLocalSide: 13; PivotPointBreakDown: 10; EmaPeriod: 200; optimizationResultBackward.PML: 0,303056; optimizationResultBackward.Range: 181; optimizationResultForward.PML: -0,381194; optimizationResultForward.Range: 97; optimizationResultTotal.Range: 278. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 13, 10, 200);
            ts.ctx = ctx;

            securityNumber = 16;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(13, 16, 50, 80);
            ts.ctx = ctx;

            securityNumber = 17;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(1, 16, 10, 180);
            ts.ctx = ctx;

            securityNumber = 18;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Long); //Security: MGNT Long 15min; LeftLocalSide: 19; RightLocalSide: 10; PivotPointBreakDown: 10; EmaPeriod: 200; optimizationResultBackward.PML: 1,21523; optimizationResultBackward.Range: 15; optimizationResultForward.PML: 4,21244; optimizationResultForward.Range: 1; optimizationResultTotal.Range: 16. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(19, 10, 10, 200);
            ts.ctx = ctx;

            securityNumber = 19;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Short); //Security: MGNT Short 15min; LeftLocalSide: 7; RightLocalSide: 19; PivotPointBreakDown: 50; EmaPeriod: 60; optimizationResultBackward.PML: 5,56438; optimizationResultBackward.Range: 85; optimizationResultForward.PML: 0,0181262; optimizationResultForward.Range: 46; optimizationResultTotal.Range: 131. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(7, 19, 50, 60);
            ts.ctx = ctx;

            securityNumber = 20;                                                                                                                     //неликвид
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: SNGR Long 15min; LeftLocalSide: 4; RightLocalSide: 10; PivotPointBreakDown: 90; EmaPeriod: 140; optimizationResultBackward.PML: 3,79498; optimizationResultBackward.Range: 3; optimizationResultForward.PML: 3,29889; optimizationResultForward.Range: 30; optimizationResultTotal.Range: 33. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 10, 90, 140);
            ts.ctx = ctx;

            securityNumber = 21;                                                                                                                      //неликвид
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: SNGR Short 15min; LeftLocalSide: 16; RightLocalSide: 7; PivotPointBreakDown: 100; EmaPeriod: 120; optimizationResultBackward.PML: 2,56544; optimizationResultBackward.Range: 86; optimizationResultForward.PML: 2,44855; optimizationResultForward.Range: 7; optimizationResultTotal.Range: 93. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(16, 7, 100, 120);
            ts.ctx = ctx;

            securityNumber = 22;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //!Security: MTSI Long 15min; LeftLocalSide: 16; RightLocalSide: 13; PivotPointBreakDown: 10; EmaPeriod: 180; optimizationResultBackward.PML: 1,81838; optimizationResultBackward.Range: 101; optimizationResultForward.PML: -0,231663; optimizationResultForward.Range: 201; optimizationResultTotal.Range: 302. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(16, 13, 10, 180);
            ts.ctx = ctx;

            securityNumber = 23;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //!Security: MTSI Short 15min; LeftLocalSide: 16; RightLocalSide: 13; PivotPointBreakDown: 10; EmaPeriod: 200; optimizationResultBackward.PML: 0,583217; optimizationResultBackward.Range: 81; optimizationResultForward.PML: -0,294981; optimizationResultForward.Range: 60; optimizationResultTotal.Range: 141. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(16, 13, 10, 200);
            ts.ctx = ctx;

            securityNumber = 24;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: NOTK Long 15min; LeftLocalSide: 13; RightLocalSide: 1; PivotPointBreakDown: 30; EmaPeriod: 40; optimizationResultBackward.PML: 4,5607; optimizationResultBackward.Range: 19; optimizationResultForward.PML: 18,2134; optimizationResultForward.Range: 28; optimizationResultTotal.Range: 47. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(13, 1, 30, 40);
            ts.ctx = ctx;

            securityNumber = 25;                                                                                                                     //Запрет Трейдера на открытие позиций по клиентскому счету
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: NOTK Short 15min; LeftLocalSide: 4; RightLocalSide: 10; PivotPointBreakDown: 80; EmaPeriod: 140; optimizationResultBackward.PML: 1,88414; optimizationResultBackward.Range: 4; optimizationResultForward.PML: 3,64946; optimizationResultForward.Range: 223; optimizationResultTotal.Range: 227. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 10, 80, 140);
            ts.ctx = ctx;

            securityNumber = 26;                                                                                                                     //неликвид 
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //!Security: HYDR Long 15min; LeftLocalSide: 4; RightLocalSide: 16; PivotPointBreakDown: 10; EmaPeriod: 100; optimizationResultBackward.PML: 0,87255; optimizationResultBackward.Range: 117; optimizationResultForward.PML: -0,097863; optimizationResultForward.Range: 2; optimizationResultTotal.Range: 119. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(4, 16, 10, 100);
            ts.ctx = ctx;

            securityNumber = 27;                                                                                                                     //неликвид
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: HYDR Short 15min; LeftLocalSide: 19; RightLocalSide: 4; PivotPointBreakDown: 30; EmaPeriod: 80; optimizationResultBackward.PML: 0,664194; optimizationResultBackward.Range: 135; optimizationResultForward.PML: 1,65009; optimizationResultForward.Range: 9; optimizationResultTotal.Range: 144. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(19, 4, 30, 80);
            ts.ctx = ctx;

            securityNumber = 28;                                                                                                                     //Запрет Трейдера на открытие позиций по клиентскому счету
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: FEES Long 15min; LeftLocalSide: 16; RightLocalSide: 19; PivotPointBreakDown: 90; EmaPeriod: 40; optimizationResultBackward.PML: 1,64319; optimizationResultBackward.Range: 90; optimizationResultForward.PML: 0,499431; optimizationResultForward.Range: 186; optimizationResultTotal.Range: 276. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(16, 19, 90, 40);
            ts.ctx = ctx;

            securityNumber = 29;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[securityNumber]), PositionSide.Null); //Security: FEES Short 15min; LeftLocalSide: 13; RightLocalSide: 7; PivotPointBreakDown: 10; EmaPeriod: 140; optimizationResultBackward.PML: 1,87474; optimizationResultBackward.Range: 13; optimizationResultForward.PML: 3,09513; optimizationResultForward.Range: 140; optimizationResultTotal.Range: 153. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.SetParameters(13, 7, 10, 140);
            ts.ctx = ctx;

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

            //===
            //foreach(var tradingSystem in tradingSystems)
            //{
            //    if (tradingSystem.PositionSide == PositionSide.Long || tradingSystem.PositionSide == PositionSide.Short)
            //        tradingSystem.Paint(context);
            //}
            //==

            //foreach (var tradingSystem in tradingSystems)            
            //    if (tradingSystem.HasOpenPosition())
            //        tradingSystem.Paint(context);

            if (tradingSystems[securityNumber] != null)
                tradingSystems[securityNumber].Paint(context);
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