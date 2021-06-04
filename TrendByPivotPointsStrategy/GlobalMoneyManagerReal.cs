using System;

namespace TrendByPivotPointsStrategy
{
    public class GlobalMoneyManagerReal : GlobalMoneyManager
    {
        public double RiskValuePrcnt { get { return riskValuePrcnt; } }
        private double riskValuePrcnt;
        private double riskValue;
        private Account account;
        private Logger logger = new NullLogger();

        public GlobalMoneyManagerReal(Account account, double riskValuePrcnt)
        {
            this.account = account;
            this.riskValuePrcnt = riskValuePrcnt;
            riskValue = riskValuePrcnt / 100.0;
        }

        public double GetMoney()
        {
            var deposit = account.Deposit;
            logger.Log("deposit = " + deposit.ToString());

            var freeBalance = account.FreeBalance;
            var partofDeposit = riskValue * deposit;
            return Math.Min(partofDeposit, freeBalance);
        }
    }
}
