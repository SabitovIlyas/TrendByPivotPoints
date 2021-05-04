using System;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class AccountReal : Account
    {
        public ISecurity Security { get => sec;}
        ISecurity sec;

        public double Deposit => throw new NotImplementedException();

        public double FreeBalance => throw new NotImplementedException();

        public double GObying => throw new NotImplementedException();

        public double GOselling => throw new NotImplementedException();

        public double Rate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }        

        public AccountReal(ISecurity sec)
        {
            this.sec = sec;
        }
    }

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
