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

            var logger = new Logger();
            MainSystem system = new SampleMainSystem();

            if (isLoggerOn == 1)
                system.Logger = new LoggerSystem(context);

            var systemParameters = new SystemParameters();

            systemParameters.Add("positionSide", positionSide);
            systemParameters.Add("isUSD", isUSD);
            systemParameters.Add("rateUSD", rateUSD);
            systemParameters.Add("shares", shares);
            systemParameters.Add("SMA", new OptimProperty(value: 13, minValue: 9, maxValue: 50, step: 1));

        }
    }
}