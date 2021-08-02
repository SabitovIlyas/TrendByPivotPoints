using System;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class AccountReal : Account
    {
        public ISecurity Security { get => sec;}
        ISecurity sec;

        public double InitDeposit => throw new NotImplementedException();

        public double Equity => throw new NotImplementedException();

        public double GObying => throw new NotImplementedException();

        public double GOselling => throw new NotImplementedException();

        public double Rate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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

        public AccountReal(ISecurity sec)
        {
            this.sec = sec;
        }

        public void Update(int barNumber)
        {         
        }
    }
}
