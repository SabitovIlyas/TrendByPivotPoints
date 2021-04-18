using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPoints
{
    class GlobalMoneyManagerFake : GlobalMoneyManager
    {
        public double RiskValuePrcnt { get; set; }
        public double Money { get; set; }

        public double GetMoney()
        {
            return Money;
        }
    }
}
