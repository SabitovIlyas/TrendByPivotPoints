using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPointsStrategy
{
    class GlobalMoneyManagerFake : GlobalMoneyManager
    {
        public double RiskValuePrcnt { get { return riskValuePrcnt; } }
        public double Money { get; set; }
        public double FreeBalance { get { return freeMoney; } }

        private double riskValuePrcnt;
        private double riskValue;
        private double deposit;
        private double freeMoney;

        public GlobalMoneyManagerFake(Account account, double deposit, double freeMoney)
        {
            //this.riskValuePrcnt = riskValuePrcnt;
            riskValue = riskValuePrcnt / 100.0;
            this.deposit = deposit;
            this.freeMoney = freeMoney;
        }

        public double GetMoneyForDeal()
        {            
            var partofDeposit = riskValue * deposit;
            return partofDeposit;            
        }
    }
}
