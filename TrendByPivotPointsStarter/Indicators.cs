using System.Collections.Generic;
using TradingSystems;
using TSLab.Script.Helpers;

namespace TrendByPivotPointsStarter
{
    public interface Indicators
    {
        List<double> SMA(List<double> bars, int period);        
    }

    public class IndicatorsTsLab : Indicators
    {
        public List<double> SMA(List<double> bars, int period)
        {
            return Series.SMA(bars, period);            
        }
    }

    public class IndicatorsLab : Indicators
    {
        public List<double> SMA(List<double> bars, int period)
        {
            throw new System.NotImplementedException();
        }
    }
}