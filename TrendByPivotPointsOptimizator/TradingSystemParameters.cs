using System.Windows.Forms;
using TradingSystems;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class TradingSystemParameters
    {
        public Security Security;
        public SystemParameters SystemParameter;

        public TradingSystemParameters() { }        

        public TradingSystemParameters(TradingSystemParameters tradingSystemParameters)
        {
            //Я здесь
            Security = new Security();
            SystemParameter = new SystemParameters();
        }
    }
}