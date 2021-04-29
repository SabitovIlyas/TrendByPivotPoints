using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    public class TradingSystem    
    {
        List<Bar> bars;
        LocalMoneyManager localMoneyManager;
        Account account;
        ISecurity sec;
        public TradingSystem(List<Bar> bars, LocalMoneyManager localMoneyManager, Account account)
        {
            this.bars = bars;
            this.localMoneyManager = localMoneyManager;
            this.account = account;
            sec = account.Security;
        }

        public void Update(int barNumber)
        {
            var le = sec.Positions.GetLastActiveForSignal("LE");
            if (le==null)
            {

            }

        }
    }
}
