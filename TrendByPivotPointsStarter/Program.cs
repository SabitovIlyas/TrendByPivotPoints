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
            //var script = new SampleScript();
            //var context = CustomContext.Create();

            //var initDeposit = 1000000; //1 млн
            //var finInfo = new FinInfo();
            //var bars = new ReadAndAddList<DataBar>();

            //var security = CustomSecurity.Create(initDeposit, finInfo, bars); //переделать. Не CustomSecurity, а SecurityLab          

            //script.Execute(context, security);

            //var initDeposit = 1000000; //1 млн
            //var finInfo = new FinInfo();
            //var bars = new ReadAndAddList<DataBar>();


            Security security = new SecurityLab();
            var securities = new List<Security> { security };
            Logger logger = new ConsoleLogger();
            List<TradingSystem> tradingSystems = new List<TradingSystem>() { new TradingSystemSMA() };
            
            var systemParameters = new SystemParameters();

            systemParameters.Add("positionSide", PositionSide.Long);
            systemParameters.Add("isUSD", true);
            systemParameters.Add("rateUSD", 90);
            systemParameters.Add("shares", 10);
            systemParameters.Add("SMA", 9);
            

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