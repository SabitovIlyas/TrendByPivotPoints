using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPoints
{
    class Initializator
    {
        public void Initialize()
        {
            var globalMoneyManager = new GlobalMoneyManager();
            var localMoneyManager = new LocalMoneyManager();
            var tradingSystem = new TradingSystem();
        }

        public void Trad()
        {
            var globalMoneyManager = new GlobalMoneyManager();
            var localMoneyManager = new LocalMoneyManager();
            var tradingSystem = new TradingSystem();
        }
    }
}
