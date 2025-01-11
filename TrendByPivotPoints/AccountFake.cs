﻿using System;
using TSLab.Script;

namespace TradingSystems
{
    public class AccountFake : AccountLab
    {
        public double[] EquityHistory { get; set; }     
        public new double FreeBalance { get { return base.FreeBalance; } set { freeBalance = value; } }
        public override double InitDeposit => base.InitDeposit;
        public override double Equity => base.Equity;
        public AccountFake(double initDeposit, Currency baseCurrency, Logger logger) : base(initDeposit, baseCurrency, logger)
        {
        }       

        public new double GObying { get; set; }
        public new double GOselling { get; set; }
        public new double Rate { get; set; }

        public ISecurity Security { get; set; }
        public Logger Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }        
    }
}