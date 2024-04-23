using System.Collections.Generic;

namespace TradingSystems
{
    public interface Indicators
    {
        List<double> SMA(List<double> bars, int period);        
    }
}