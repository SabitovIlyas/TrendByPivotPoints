using System.Collections.Generic;
using System.Linq;
using TSLab.Script.Helpers;

namespace TradingSystems
{
    public class IndicatorsTsLab : Indicators
    {
        public List<double> SMA(List<double> bars, int period)
        {
            return Series.SMA(bars, period).ToList();
        }
    }
}