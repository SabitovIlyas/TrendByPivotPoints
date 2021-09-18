﻿using System;

namespace TrendByPivotPointsStrategy
{
    public class GlobalMoneyManagerReal : GlobalMoneyManager
    {
        public double RiskValuePrcnt { get { return riskValuePrcnt; } }
        private double riskValuePrcnt;
        private double riskValue;
        private Account account;
        private Logger logger = new NullLogger();
        public Logger Logger { get { return logger; } set { logger = value; } }

        public GlobalMoneyManagerReal(Account account, double riskValuePrcnt)
        {
            this.account = account;
            this.riskValuePrcnt = riskValuePrcnt;
            riskValue = riskValuePrcnt / 100.0;
        }

        //public double GetMoneyForDeal()
        //{
        //    var deposit = account.InitDeposit;            
        //    var partofDeposit = riskValue * deposit;
        //    var result = Math.Min(partofDeposit, FreeBalance);
        //    return result;

        //    //var currDepo = sec.InitDeposit + sec.Positions.TotalProfit(ctx.BarsCount - 1);

        //}

        public double GetMoneyForDeal()
        {                      
            logger.Log("FreeBalance = " + Equity.ToString());
            return riskValue * Equity;                       
        }

        public double Equity => account.Equity;

        public double FreeBalance => throw new NotImplementedException();
    }
}
