using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TSLab.Script.Handlers;

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

        public virtual double GetEquity()
        {
            var lastBarNumber = securities.First().Bars.Count - 1;
            return GetEquity(lastBarNumber);
        }

        public virtual double GetEquity(int barNumber)
        {
            var equity = initDeposit;
            foreach (var security in securities)
                equity += ((SecurityLab)security).GetProfit(barNumber);

            var d = CountDecimalPlaces(securities.First().GetBarClose(barNumber));
            return Math.Round(equity, d);
        }

        private int CountDecimalPlaces(double value)
        {
            string strValue = value.ToString();
            var currentCulture = CultureInfo.CurrentCulture;
            char decimalSeparator = currentCulture.NumberFormat.NumberDecimalSeparator[0];

            int pointIndex = strValue.IndexOf(decimalSeparator);

            if (pointIndex != -1)
                return strValue.Length - pointIndex - 1;

            return 0;
        }      

        public double GetMaxDrawDown(int barNumber)
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

        public override double GetMaxDrawDown()
        {
            var lastBarNumber = securities.First().Bars.Count - 1;
            return GetMaxDrawDown(lastBarNumber);            
        }

        public virtual double GetRecoveryFactor()
        {
            var lastBarNumber = securities.First().Bars.Count - 1;
            var maxDrawdown = GetMaxDrawDown(lastBarNumber);
            var d = CountDecimalPlaces(securities.First().GetBarClose(lastBarNumber));
            var result = Math.Round(securities.First().GetProfit() / GetMaxDrawDown(), d);

            var result;
        }
    }
}