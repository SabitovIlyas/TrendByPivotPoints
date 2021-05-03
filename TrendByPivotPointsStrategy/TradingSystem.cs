using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLab.DataSource;
using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    public class TradingSystem    
    {
        List<Bar> bars;
        LocalMoneyManager localMoneyManager;
        Account account;
        ISecurity sec;
        PivotPointsIndicator pivotPointsIndicator;
        PatternPivotPoints_1l2g3 patternPivotPoints_1l2g3;
        PatternPivotPoints_1g2 patternPivotPoints_1g2;
        PatternPivotPoints_1l2 patternPivotPoints_1l2;
        Double takeProfitLong;
        int counter;

        public TradingSystem(List<Bar> bars, LocalMoneyManager localMoneyManager, Account account)
        {
            this.bars = bars;
            this.localMoneyManager = localMoneyManager;
            this.account = account;
            sec = account.Security;

            pivotPointsIndicator = new PivotPointsIndicator();
            patternPivotPoints_1l2g3 = new PatternPivotPoints_1l2g3();
            patternPivotPoints_1g2 = new PatternPivotPoints_1g2();
            patternPivotPoints_1l2 = new PatternPivotPoints_1l2();
        }

        public void Update(int barNumber)
        {
            var le = sec.Positions.GetLastActiveForSignal("LE");
            counter++;
            TrendByPivotPointsStrategy.ctx.Log("counter " + counter.ToString());
            var subBars = GetSubBars(barNumber);
            var lastBar = subBars.Last();

            if (le == null)
            {
                

                var lows = pivotPointsIndicator.GetLows(subBars, 3, 2);
                var values = new List<double>();
                foreach (var low in lows)
                    values.Add(low.Value);

                var compressedSec = sec.CompressTo(new Interval(30, DataIntervals.MINUTE));

                var compressedBars = new List<Bar>();
                foreach (var compressedBar in compressedSec.Bars)
                    if (compressedBar.Date < sec.Bars[barNumber].Date)
                    {
                        compressedBars.Add(new Bar() { Open = compressedBar.Open, High = compressedBar.High, Low = compressedBar.Low, Close = compressedBar.Close, Date = compressedBar.Date });
                    }

                var lowsFilter = pivotPointsIndicator.GetLows(compressedBars, 3, 3);

                var valuesFilterLows = new List<double>();
                foreach (var low in lowsFilter)
                    valuesFilterLows.Add(low.Value);

                var highsFilter = pivotPointsIndicator.GetHighs(compressedBars, 3, 3);

                var valuesFilterHighs = new List<double>();
                foreach (var high in highsFilter)
                    valuesFilterHighs.Add(high.Value);

                TrendByPivotPointsStrategy.ctx.Log("values.Count = " + valuesFilterHighs.Count.ToString());

                if (!IsAboutEndOfSession(lastBar.Date))
                {
                    if (patternPivotPoints_1l2g3.Check(values) && patternPivotPoints_1g2.Check(valuesFilterLows) && !patternPivotPoints_1l2.Check(valuesFilterHighs))
                    //if (patternPivotPoints_1l2g3.Check(values) && patternPivotPoints_1g2.Check(valuesFilterLows))
                    //if (patternPivotPoints_1l2g3.Check(values))
                    {
                        if (barNumber == lows.Last().BarNumber + 3)
                        {
                            var lowLast = lows.Last();
                            var stopPrice = lowLast.Value - 1;
                            if (subBars.Last().Close >= stopPrice)
                            {
                                var contracts = 1;
                                sec.Positions.BuyAtMarket(barNumber + 1, contracts, "LE");
                                takeProfitLong = 0;
                            }
                        }
                    }
                }                
            }
            else
            {
                var lows = pivotPointsIndicator.GetLows(subBars, 3, 3);
                var low = lows.Last();

                var riskValue = le.EntryPrice - (low.Value - 1);

                if (takeProfitLong == 0)
                    takeProfitLong = le.EntryPrice + riskValue*2;
                var stopLoss = low.Value - 1;

                if (IsAboutEndOfSession(lastBar.Date))
                    le.CloseAtMarket(barNumber + 1, "LXT");

                le.CloseAtStop(barNumber + 1, stopLoss, "LXS");
                le.CloseAtProfit(barNumber + 1, takeProfitLong, "LXP");

                //if (IsAboutEndOfSession(lastBar.Date))
                //    le.CloseAtMarket(barNumber + 1, "LXT");
            }
        }

        private List<Bar> GetSubBars(int barNumber)
        {
            var subBars = new List<Bar>();
            for (var i = 0; i <= barNumber; i++)
                subBars.Add(bars[i]);

            return subBars;
        }

        private bool IsAboutEndOfSession(DateTime barDateTime)
        {
            if (barDateTime.Hour == 23 && barDateTime.Minute == 40)
                return true;
            return false;
        }
    }
}