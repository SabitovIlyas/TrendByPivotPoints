using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPointsStrategy
{
    public class MainSystem
    {
        TradingSystem tradingSystem;
        public void Initialize()
        {
            var account = new AccountReal();
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 1.00);
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, Currency.Ruble);
            tradingSystem = new TradingSystem();
        }
        
        public void Run()
        {
            for (var i = 0; i < 100; i++)
            {

            }
        }

        public void Paint()
        {

        }
    }
}
