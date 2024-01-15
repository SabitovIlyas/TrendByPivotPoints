using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            var script = new SampleScript();
            var context = CustomContext.Create();

            var initDeposit = 100000;
            var finInfo = new FinInfo();
            var bars = new ReadAndAddList<DataBar>();

            var security = CustomSecurity.Create(initDeposit, finInfo, bars);
            script.Execute(context, security);
        }
    }
}