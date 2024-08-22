using System;
using System.Collections.Generic;
using TrendByPivotPoints;
using TSLab.DataSource;
using TSLab.Script;

namespace TradingSystems
{
    public class AccountFake : AccountLab
    {
     
        public new double FreeBalance { get { return base.FreeBalance; } set { freeBalance = value; } }
        public override double InitDeposit => base.InitDeposit;
        public override double Equity => base.Equity;
        public AccountFake(double initDeposit, Currency baseCurrency, Logger logger) : base(initDeposit, baseCurrency, logger)
        {
        }

        

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