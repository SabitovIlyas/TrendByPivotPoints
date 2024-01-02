using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;

namespace CorrelationCalculator
{
    public class BarsCorrelationPreparator
    {
        private List<Bar> barsFirstSecurity;
        private List<Bar> barsSecondSecurity;
        private List<DateTime> timeLine;
        private string marked = "Marked";
        private string firstSecurityPeriod = string.Empty;
        private string secondSecurityPeriod = string.Empty;
        private int firstSecurityDigitsAfterPoint = 0;
        private int secondSecurityDigitsAfterPoint = 0;

        public BarsCorrelationPreparator(List<Bar> barsFirstSecurity, List<Bar> barsSecondSecurity)
        {
            this.barsFirstSecurity = barsFirstSecurity;
            this.barsSecondSecurity = barsSecondSecurity;

            firstSecurityPeriod = barsFirstSecurity.First().Period;
            secondSecurityPeriod = barsSecondSecurity.First().Period;
            firstSecurityDigitsAfterPoint = barsFirstSecurity.First().DigitsAfterPoint;
            secondSecurityDigitsAfterPoint = barsSecondSecurity.First().DigitsAfterPoint;
        }

        //public BarsCorrelationPreparator(SecurityInfo securityFirst, SecurityInfo securitySecond)
        //{
        //    var n = securityFirst.Security.GetBarsCountReal();
        //    barsFirstSecurity = securityFirst.GetBars(n - 1);

        //    n = securitySecond.GetBarsCountReal();
        //    barsSecondSecurity = securitySecond.GetBars(n - 1);

        //    var sec1 = (SecurityTSlab)securityFirst;
        //    firstSecurityPeriod = sec1.security.Interval.ToString() + sec1.security.IntervalBase.ToString();
        //    var sec2 = (SecurityTSlab)securityFirst;
        //    secondSecurityPeriod = sec2.security.Interval.ToString() + sec2.security.IntervalBase.ToString();
        //    firstSecurityDigitsAfterPoint = -1;
        //    secondSecurityDigitsAfterPoint = -1;
        //}

        public void Prepare()
        {
            timeLine = CreateTimeLine();
            CreateMissedBars();
            ExtraPolateMissedBars(barsFirstSecurity);
            InterpolateMissedBars(barsFirstSecurity, firstSecurityDigitsAfterPoint);

            ExtraPolateMissedBars(barsSecondSecurity);
            InterpolateMissedBars(barsSecondSecurity, secondSecurityDigitsAfterPoint);
        }
        
        private List<DateTime> CreateTimeLine()
        {
            var listDateTime = new List<DateTime>();
            foreach (Bar bar in barsFirstSecurity)
                listDateTime.Add(bar.Date);

            foreach (Bar bar in barsSecondSecurity)
                if (!listDateTime.Contains(bar.Date))
                    listDateTime.Add(bar.Date);

            listDateTime.Sort();
            return listDateTime;
        }

        private void CreateMissedBars()
        {
            var doesBarsFirstSecurityHaveMarkedElements = false;
            var doesBarsSecondSecurityHaveMarkedElements = false;
            foreach (var time in timeLine)
            {
                if (barsFirstSecurity.Where(p => p.Date == time).Count() == 0)
                {
                    barsFirstSecurity.Add(Bar.Create(time, 0, 0, 0, 0, 0, ticker: marked, period: firstSecurityPeriod));
                    doesBarsFirstSecurityHaveMarkedElements = true;
                }
                if (barsSecondSecurity.Where(p => p.Date == time).Count() == 0)
                {
                    barsSecondSecurity.Add(Bar.Create(time, 0, 0, 0, 0, 0, ticker: marked, period: secondSecurityPeriod));
                    doesBarsSecondSecurityHaveMarkedElements = true;
                }
            }

            var comparer = new BarsComparer();

            if (doesBarsFirstSecurityHaveMarkedElements)
                barsFirstSecurity.Sort(comparer);

            if (doesBarsSecondSecurityHaveMarkedElements)
                barsSecondSecurity.Sort(comparer);
        }

        private void ExtraPolateMissedBars(List<Bar> bars)
        {
            int index;

            index = 0;
            if (bars[index].Ticker == marked)
            {
                var bar = bars.Find(p => p.Ticker != marked);
                bars[index].Open = bar.Open;
                bars[index].High = bar.High;
                bars[index].Low = bar.Low;
                bars[index].Close = bar.Close;
                bars[index].Volume = bar.Volume;
                bars[index].Ticker = bar.Ticker;
            }

            index = bars.Count - 1;
            if (bars[index].Ticker == marked)
            {
                var bar = bars.FindLast(p => p.Ticker != marked);
                bars[index].Open = bar.Open;
                bars[index].High = bar.High;
                bars[index].Low = bar.Low;
                bars[index].Close = bar.Close;
                bars[index].Volume = bar.Volume;
                bars[index].Ticker = bar.Ticker;
            }
        }       

        private void InterpolateMissedBars(List<Bar> bars, int digitsAfterPoint)
        {
            for (var i = 1; i < bars.Count - 1; i++)
            {
                if (bars[i].Ticker != marked)
                    continue;

                var bar = bars[i];
                var barLeft = bars[i - 1];

                int j;
                for (j = i + 1; j < bars.Count; j++)
                    if (bars[j].Ticker != marked)
                        break;

                var barRight = bars[j];

                if (digitsAfterPoint != -1)
                {
                    bar.Open = Math.Round(barLeft.Open + (barRight.Open - barLeft.Open) / (j - i + 1), digitsAfterPoint, MidpointRounding.AwayFromZero);
                    bar.High = Math.Round(barLeft.High + (barRight.High - barLeft.High) / (j - i + 1), digitsAfterPoint, MidpointRounding.AwayFromZero);
                    bar.Low = Math.Round(barLeft.Low + (barRight.Low - barLeft.Low) / (j - i + 1), digitsAfterPoint, MidpointRounding.AwayFromZero);
                    bar.Close = Math.Round(barLeft.Close + (barRight.Close - barLeft.Close) / (j - i + 1), digitsAfterPoint, MidpointRounding.AwayFromZero);
                    bar.Volume = Math.Round(barLeft.Volume + (barRight.Volume - barLeft.Volume) / (j - i + 1), digitsAfterPoint, MidpointRounding.AwayFromZero);
                    bar.Ticker = barLeft.Ticker;
                }
                else
                {
                    bar.Open = barLeft.Open + (barRight.Open - barLeft.Open) / (j - i + 1);
                    bar.High = barLeft.High + (barRight.High - barLeft.High) / (j - i + 1);
                    bar.Low = barLeft.Low + (barRight.Low - barLeft.Low) / (j - i + 1);
                    bar.Close = barLeft.Close + (barRight.Close - barLeft.Close) / (j - i + 1);
                    bar.Volume = barLeft.Volume + (barRight.Volume - barLeft.Volume) / (j - i + 1);
                    bar.Ticker = barLeft.Ticker;
                }                
            }
        }
    }
}