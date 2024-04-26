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
            Logger logger = new ConsoleLogger();           

            try
            {
                Starter system = new LabStarter(logger);
                system.Initialize();
                system.Run();
                system.Paint();
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
        }
    }
}