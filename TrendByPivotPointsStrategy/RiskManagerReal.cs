using System;

namespace TradingSystems
{
    public class RiskManagerReal : RiskManager
    {
        public double RiskValuePrcnt { get { return riskValuePrcnt; } }
        private double riskValuePrcnt;
        private double riskValue;
        private Account account;
        private Logger logger = new NullLogger();
        public Logger Logger { get { return logger; } set { logger = value; } }

        public RiskManagerReal(Account account, Logger logger, double riskValuePrcnt = 100)
        {
            Logger = logger;
            this.account = account;
            this.riskValuePrcnt = riskValuePrcnt;
            riskValue = riskValuePrcnt / 100.0;
        }        

        public double GetMoneyForDeal()
        {
            logger.Log("Получаем средства для совершения сделки...");
            var moneyForDeal = riskValue * Equity;
            var message = string.Format("Equity = {0}; риск в % от Equity = {1}; рискуем следующей суммой: {2}", Equity, riskValue * 100, moneyForDeal);
            logger.Log(message);
            return moneyForDeal;
        }

        public double Equity => account.Equity;

        public double FreeBalance => account.FreeBalance;
    }
}
