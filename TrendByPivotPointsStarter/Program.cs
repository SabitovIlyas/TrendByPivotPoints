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
            List<Security> securities = new List<Security> { security };
            Logger logger = new ConsoleLogger();
            Account account = new AccountLab(initDeposit: 1000000, logger);
            RiskManager riskManager = new RiskManagerReal(account);
            ContractsManager contractsManager = new ContractsManager(riskManager, account, securities);
            Indicators indicators = new IndicatorsTsLab();

            SystemParameters systemParameters = new SystemParameters();

            systemParameters.Add("positionSide", PositionSide.Long);
            systemParameters.Add("isUSD", true);
            systemParameters.Add("rateUSD", 90);
            systemParameters.Add("shares", 10);
            systemParameters.Add("SMA", 9);            

            List<TradingSystem> tradingSystems = new List<TradingSystem>() { new TradingSystemSMA(securities, systemParameters, contractsManager, indicators, logger) };

            try
            {
                MainSystem system = new LabMainSystem(tradingSystems, systemParameters, securities, logger);                
                system.Run();                //реализовать этот метод в TradingSystem. От класса MainSystem отказываемся. Или не отказываться, а назвать его Starter или Runner
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
        }
    }
}