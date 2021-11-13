using System;
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
    public class MainSystemForOptimization : MainSystem
    {
        //TradingSystemPivotPointsTwoTimeFrames tradingSystem1;
        TradingSystemPivotPointsEMA tradingSystem1;
        Security securityFirst;
        IContext ctx;
        ContextTSLab context;
        Account account;
        //List<TradingSystemPivotPointsTwoTimeFrames> tradingSystems;
        List<TradingSystemPivotPointsEMA> tradingSystems;
        int securityNumber;
        public Logger Logger { get { return logger; } set { logger = value; } }
        Logger logger=new NullLogger();

        public void Initialize(ISecurity[] securities, IContext ctx)
        {
            //var logger = new LoggerSystem(ctx);
            var logger = Logger;
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
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble, shares);

            //tradingSystems = new List<TradingSystemPivotPointsTwoTimeFrames>();
            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystemPivotPointsEMA ts;

            //tradingSystems.Add(new TradingSystemPivotPointsTwoTimeFrames(localMoneyManagerRuble, account, this.securityFirst));            
            ts = new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst, (PositionSide)((int)positionSide));//si-5min            
            localMoneyManagerRuble.Logger = logger;
            ts.Logger = logger;
            tradingSystems.Add(ts);
            //comission = 0;
            totalComission = comission * 2;
            //comission = 1 * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            ts.SetParameters(leftLocalSide, rightLocalSide, pivotPointBreakDownSide, EmaPeriodSide);
            securities[0].Commission = CalculateCommission;
           
            account.Logger = logger;
            this.ctx = ctx;
            context = new ContextTSLab(ctx);
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

        bool leSeNullPreviousBar = false;
        public void Run()
        {
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

                    //var sec = securityFirst as SecurityTSlab;

                    //var le = sec.security.Positions.GetLastActiveForSignal("LE", i);
                    //var se = sec.security.Positions.GetLastActiveForSignal("SE", i);


                    //var leSeNullCurrentBar = (le == null) && (se == null);
                    //var moneyBefore = account.Equity;

                    account.Update(lastClosedBarNumberInRealTrading);

                    //var moneyAfter = account.Equity;
                    //var br = (leSeNullCurrentBar && leSeNullPreviousBar && (moneyAfter != moneyBefore));

                    //leSeNullPreviousBar = leSeNullCurrentBar;
                }
            }

            if (IsRealTimeTrading())
            {
                foreach (var tradingSystem in tradingSystems)
                    tradingSystem.CheckPositionCloseCase(lastBarNumber);

                if (IsLastBarClosed())
                    foreach (var tradingSystem in tradingSystems)
                        tradingSystem.Update(lastBarNumber);
            }
            else
                foreach (var tradingSystem in tradingSystems)
                    tradingSystem.Update(lastBarNumber);

            account.Update(lastBarNumber);
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

        public void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide, double rateUSD, double positionSide, 
            double comission, double riskValuePrcnt, int securityNumber, int instrumentsGroup, int shares)
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
            this.shares = shares;
        }

        private double leftLocalSide;
        private double rightLocalSide;
        private double pivotPointBreakDownSide;
        private double EmaPeriodSide;
        private double rateUSD;
        private double positionSide;
        private double comission;
        private double riskValuePrcnt;
        private int shares;
    }
}