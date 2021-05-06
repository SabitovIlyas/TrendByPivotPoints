using System;
using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    public class AccountLab : Account
    {
        private ISecurity sec;
        public double Deposit
        {
            get
            {
                if (sec == null)
                    return 0;
                return sec.InitDeposit;
            }
        }

        public double FreeBalance
        {
            get
            {
                if (sec == null)
                    return 0;
                return sec.InitDeposit;
            }
        }

        public double GObying => 4500;

        public double GOselling => 4500;

        public double Rate { get => 1; set => throw new NotImplementedException(); }

        public ISecurity Security
        {
            get
            {
                return sec;
            }
            set
            {
                sec = value;
            }
        }

        public AccountLab(ISecurity sec)
        {
            this.sec = sec;
        }
    }
}
