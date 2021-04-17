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
            var account = new AccountReal();
            var globalMoneyManager = new GlobalMoneyManager(account, riskValuePrcnt: 5.00);
            var localMoneyManager = new LocalMoneyManager();
            var tradingSystem = new TradingSystem();
        }       
    }
}
