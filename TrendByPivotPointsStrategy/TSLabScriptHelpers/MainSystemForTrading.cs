using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public class MainSystemForTrading : PivotPointsStarter
    {        
        Security securityFirst;
        IContext ctx;
        static DateTime lastClosedBarDateTime = DateTime.MinValue;

        public override void Initialize(ISecurity[] securities, IContext ctx)
        {
            logger = new TsLabLogger(ctx);
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
                        securityList = Initialize15minRubleScript(securities);  //1
                        break;
                    }
                case 2:
                    {
                        securityList = Initialize5minUSDScript(securities); //не торгую
                        break;
                    }
                case 3:
                    {
                        securityList = Initialize15minUSDScript(securities);    //3
                        break;
                    }
                case 4:
                    {
                        securityList = Initialize15minRubleScript10Shares(securities);  //4
                        break;
                    }
                case 5:
                    {
                        securityList = Initialize15minUSDScript10Shares(securities);    //5
                        break;
                    }
                case 6:
                    {
                        securityList = Initialize15minUSDScript1000Shares(securities);  //не торгую
                        break;
                    }
                case 7:
                    {
                        securityList = InitializeTest(securities);  //тест
                        break;
                    }
            }

            //tradingSystem.Logger = logger;
            account.Logger = logger;
            account.Rate = rateUSD;            
            context = ContextTSLab.Create(ctx);
            account.Initialize(securityList);
        }

        private List<Security> InitializeTest(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: 0.01);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new ContractsManager(globalMoneyManager, account, Currency.USD);
            localMoneyManagerUSD.Logger = logger;

            tradingSystems = new List<TradingSystem>();
                        
            TradingSystem ts;
                        
            ts = new TradingSystemPivotPointsEmaRtUpdate(localMoneyManagerUSD, account, this.securityFirst, PositionSide.Long);   //Security: GOLD Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            ts.Initialize(ctx);
            ts.SetParameters(7, 10, 140);            
            
            return securityList;
        }

        private List<Security> Initialize15minUSDScript1000Shares(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: 0.01);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new ContractsManager(globalMoneyManager, account, Currency.USD, shares: 1000);
            localMoneyManagerUSD.Logger = logger;

            tradingSystems = new List<TradingSystem>();

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
            ts.Initialize(ctx);
            ts.SetParameters(4, 4, 10, 20);            
                        
            //Security: GBPU Short 15min; LeftLocalSide: 13; RightLocalSide: 19; PivotPointBreakDown: 40; EmaPeriod: 40;
            //optimizationResultBackward.ProfitDealsPrcnt: 30,8943; optimizationResultBackward.PML: 2,04893; optimizationResultBackward.Range: 161;
            //optimizationResultForward.ProfitDealsPrcnt: 25,2874; optimizationResultForward.PML: -0,308342; optimizationResultForward.Range: 134; optimizationResultTotal.Range: 295.
            
            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 19, 40, 40);            

            //Security: ED Long 15min; LeftLocalSide: 16; RightLocalSide: 7; PivotPointBreakDown: 10; EmaPeriod: 100;
            //optimizationResultBackward.ProfitDealsPrcnt: 28,4689; optimizationResultBackward.PML: 3,52304; optimizationResultBackward.Range: 66;
            //optimizationResultForward.ProfitDealsPrcnt: 27,9279; optimizationResultForward.PML: -0,179606; optimizationResultForward.Range: 166; optimizationResultTotal.Range: 232.

            securityNumber = 2;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 7, 10, 100);            

            //Security: ED Short 15min; LeftLocalSide: 19; RightLocalSide: 4; PivotPointBreakDown: 10; EmaPeriod: 60;
            //optimizationResultBackward.ProfitDealsPrcnt: 27,4699; optimizationResultBackward.PML: 0,85454; optimizationResultBackward.Range: 172;
            //optimizationResultForward.ProfitDealsPrcnt: 28,7356; optimizationResultForward.PML: 0,334178; optimizationResultForward.Range: 57; optimizationResultTotal.Range: 229.

            securityNumber = 3;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);            
            ts.Initialize(ctx);
            ts.SetParameters(19, 4, 10, 60);

            return securityList;
        }

        private List<Security> Initialize15minUSDScript10Shares(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: 0.01);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new ContractsManager(globalMoneyManager, account, Currency.USD, shares: 10);
            localMoneyManagerUSD.Logger = logger;

            tradingSystems = new List<TradingSystem>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, this.securityFirst, PositionSide.Long);   //Security: Silver Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 16, 20, 100);            

            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);   //!Security: Silver Short 15min; LeftLocalSide: 1; RightLocalSide: 13; PivotPointBreakDown: 50; EmaPeriod: 120; optimizationResultBackward.PML: 5,79696; optimizationResultBackward.Range: 49; optimizationResultForward.PML: 1,79083; optimizationResultForward.Range: 186; optimizationResultTotal.Range: 235. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(1, 13, 50, 120);            

            securityNumber = 2;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Long);   //Security: Brent Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(1, 10, 50, 80);            

            securityNumber = 3;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);   //!Security: Brent Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(7, 4, 50, 160);            

            return securityList;
        }

        private List<Security> Initialize15minRubleScript10Shares(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);
                        
            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: 0.01);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new ContractsManager(globalMoneyManager, account, Currency.Ruble, shares: 10);
            localMoneyManagerRuble.Logger = logger;

            tradingSystems = new List<TradingSystem>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Long);   //Security: MXI Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 10, 60, 180);            

            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);   //Security: MXI Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(19, 1, 40, 140);            

            return securityList;
        }

        private List<Security> Initialize15minUSDScript(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);
                        
            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: 0.01);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new ContractsManager(globalMoneyManager, account, Currency.USD);
            localMoneyManagerUSD.Logger = logger;

            tradingSystems = new List<TradingSystem>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, this.securityFirst, PositionSide.Long);   //Security: GOLD Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(7, 7, 10, 140);            

            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Short);   //Security: GOLD Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 1, 10, 40);            

            return securityList;
        }

        private List<Security> Initialize5minUSDScript(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerUSD = new ContractsManager(globalMoneyManager, account, Currency.USD); //заменить на USD
            localMoneyManagerUSD.Logger = logger;

            var localMoneyManagerUSD10Shares = new ContractsManager(globalMoneyManager, account, Currency.USD, 10); //заменить на USD            
            localMoneyManagerUSD10Shares.Logger = logger;

            tradingSystems = new List<TradingSystem>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, this.securityFirst, PositionSide.Null);   //brent-5min long      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0033 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            ts.Initialize(ctx);
            ts.SetParameters(4, 13, 70, 140);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[1]), PositionSide.Null);   //gold-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.04 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[1]);
            ts.Initialize(ctx);
            ts.SetParameters(7, 13, 70, 160);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSLab(securities[2]), PositionSide.Long);   //silver-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0011 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[2]);
            ts.Initialize(ctx);
            ts.SetParameters(10, 4, 90, 160);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSLab(securities[3]), PositionSide.Null);   //brent-5min short      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0033 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[3]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 10, 70, 100);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD, account, new SecurityTSLab(securities[4]), PositionSide.Null);   //gold-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.04 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[4]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 16, 10, 20);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerUSD10Shares, account, new SecurityTSLab(securities[5]), PositionSide.Null);   //silver-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.0011 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[5]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 16, 90, 180);

            return securityList;
        }

        private List<Security> Initialize15minRubleScript(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: 0.01);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new ContractsManager(globalMoneyManager, account, Currency.Ruble);
            localMoneyManagerRuble.Logger = logger;

            tradingSystems = new List<TradingSystem>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;
            
            var securityNumber = 0;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Long);   //Security: Eu Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(4, 1, 20, 200);            

            securityNumber = 1;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Long);   //Security: GAZR Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(19, 1, 60, 140);            

            securityNumber = 2;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Long);   //Security: LKOH Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(7, 13, 10, 180);            

            securityNumber = 3;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Long);    //Security: SBRF Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(1, 16, 80, 100);            

            securityNumber = 4;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: Si Long 15min; LeftLocalSide: 1; RightLocalSide: 16; PivotPointBreakDown: 100; EmaPeriod: 160; optimizationResultBackward.PML: 9,27828; optimizationResultBackward.Range: 88; optimizationResultForward.PML: 1,12731; optimizationResultForward.Range: 77; optimizationResultTotal.Range: 165. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(1, 16, 100, 160);            

            securityNumber = 5;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: VTBR Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(4, 16, 10, 180);            

            securityNumber = 6;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Short);   //Security: Eu Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(1, 13, 100, 20);            

            securityNumber = 7;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);   //!Security: GAZR Short 15min; LeftLocalSide: 19; RightLocalSide: 4; PivotPointBreakDown: 40; EmaPeriod: 100; optimizationResultBackward.PML: 2,63234; optimizationResultBackward.Range: 331; optimizationResultForward.PML: 0,755192; optimizationResultForward.Range: 36; optimizationResultTotal.Range: 367. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(19, 4, 40, 100);            

            securityNumber = 8;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);   //!Security: LKOH Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(1, 16, 10, 180);            

            securityNumber = 9;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null);    //Security: SBRF Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 1, 20, 120);            

            securityNumber = 10;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Short); //Security: Si Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(10, 1, 50, 20);            

            securityNumber = 11;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Short); //Security: VTBR Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(19, 7, 10, 140);            

            securityNumber = 12;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: TATN Long 15min; LeftLocalSide: 16; RightLocalSide: 16; PivotPointBreakDown: 10; EmaPeriod: 180; optimizationResultBackward.PML: 2,05025; optimizationResultBackward.Range: 63; optimizationResultForward.PML: 5,46902; optimizationResultForward.Range: 2; optimizationResultTotal.Range: 65. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 16, 10, 180);            

            securityNumber = 13;                                                                                                                      //Запрет Трейдера на открытие позиций по клиентскому счету
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: TATN Short 15min; LeftLocalSide: 19; RightLocalSide: 16; PivotPointBreakDown: 10; EmaPeriod: 140; optimizationResultBackward.PML: 2,70446; optimizationResultBackward.Range: 14; optimizationResultForward.PML: 5,7412; optimizationResultForward.Range: 103; optimizationResultTotal.Range: 117. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(19, 16, 10, 140);            

            securityNumber = 14;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Long); //Security: SBPR Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(1, 16, 80, 100);            

            securityNumber = 15;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: SBPR Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 1, 20, 120);            

            securityNumber = 16;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Long); //Security: ROSN Long 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 16, 50, 80);            

            securityNumber = 17;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: ROSN Short 15min
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(1, 16, 10, 180);            

            securityNumber = 18;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: MGNT Long 15min; LeftLocalSide: 19; RightLocalSide: 10; PivotPointBreakDown: 10; EmaPeriod: 200; optimizationResultBackward.PML: 1,21523; optimizationResultBackward.Range: 15; optimizationResultForward.PML: 4,21244; optimizationResultForward.Range: 1; optimizationResultTotal.Range: 16. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(19, 10, 10, 200);            

            securityNumber = 19;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: MGNT Short 15min; LeftLocalSide: 7; RightLocalSide: 19; PivotPointBreakDown: 50; EmaPeriod: 60; optimizationResultBackward.PML: 5,56438; optimizationResultBackward.Range: 85; optimizationResultForward.PML: 0,0181262; optimizationResultForward.Range: 46; optimizationResultTotal.Range: 131. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(7, 19, 50, 60);            

            securityNumber = 20;                                                                                                                     //неликвид
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: SNGR Long 15min; LeftLocalSide: 4; RightLocalSide: 10; PivotPointBreakDown: 90; EmaPeriod: 140; optimizationResultBackward.PML: 3,79498; optimizationResultBackward.Range: 3; optimizationResultForward.PML: 3,29889; optimizationResultForward.Range: 30; optimizationResultTotal.Range: 33. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(4, 10, 90, 140);            

            securityNumber = 21;                                                                                                                      //неликвид
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: SNGR Short 15min; LeftLocalSide: 16; RightLocalSide: 7; PivotPointBreakDown: 100; EmaPeriod: 120; optimizationResultBackward.PML: 2,56544; optimizationResultBackward.Range: 86; optimizationResultForward.PML: 2,44855; optimizationResultForward.Range: 7; optimizationResultTotal.Range: 93. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 7, 100, 120);            

            securityNumber = 22;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: MTSI Long 15min; LeftLocalSide: 16; RightLocalSide: 13; PivotPointBreakDown: 10; EmaPeriod: 180; optimizationResultBackward.PML: 1,81838; optimizationResultBackward.Range: 101; optimizationResultForward.PML: -0,231663; optimizationResultForward.Range: 201; optimizationResultTotal.Range: 302. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 13, 10, 180);            

            securityNumber = 23;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: MTSI Short 15min; LeftLocalSide: 16; RightLocalSide: 13; PivotPointBreakDown: 10; EmaPeriod: 200; optimizationResultBackward.PML: 0,583217; optimizationResultBackward.Range: 81; optimizationResultForward.PML: -0,294981; optimizationResultForward.Range: 60; optimizationResultTotal.Range: 141. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 13, 10, 200);            

            securityNumber = 24;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: NOTK Long 15min; LeftLocalSide: 13; RightLocalSide: 1; PivotPointBreakDown: 30; EmaPeriod: 40; optimizationResultBackward.PML: 4,5607; optimizationResultBackward.Range: 19; optimizationResultForward.PML: 18,2134; optimizationResultForward.Range: 28; optimizationResultTotal.Range: 47. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 1, 30, 40);            

            securityNumber = 25;                                                                                                                     //Запрет Трейдера на открытие позиций по клиентскому счету
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: NOTK Short 15min; LeftLocalSide: 4; RightLocalSide: 10; PivotPointBreakDown: 80; EmaPeriod: 140; optimizationResultBackward.PML: 1,88414; optimizationResultBackward.Range: 4; optimizationResultForward.PML: 3,64946; optimizationResultForward.Range: 223; optimizationResultTotal.Range: 227. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(4, 10, 80, 140);            

            securityNumber = 26;                                                                                                                     //неликвид 
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //!Security: HYDR Long 15min; LeftLocalSide: 4; RightLocalSide: 16; PivotPointBreakDown: 10; EmaPeriod: 100; optimizationResultBackward.PML: 0,87255; optimizationResultBackward.Range: 117; optimizationResultForward.PML: -0,097863; optimizationResultForward.Range: 2; optimizationResultTotal.Range: 119. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(4, 16, 10, 100);            

            securityNumber = 27;                                                                                                                     //неликвид
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: HYDR Short 15min; LeftLocalSide: 19; RightLocalSide: 4; PivotPointBreakDown: 30; EmaPeriod: 80; optimizationResultBackward.PML: 0,664194; optimizationResultBackward.Range: 135; optimizationResultForward.PML: 1,65009; optimizationResultForward.Range: 9; optimizationResultTotal.Range: 144. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(19, 4, 30, 80);            

            securityNumber = 28;                                                                                                                     //Запрет Трейдера на открытие позиций по клиентскому счету
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: FEES Long 15min; LeftLocalSide: 16; RightLocalSide: 19; PivotPointBreakDown: 90; EmaPeriod: 40; optimizationResultBackward.PML: 1,64319; optimizationResultBackward.Range: 90; optimizationResultForward.PML: 0,499431; optimizationResultForward.Range: 186; optimizationResultTotal.Range: 276. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 19, 90, 40);            

            securityNumber = 29;
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[securityNumber]), PositionSide.Null); //Security: FEES Short 15min; LeftLocalSide: 13; RightLocalSide: 7; PivotPointBreakDown: 10; EmaPeriod: 140; optimizationResultBackward.PML: 1,87474; optimizationResultBackward.Range: 13; optimizationResultForward.PML: 3,09513; optimizationResultForward.Range: 140; optimizationResultTotal.Range: 153. 
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[securityNumber]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 7, 10, 140);            

            return securityList;
        }

        private List<Security> Initialize5minRubleScript(ISecurity[] securities)
        {
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new RiskManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;
            var localMoneyManagerRuble = new ContractsManager(globalMoneyManager, account, Currency.Ruble);
                        
            tradingSystems = new List<TradingSystem>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, PositionSide.Null);   //eu-5min long      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            ts.Initialize(ctx);
            ts.SetParameters(10, 10, 10, 180);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[1]), PositionSide.Null);   //gz-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[1]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 16, 80, 200);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[2]), PositionSide.Null);   //lkoh-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[2]);
            ts.Initialize(ctx);
            ts.SetParameters(10, 16, 80, 160);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[3]), PositionSide.Null); //sbrf-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[3]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 13, 60, 20);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[4]), PositionSide.Long); //si-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[4]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 1, 10, 60);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[5]), PositionSide.Null); //vtbr-5min long
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[5]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 1, 80, 180);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[6]), PositionSide.Null);   //eu-5min short      
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.34 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[6]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 4, 100, 40);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[7]), PositionSide.Null);   //gz-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.9 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[7]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 4, 10, 100);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[8]), PositionSide.Null);   //lkoh-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 4.15 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[8]);
            ts.Initialize(ctx);
            ts.SetParameters(13, 16, 30, 120);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[9]), PositionSide.Null); //sbrf-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 2.03 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[9]);
            ts.Initialize(ctx);
            ts.SetParameters(4, 16, 10, 120);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[10]), PositionSide.Null); //si-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 1.13 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[10]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 4, 70, 40);

            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSLab(securities[11]), PositionSide.Null); //vtbr-5min short
            ts.Logger = logger;
            tradingSystems.Add(ts);
            totalComission = 0.33 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[11]);
            ts.Initialize(ctx);
            ts.SetParameters(16, 16, 50, 180);

            return securityList;
        }
        
        public override void Run()
        {
            //logger.SwitchOff();
            //var localLogger = new LoggerSystem(ctx);

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
    }
}