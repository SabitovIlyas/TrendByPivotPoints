using System.CodeDom;
using System.Runtime.Remoting.Contexts;
using TradingSystems;
using TSLab.DataSource;
using TSLab.Script.Optimization;

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

            var security = new SecurityLab();
            var logger = new ConsoleLogger();
            MainSystem system = new SampleMainSystem();                        
            system.Logger = logger;
            
            var systemParameters = new SystemParameters();

            systemParameters.Add("positionSide", PositionSide.Long);
            systemParameters.Add("isUSD", true);
            systemParameters.Add("rateUSD", 90);
            systemParameters.Add("shares", 10);
            systemParameters.Add("SMA", 9);

        }
    }
}