using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public abstract class Starter
    {
        protected Context context;
        protected List<TradingSystem> tradingSystems;
        protected int securityNumber;

        protected double rateUSD;
        protected PositionSide positionSide;
        protected double comission;
        protected double riskValuePrcnt;
        protected Currency currency;

        protected int shares;
        protected Logger logger = new NullLogger();
        protected Account account;
        protected SystemParameters systemParameters;
        protected List<Security> securities;
        protected Security securityFirst;

        public Logger Logger { get { return logger; } set { logger = value; } }

        public virtual void Initialize()
        {
            securityFirst = securities.First();
        }
        
        public abstract void Paint();
        public void Run()
        {
            foreach (var tradingSystem in tradingSystems)
                tradingSystem.CalculateIndicators();

            var lastBarNumber = securityFirst.GetBarsCountReal() - 1;
            if (lastBarNumber < 1)
                return;

            for (var barNumber = 0; barNumber <= lastBarNumber; barNumber++)
            {
                foreach (var tradingSystem in tradingSystems)
                {
                    tradingSystem.Update(barNumber);
                    account.Update(barNumber);
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
                var rateUSD = (double)systemParameters.GetValue("rateUSD");
                var shares = (int)systemParameters.GetValue("shares");

                this.positionSide = PositionSide.Null;
                if (positionSide == 0)
                    this.positionSide = PositionSide.Long;
                else if (positionSide == 1)
                    this.positionSide = PositionSide.Short;

                if (isUSD == 1)
                    currency = Currency.USD;
                currency = Currency.Ruble;

                this.rateUSD = rateUSD;
                this.shares = shares;
            }
            catch (KeyNotFoundException e)
            {
                logger.Log("Прекращаем работу, так как не установлен параметр: ", e.Message);
                throw new ApplicationException("Не удалось установить основные параметры для торговой системы.");
            }
        }

        protected bool IsLaboratory(ISecurity security)
        {
            var realTimeSecurity = security as ISecurityRt;
            return realTimeSecurity == null;
        }
    }
}