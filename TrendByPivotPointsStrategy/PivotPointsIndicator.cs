using System.Collections.Generic;

namespace TrendByPivotPointsStrategy
{
    public class PivotPointsIndicator
    {
        private List<Indicator> lows = new List<Indicator>();
        private List<Indicator> highs = new List<Indicator>();
        private int rightLocalExtremums;

        public void CalculateLows(Security security, int leftLocal, int rightLocal, bool isConverted = false)
        {
            var convertable = new Converter(isConverted);
            var result = new List<Indicator>();
            var count = security.GetBarsCountReal();
            rightLocalExtremums = rightLocal;            

            for (var i = leftLocal; i < count - rightLocal; i++)
            {
                double low1 = convertable.GetBarLow(security, i);
                double low2;

                var low = true;
                for (var j = i - leftLocal; j < i; j++)
                {
                    low2 = convertable.GetBarLow(security, j);
                    if (convertable.IsGreaterOrEqual(low1, low2))
                    {
                        low = false;
                        break;
                    }
                }

                if (low == true)
                {
                    for (var j = i + 1; j <= i + rightLocal; j++)
                    {
                        low2 = convertable.GetBarLow(security, j);
                        if (convertable.IsLess(low2, low1))
                        {
                            low = false;
                            break;
                        }
                    }
                }

                if (low == true)
                    result.Add(new Indicator() { BarNumber = i, Value = low1 });
            }

            if (!isConverted)
                lows = result;
            else
                highs = result;
        }

        public List<Indicator> GetLows(int barNumber, bool isConverted = false)
        {
            List<Indicator> extremums;
            if (!isConverted)
                extremums = lows;
            else
                extremums = highs;

            var result = new List<Indicator>();


            foreach (var extremum in extremums)
            {
                if (extremum.BarNumber + rightLocalExtremums > barNumber)
                    break;
                result.Add(extremum);
            }

            return result;
        }

        public void CalculateHighs(Security security, int leftLocal, int rightLocal, bool isConverted = false)
        {
            CalculateLows(security, leftLocal, rightLocal, isConverted: !isConverted);
        }

        public List<Indicator> GetHighs(int barNumber, bool isConverted = false)
        {
            return GetLows(barNumber, isConverted: !isConverted);            
        }
    }
}