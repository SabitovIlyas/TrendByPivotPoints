using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public class AccountLab : Account
    {
        //TODO: Реализовать этот класс. Пока он пустой.
        public override double InitDeposit => initDeposit;
        public override double Equity => equity;
        public override double FreeBalance => freeBalance;

        private double initDeposit;
        protected double freeBalance;

        public AccountLab(double initDeposit, Currency currency, Logger logger)
        {
            this.initDeposit = initDeposit;
            equity = initDeposit;
            freeBalance = initDeposit;
            this.currency = currency;            
            this.logger = logger;
        }
        public AccountLab(double initDeposit, Currency currency, List<Security> securities, Logger logger)
        {
            this.initDeposit = initDeposit;
            equity = initDeposit;
            freeBalance = initDeposit;
            this.currency = currency;
            this.securities = securities;
            this.logger = logger;
        }        

        public double GObying => throw new System.NotImplementedException();

        public double GOselling => throw new System.NotImplementedException();

        public double Rate { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public Currency Currency => throw new System.NotImplementedException();

        public void Initialize(List<Security> securities)
        {
            throw new System.NotImplementedException();
        }
    }
}
