using System;
using TradingSystems;
using Security = TradingSystems.Security;
using System.Collections.Generic;

namespace TrendByPivotPointsStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            Security security = new SecurityLab(Currency.Ruble, shares: 1);
            var securities = new List<Security> { security };
            Logger logger = new ConsoleLogger();
            Account account = new AccountLab(initDeposit: 1000000, logger);
            RiskManager riskManager = new RiskManagerReal(account);
            ContractsManager contractsManager = new ContractsManager(riskManager, account, securities);

            var systemParameters = new SystemParameters();

            systemParameters.Add("positionSide", PositionSide.Long);
            systemParameters.Add("isUSD", true);
            systemParameters.Add("rateUSD", 90);
            systemParameters.Add("shares", 10);
            systemParameters.Add("SMA", 9);

            

            List<TradingSystem> tradingSystems = new List<TradingSystem>() { new TradingSystemSMA(securities, systemParameters, logger ) };

            try
            {
                MainSystem system = new LabMainSystem(tradingSystems, systemParameters, securities, logger);                
                system.Run();                
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
        }
    }
}