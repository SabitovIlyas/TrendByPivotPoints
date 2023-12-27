using System.Collections.Generic;
using TradingSystems;

namespace PeparatorDataForSpreadTradingSystems
{
    public class Spread
    {
        public List<Bar> Bars { get; private set; }

        private List<Bar> bars1;
        private List<Bar> bars2;
        public static Spread Create(List<Bar> bars1, List<Bar> bars2)
        {
            return new Spread(bars1, bars2);
        }

        private Spread(List<Bar> bars1, List<Bar> bars2)
        {
            this.bars1 = bars1;
            this.bars2 = bars2;
            CalculateBars();
        }

        private void CalculateBars()
        {
            Bars = new List<Bar>();
            foreach(var bar1 in bars1) 
            {                
                foreach (var bar2 in bars2)
                {
                    if (bar1.Date == bar2.Date)
                        Bars.Add(bar1 - bar2);
                }
            }
        }
    }
}
