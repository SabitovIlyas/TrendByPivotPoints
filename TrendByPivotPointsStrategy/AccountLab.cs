using System.Collections.Generic;

namespace TradingSystems
{
    public class AccountLab : Account
    {
        public override double InitDeposit => initDeposit;
        public override double Equity => equity;

        public override double FreeBalance => freeBalance;

        protected double initDeposit;
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
        public override void Update(int barNumber)
        {
            foreach (var security in securities)            
                ((SecurityLab)security).Update(barNumber);
            
            base.Update(barNumber);
        }

        public virtual double GetEquity(int barNumber)
        {
            var equity = initDeposit;
            foreach (var security in securities)
                equity += ((SecurityLab)security).GetProfit(barNumber);

            return equity;
        }

        public double GetDrawDown(int barNumber)
        {
            var maxCapital = initDeposit;
            var maxDrawdown = 0d;

            for (var i = 1; i <= barNumber; i++)
            {
                var capital = GetEquity(i);
                var drawdown = (maxCapital - capital) / maxCapital * 100;
                
                if (drawdown > maxDrawdown)                
                    maxDrawdown = drawdown;                

                if (capital > maxCapital)                
                    maxCapital = capital;                
            }            

            return maxDrawdown;
        }
    }
}