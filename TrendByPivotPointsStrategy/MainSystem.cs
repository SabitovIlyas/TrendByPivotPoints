using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using TSLab.Script;
using TSLab.Script.Handlers;
using SystemColor = System.Drawing.Color;
using TsLabColor = TSLab.Script.Color;
using TSLab.DataSource;
using TSLab.Script.Realtime;

namespace TrendByPivotPointsStrategy
{
    public class MainSystem
    {
        TradingSystem tradingSystem;
        Security security;
        public void Initialize(ISecurity sec, IContext ctx)
        {
            Account account;
            if (IsLaboratory(sec))
            {
                account = new AccountLab(sec);
                //security = new SecurityLab(sec);
                security = new SecurityReal(sec);
            }
            else
            {
                account = new AccountReal(sec);
                security = new SecurityReal(sec);
            }
            
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 1.00);
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
            tradingSystem = new TradingSystem(localMoneyManagerRuble, account, security);
            tradingSystem.Logger = new LoggerSystem(ctx);            
        }
        
        public void Run()
        {
            for (var i = 0; i < security.GetSecurityCount(); i++)                            
                tradingSystem.Update(i);            
        }

        public void Paint(IContext ctx, ISecurity sec)
        {
            var pane = ctx.CreatePane("Инструмент (основной таймфрейм)", 50, false);
            var color = new TsLabColor(SystemColor.Green.ToArgb());
            pane.AddList(sec.ToString(), sec, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);            

            var compressedSec = sec.CompressTo(new Interval(30, DataIntervals.MINUTE));
            pane = ctx.CreatePane("Инструмент (средний таймфрейм)", 50, false);            
            pane.AddList(compressedSec.ToString(), compressedSec, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);            

            //compressedSec = sec.CompressTo(new Interval(120, DataIntervals.MINUTE));
            //pane = ctx.CreatePane("Инструмент (старший таймфрейм)", 50, false);
            //pane.AddList(compressedSec.ToString(), compressedSec, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);
        }

        private bool IsLaboratory(ISecurity security)
        {
            var realTimeSecurity = security as ISecurityRt;
            return realTimeSecurity == null;
        }
    }
}
