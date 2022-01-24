using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class PivotPointsIndicator
    {
        private List<Indicator> lows = new List<Indicator>();
        private List<Indicator> highs = new List<Indicator>();
        private Security security;
        private int rightLocalLows;
        private int rightLocalHighs;

        public void CalculateLows(Security security, int leftLocal, int rightLocal)
        {
            var result = new List<Indicator>();
            var count = security.GetBarsCountReal();
            rightLocalLows = rightLocal;

            for (var i = leftLocal; i < count - rightLocal; i++)
            {
                double low1 = security.GetBarLow(i);
                double low2;

                var low = true;
                for (var j = i - leftLocal; j < i; j++)
                {
                    low2 = security.GetBarLow(j);
                    if (low1 >= low2)
                    {
                        low = false;
                        break;
                    }
                }

                if (low == true)
                {
                    for (var j = i + 1; j <= i + rightLocal; j++)
                    {
                        low2 = security.GetBarLow(j);
                        if (low2 < low1)
                        {
                            low = false;
                            break;
                        }
                    }
                }

                if (low == true)
                    result.Add(new Indicator() { BarNumber = i, Value = low1 });
            }

            //this.security = security;
            lows = result;
        }

        public List<Indicator> GetLows(int barNumber)
        {
            var result = new List<Indicator>();

            foreach (var low in lows)
            {
                if (low.BarNumber + rightLocalLows > barNumber)
                    break;
                result.Add(low);
            }

            return result;
        }

        public List<Indicator> GetLows(List<Bar> bars, int leftLocal, int rightLocal)
        {
            var result = new List<Indicator>();

            for (var i = leftLocal; i < bars.Count - rightLocal; i++)
            {
                var low = true;
                for (var j = i - leftLocal; j < i; j++)
                {
                    if (bars[i].Low >= bars[j].Low)
                    {
                        low = false;
                        break;
                    }
                }

                if (low == true)
                {
                    for (var j = i + 1; j <= i + rightLocal; j++)
                    {
                        if (bars[j].Low < bars[i].Low)
                        {
                            low = false;
                            break;
                        }
                    }
                }

                if (low == true)
                    result.Add(new Indicator() { BarNumber = i, Value = bars[i].Low });
            }

            return result;
        }

        public List<Indicator> GetHighs(List<Bar> bars, int leftLocal, int rightLocal)
        {
            var result = new List<Indicator>();

            for (var i = leftLocal; i < bars.Count - rightLocal; i++)
            {
                var high = true;
                for (var j = i - leftLocal; j < i; j++)
                {
                    if (bars[i].High <= bars[j].High)
                    {
                        high = false;
                        break;
                    }
                }

                if (high == true)
                {
                    for (var j = i + 1; j <= i + rightLocal; j++)
                    {
                        if (bars[j].High > bars[i].High)
                        {
                            high = false;
                            break;
                        }
                    }
                }

                if (high == true)
                    result.Add(new Indicator() { BarNumber = i, Value = bars[i].High });
            }

            return result;
        }

        public void CalculateHighs(Security security, int leftLocal, int rightLocal)
        {
            var result = new List<Indicator>();
            var count = security.GetBarsCountReal();
            rightLocalHighs = rightLocal;

            for (var i = leftLocal; i < count - rightLocal; i++)
            {
                double high1 = security.GetBarHigh(i);
                double high2;

                var high = true;
                for (var j = i - leftLocal; j < i; j++)
                {
                    high2 = security.GetBarHigh(j);
                    if (high1 <= high2)
                    {
                        high = false;
                        break;
                    }
                }

                if (high == true)
                {
                    for (var j = i + 1; j <= i + rightLocal; j++)
                    {
                        high2 = security.GetBarHigh(j);
                        if (high2 > high1)
                        {
                            high = false;
                            break;
                        }
                    }
                }

                if (high == true)
                    result.Add(new Indicator() { BarNumber = i, Value = high1 });
            }

            //this.security = security;
            highs = result;
        }

        public List<Indicator> GetHighs(int barNumber)
        {
            var result = new List<Indicator>();

            foreach (var high in highs)
            {
                if (high.BarNumber + rightLocalHighs > barNumber)
                    break;
                result.Add(high);
            }

            return result;
        }
    }
}
