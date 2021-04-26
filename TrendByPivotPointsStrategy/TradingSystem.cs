using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPointsStrategy
{
    public class TradingSystem    
    {
        List<Bar> bars;
        public TradingSystem(List<Bar> bars)
        {
            this.bars = bars;
        }

        public void Update(int barNumber)
        {

        }
    }
}
