using System;
using TrendByPivotPoints;

namespace TrendByPivotPoints
{
    public class AccountFake : Account
    {
        public double Deposit { get; set; }
        public double FreeBalance { get; set; }

        public double GObying { get; set; }

        public double GOselling { get; set; }
        public double Rate { get; set; }
    }
}