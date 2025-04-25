using System.Windows.Forms;
using TradingSystems;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class TradingSystemParameters
    {
        public Security Security;
        public SystemParameters SystemParameters;

        public TradingSystemParameters() { }        

        public TradingSystemParameters(TradingSystemParameters tradingSystemParameters)
        {
            Security = tradingSystemParameters.Security.GetClone();
            SystemParameters = new SystemParameters(tradingSystemParameters.SystemParameters);
        }
    }
}