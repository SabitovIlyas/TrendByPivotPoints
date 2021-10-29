using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TrendByPivotPointsStrategy
{
    public class AccountReal : Account
    {
        public ISecurity Security { get => sec;}
        ISecurity sec;

        public double InitDeposit => sec.InitDeposit;

        public double Equity 
        {
            get
            {
                var rtSec = sec as ISecurityRt;
                if (rtSec != null)
                    return rtSec.CurrencyBalance;
                return 0;
            } 
        }

        public double GObying => sec.FinInfo.BuyDeposit ?? double.MaxValue;

        public double GOselling => sec.FinInfo.SellDeposit ?? double.MaxValue;

        public double Rate { get { return rate; } set { rate = value; } }
        private double rate;

        Logger logger = new NullLogger();

        public Logger Logger
        {
            get
            {
                return logger;
            }

            set
            {
                logger = value;
            }
        }

        public double FreeBalance => throw new NotImplementedException();

        public AccountReal(ISecurity sec)
        {
            this.sec = sec;
        }

        public void Update(int barNumber)
        {         
        }
        public void Initialize(List<Security> securities)
        {            
        }
    }
}
