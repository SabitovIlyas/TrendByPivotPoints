using System;
using System.Collections.Generic;
using System.Linq;
using TrendByPivotPointsPeparatorDataForSpread;

namespace CorrelationCalculator
{
    public class BarsCorrelationPreparator
    {
        public List<Bar> BarsFirstSecurityPrepared { get; private set; } = new List<Bar>();
        public List<Bar> BarsSecondSecurityPrepared { get; private set; } = new List<Bar>();

        private List<Bar> barsFirstSecurity;
        private List<Bar> barsSecondSecurity;
        private List<DateTime> timeLine;
        private string marked = "Marked";
        private string firstSecurityPeriod = string.Empty;
        private string secondSecurityPeriod = string.Empty;

        public BarsCorrelationPreparator(List<Bar> barsFirstSecurity, List<Bar> barsSecondSecurity)
        {
            this.barsFirstSecurity = barsFirstSecurity;
            this.barsSecondSecurity = barsSecondSecurity;
            firstSecurityPeriod = barsFirstSecurity.First().Period;
            secondSecurityPeriod = barsSecondSecurity.First().Period;
        }

        public void Prepare()
        {
            timeLine = CreateTimeLine();
            CreateMissedBars();
            ExtraPolateMissedBars();
            InterpolateMissedBars();
        }
        
        private List<DateTime> CreateTimeLine()
        {
            var listDateTime = new List<DateTime>();
            foreach (Bar bar in barsFirstSecurity)
                listDateTime.Add(bar.DateTime);

            foreach (Bar bar in barsSecondSecurity)
                if (!listDateTime.Contains(bar.DateTime))
                    listDateTime.Add(bar.DateTime);

            listDateTime.Sort();
            return listDateTime;
        }

        private void CreateMissedBars()
        {
            var doesBarsFirstSecurityHaveMarkedElements = false;
            var doesBarsSecondSecurityHaveMarkedElements = false;
            foreach (var time in timeLine)
            {
                if (barsFirstSecurity.Where(p => p.DateTime == time).Count() == 0)
                {
                    barsFirstSecurity.Add(Bar.Create(time, 0, 0, 0, 0, 0, ticker: marked));
                    doesBarsFirstSecurityHaveMarkedElements = true;
                }
                if (barsSecondSecurity.Where(p => p.DateTime == time).Count() == 0)
                {
                    barsSecondSecurity.Add(Bar.Create(time, 0, 0, 0, 0, 0, ticker: marked));
                    doesBarsSecondSecurityHaveMarkedElements = true;
                }
            }

            var comparer = new BarsComparer();

            if (doesBarsFirstSecurityHaveMarkedElements)
                barsFirstSecurity.Sort(comparer);

            if (doesBarsSecondSecurityHaveMarkedElements)
                barsSecondSecurity.Sort(comparer);
        }

        private void ExtraPolateMissedBars()
        {
            int index;

            index = 0;
            if (barsFirstSecurity[index].Ticker == marked)
            {
                var bar = barsFirstSecurity.Find(p => p.Ticker != marked);
                barsFirstSecurity[index].Open = bar.Open;
                barsFirstSecurity[index].High = bar.High;
                barsFirstSecurity[index].Low = bar.Low;
                barsFirstSecurity[index].Close = bar.Close;
                barsFirstSecurity[index].Volume = bar.Volume;
                barsFirstSecurity[index].Ticker = bar.Ticker;
            }

            index = barsFirstSecurity.Count - 1;
            if (barsFirstSecurity[index].Ticker == marked)
            {
                var bar = barsFirstSecurity.FindLast(p => p.Ticker != marked);
                barsFirstSecurity[index].Open = bar.Open;
                barsFirstSecurity[index].High = bar.High;
                barsFirstSecurity[index].Low = bar.Low;
                barsFirstSecurity[index].Close = bar.Close;
                barsFirstSecurity[index].Volume = bar.Volume;
                barsFirstSecurity[index].Ticker = bar.Ticker;
            }
        }


        private void InterpolateMissedBars()
        {            
            for (var i = 1; i < barsFirstSecurity.Count-1; i++)
            {
                if (barsFirstSecurity[i].Ticker != marked)
                    continue;

                var bar = barsFirstSecurity[i];
                var barLeft = barsFirstSecurity[i-1];

                int j;
                for (j = i + 1; j < barsFirstSecurity.Count; j++)
                    if (barsFirstSecurity[j].Ticker != marked)
                        break;

                var barRight = barsFirstSecurity[j];

                bar.Open = barLeft.Open + (barRight.Open - barLeft.Open) / (j - i + 1);
                bar.High = barLeft.High + (barRight.High - barLeft.High) / (j - i + 1);
                bar.Low = barLeft.Low + (barRight.Low - barLeft.Low) / (j - i + 1);
                bar.Close = barLeft.Close + (barRight.Close - barLeft.Close) / (j - i + 1);
                bar.Volume = barLeft.Volume + (barRight.Volume - barLeft.Volume) / (j - i + 1);
            }
        }
    }
}