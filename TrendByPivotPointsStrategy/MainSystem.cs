using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class MainSystem
    {
        TradingSystem tradingSystem;
        List<Bar> bars;
        public void Initialize(ISecurity sec, List<Bar> bars)
        {
            var account = new AccountReal(sec);
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 1.00);
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
            tradingSystem = new TradingSystem(bars);
            this.bars = bars;
        }
        
        public void Run()
        {
            for (var i = 0; i < bars.Count; i++)
            {
                tradingSystem.Update(i);
            }
        }

        public void Paint()
        {

        }
    }
}
