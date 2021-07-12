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
        TradingSystem tradingSystem;
        Security security;
        IContext ctx;
        ContextTSLab context;
        public void Initialize(ISecurity sec, IContext ctx)
        {
            Account account;

            if (IsLaboratory(sec))            
                account = new AccountLab(sec);            
            else            
                account = new AccountReal(sec);

            security = new SecurityTSlab(sec);
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 1.00);
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
            tradingSystem = new TradingSystem(localMoneyManagerRuble, account, security);
            tradingSystem.Logger = new LoggerSystem(ctx);
            this.ctx = ctx;
            var comis = new AbsolutCommission() { Commission = 2.3 };
            comis.Execute(sec);
            context = new ContextTSLab(ctx);            
        }

        public void Run()
        {            
            tradingSystem.CalculateIndicators();
            var lastBarNmber = security.GetBarsCount() - 1;

            for (var i = 0; i < lastBarNmber; i++)
                tradingSystem.Update(i);

            if (IsRealTimeTrading())
            {
                tradingSystem.CheckPositionCloseCase(lastBarNmber);
                if (IsLastBarClosed())
                    tradingSystem.Update(lastBarNmber);
            }
            else
                tradingSystem.Update(lastBarNmber);
        }

        public void Paint(IContext ctx, ISecurity sec)
        {
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
            return security.IsRealTimeTrading;
        }
    }
}
