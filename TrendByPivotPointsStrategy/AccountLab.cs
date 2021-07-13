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

        public double Depo
        {
            get
            {
                //var l = sec.Positions.GetLastPosition(5);
                //l.Profit();
                return 0;
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
            equity = sec.InitDeposit;
        }

        public void Update(int barNumber)
        {
            var positions = sec.Positions;
            var lastPosition = positions.GetLastPosition(barNumber);
            lastPosition.IsActiveForBar(barNumber);

            //equity=equity+
            //var l = sec.Positions.GetLastPosition(5);
            //l.Profit();
        }

        private double equity;
    }
}
