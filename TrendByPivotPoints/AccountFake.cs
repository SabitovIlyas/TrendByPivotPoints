using System;
using System.Collections.Generic;
using TrendByPivotPoints;
using TSLab.DataSource;
using TSLab.Script;

namespace TradingSystems
{
    public class AccountFake : Account
    {
        public double InitDeposit { get; set; }
        public double Equity { get; set; }
        public double GObying { get; set; }
        public double GOselling { get; set; }
        public double Rate { get; set; }

        public ISecurity Security { get; set; }
        public ILogger Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double FreeBalance { get; set; }

        public void Initialize(List<Security> securities)
        {
        }

        public void Update(int barNumber)
        {            
        }
    }
}