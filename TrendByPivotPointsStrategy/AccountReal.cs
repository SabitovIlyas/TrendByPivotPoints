using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Realtime;

namespace TradingSystems
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
                    return rtSec.EstimatedBalance;
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

        public double FreeBalance
        {
            get
            {
                var rtSec = sec as ISecurityRt;
                if (rtSec != null)
                    return rtSec.CurrencyBalance;
                return 0;
            }
        }

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
