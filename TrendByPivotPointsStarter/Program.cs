using System;
using TradingSystems;

namespace TrendByPivotPointsStarter
{
    class Program
    {
        static void Main(string[] args)
        {            
            var logger = new ConsoleLogger();           

            try
            {
                var system = new LabStarterSMATradingSystem(logger);
                system.SetParameters();
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