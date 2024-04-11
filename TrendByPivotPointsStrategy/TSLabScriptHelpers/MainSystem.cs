using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public abstract class MainSystem
    {
        protected Context context;
        protected List<TradingStrategy> tradingSystems;
        protected int securityNumber;

        protected double rateUSD;
        protected int positionSide;
        protected double comission;
        protected double riskValuePrcnt;
        protected int isUSD;

        protected int shares;
        protected Logger logger = new NullLogger();
        protected Account account;
        protected SystemParameters systemParameters;
        protected List<Security> securities;
        protected Security securityFirst;

        public Logger Logger { get { return logger; } set { logger = value; } }

        public abstract void Initialize(List<Security> securities);
        protected void InitializeBase()
        {
            securityFirst = securities.First();
            CreateAccount();
        }
        public abstract void Paint();
        public abstract void Run();
        public abstract void SetParameters(SystemParameters systemParameters);
        protected void SetBaseParameters(SystemParameters systemParameters)
        {
            this.systemParameters = systemParameters;
            try
            {                
                positionSide = systemParameters.GetInt("positionSide");
                isUSD = systemParameters.GetInt("isUSD");
                rateUSD = systemParameters.GetDouble("rateUSD");
                shares = systemParameters.GetInt("shares");

            }
            catch (KeyNotFoundException e)
            {
                logger.Log("Прекращаем работу, так как не установлен параметр: ", e.Message);
                throw new ApplicationException("Не удалось установить основные параметры для торговой системы.");
            }
        }
        
        protected Account CreateAccount()
        {
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);
        }

        protected bool IsLaboratory(ISecurity security)
        {
            var realTimeSecurity = security as ISecurityRt;
            return realTimeSecurity == null;
        }
    }
}