using System;
using TrendByPivotPoints;
using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    public class AccountFake : Account
    {
        public double Deposit { get; set; }
        public double FreeBalance { get; set; }

        public double GObying { get; set; }

        public double GOselling { get; set; }
        public double Rate { get; set; }

        public ISecurity Security => throw new NotImplementedException();
    }
}