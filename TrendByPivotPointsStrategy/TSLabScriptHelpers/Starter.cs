using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystems
{
    public abstract class Starter
    {
        public Account Account { get; protected set; }
        public List<NonTradingPeriod> NonTradingPeriods { get;  set; }

        protected List<TradingSystem> tradingSystems;
        protected int securityNumber;
        protected double rateUSD;
        protected PositionSide positionSide;

        protected double comission;
        protected double riskValuePrcnt;
        protected Currency currency;
        protected int shares;

        protected Logger logger = new LoggerNull();
        protected SystemParameters systemParameters;
        protected List<Security> securities;
        protected Security securityFirst;

        protected Context context;
        protected int contracts;
        protected double equity;
        protected double commissionRate;

        public Logger Logger { get { return logger; } set { logger = value; } }

        public virtual void Initialize()
        {
            securityFirst = securities.First();
        }

        public virtual void Paint() { }
        public void Run()
        {
            var lastBarNumber = securityFirst.GetBarsCountReal() - 1;
            if (lastBarNumber < 1)
                return;

            foreach (var tradingSystem in tradingSystems)
                tradingSystem.CalculateIndicators();

            for (var barNumber = 0; barNumber <= lastBarNumber; barNumber++)
            {
                foreach (var tradingSystem in tradingSystems)
                {
                    Account.Update(barNumber);
                    tradingSystem.Update(barNumber);
                }
            }
        }
        public virtual void SetParameters(SystemParameters systemParameters)
        {
            this.systemParameters = systemParameters;
            try
            {
                var positionSide = (int)systemParameters.GetValue("positionSide");
                var isUSD = (int)systemParameters.GetValue("isUSD");
                rateUSD = (double)systemParameters.GetValue("rateUSD");
                shares = (int)systemParameters.GetValue("shares");
                contracts = (int)systemParameters.GetValue("contracts");
                equity = (double)systemParameters.GetValue("equity");
                riskValuePrcnt = (double)systemParameters.GetValue("riskValuePrcnt");

                if (positionSide == 0)
                    this.positionSide = PositionSide.Long;
                else if (positionSide == 1)
                    this.positionSide = PositionSide.Short;
                else
                    this.positionSide = PositionSide.Null;

                if (isUSD == 1)
                    currency = Currency.USD;
                currency = Currency.RUB;
            }
            catch (KeyNotFoundException e)
            {
                logger.Log("Прекращаем работу, так как не установлен параметр: ", e.Message);
                throw new ApplicationException("Не удалось установить основные параметры для торговой системы.");
            }
        }

        public virtual void PrintResults() { }

        public Security GetSecurity()
        {
            return securities.First();
        }
    }
}