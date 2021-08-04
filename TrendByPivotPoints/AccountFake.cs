using System;
using System.Collections.Generic;
using TrendByPivotPoints;
using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    public class AccountFake : Account
    {
        public double InitDeposit { get; set; }
        public double Equity { get; set; }

        public double GObying { get; set; }

        public double GOselling { get; set; }
        public double Rate { get; set; }

        public ISecurity Security { get; set; }
        public Logger Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Initialize(List<Security> securities)
        {
        }

        public void Update(int barNumber)
        {            
        }
    }
}