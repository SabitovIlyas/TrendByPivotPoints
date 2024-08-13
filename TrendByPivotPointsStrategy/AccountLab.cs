using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public class AccountLab : Account
    {
        //TODO: Реализовать этот класс. Пока он пустой.
        private double equity;
        Logger logger;
        private double initDeposit;

        public AccountLab(double initDeposit, Security security, Currency baseCurrency,
            Logger logger) 
        {
            this.initDeposit = initDeposit;
            equity = initDeposit;
        }

        public double InitDeposit => throw new System.NotImplementedException();

        public double Equity => throw new System.NotImplementedException();

        public double GObying => throw new System.NotImplementedException();

        public double GOselling => throw new System.NotImplementedException();

        public double Rate { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public ISecurity Security => throw new System.NotImplementedException();

        public Logger Logger { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public double FreeBalance => throw new System.NotImplementedException();

        public Currency Currency => throw new System.NotImplementedException();

        public void Initialize(List<Security> securities)
        {
            throw new System.NotImplementedException();
        }

        public void Update(int barNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}
