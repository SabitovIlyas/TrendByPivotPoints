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
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            this.securityFirst = new SecurityTSlab(securityFirst);
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 1.00);
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
                        
            //tradingSystems = new List<TradingSystemPivotPointsTwoTimeFrames>();
            tradingSystems = new List<TradingSystemPivotPointsEMA>();

            double comission;
            AbsolutCommission comis;

            //tradingSystems.Add(new TradingSystemPivotPointsTwoTimeFrames(localMoneyManagerRuble, account, this.securityFirst));
            tradingSystems.Add(new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, this.securityFirst));
            comission = 1.15 * 2;
            //comission = 1 * 2;
            comis = new AbsolutCommission() { Commission = comission };
            comis.Execute(securities[0]);

            //tradingSystems.Add(new TradingSystemPivotPointsTwoTimeFrames(localMoneyManagerRuble, account, new SecurityTSlab(securities[1])));
            tradingSystems.Add(new TradingSystemPivotPointsEMA(localMoneyManagerRuble, account, new SecurityTSlab(securities[1])));
            comission = 2.02 * 2;
            //comission = 2 * 2;
            comis = new AbsolutCommission() { Commission = comission };
            comis.Execute(securities[1]);






            var logger = new LoggerSystem(ctx);
            //tradingSystem.Logger = logger;
            account.Logger = logger;
            this.ctx = ctx;
            context = new ContextTSLab(ctx);
        }

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
                    account.Update(i);
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