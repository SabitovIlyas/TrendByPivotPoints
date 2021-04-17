﻿using System;

namespace TrendByPivotPoints
{
    public class GlobalMoneyManager
    {
        public double RiskValuePrcnt { get { return riskValuePrcnt; } }
        private double riskValuePrcnt;
        private double riskValue;
        private Account account;

        public GlobalMoneyManager(Account account, double riskValuePrcnt)
        {
            this.account = account;
            this.riskValuePrcnt = riskValuePrcnt;
            riskValue = riskValuePrcnt / 100.0;
        }

        public double GetMoney()
        {
            var deposit = account.Deposit;
            var freeBalance = account.FreeBalance;
            var partofDeposit = riskValue * deposit;            
            return Math.Min(partofDeposit, freeBalance);
        }
    }
}
