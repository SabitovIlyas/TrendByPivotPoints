using System;

namespace TrendByPivotPoints
{
    public class AccountFake : Account
    {
        public double GetDeposit()
        {
            throw new NotImplementedException();
        }

        public double GetFreeBalance()
        {
            throw new NotImplementedException();
        }
    }
}
