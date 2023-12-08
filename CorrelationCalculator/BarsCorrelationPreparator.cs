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

        public BarsCorrelationPreparator(List<Bar> barsFirstSecurity, List<Bar> barsSecondSecurity)
        {
            this.barsFirstSecurity = barsFirstSecurity;
            this.barsSecondSecurity = barsSecondSecurity;
        }

        public void Prepare()
        {
            timeLine = CreateTimeLine();
            CreateMissedBars();
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
                    barsFirstSecurity.Add(Bar.Create(time, 0, 0, 0, 0, ticker: "Marked"));
                    doesBarsFirstSecurityHaveMarkedElements = true;
                }
                if (barsSecondSecurity.Where(p => p.DateTime == time).Count() == 0)
                {
                    barsSecondSecurity.Add(Bar.Create(time, 0, 0, 0, 0, ticker: "Marked"));
                    doesBarsSecondSecurityHaveMarkedElements = true;
                }
            }

            var comparer = new BarsComparer();

            if (doesBarsFirstSecurityHaveMarkedElements)
                barsFirstSecurity.Sort(comparer);

            if (doesBarsSecondSecurityHaveMarkedElements)
                barsSecondSecurity.Sort(comparer);
        }

        private void InterpolateMissedBars()
        {            
            for (var i = 0; i < barsFirstSecurity.Count; i++)
            {
                Bar barLeft = null;
                Bar barRight = null;
                Bar bar = barsFirstSecurity[i];
                if (i > 0)
                    barLeft = barsFirstSecurity[i - 1]; //не просто слева, а слева, без пометки Marked

                if (i < barsFirstSecurity.Count - 1)
                    barRight = barsFirstSecurity[i + 1]; //не просто справа, а справа, без пометки Marked

                //if (bar.Ticker == "Marked")
                //    if (barLeft != null)



            }
        }
    }
}