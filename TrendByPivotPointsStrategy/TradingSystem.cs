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
        PatternPivotPoints_1g2g3 patternPivotPoints_1g2g3;
        PatternPivotPoints_1g2l3 patternPivotPoints_1g2l3;
        PatternPivotPoints_1l2l3 patternPivotPoints_1l2l3;


        Double takeProfitLong;
        Double takeProfitShort;
        int counter;
        DateTime previousBarDateTime;

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
            patternPivotPoints_1g2g3 = new PatternPivotPoints_1g2g3();
            patternPivotPoints_1g2l3 = new PatternPivotPoints_1g2l3();
            patternPivotPoints_1l2l3 = new PatternPivotPoints_1l2l3();
        }
        
        public void Update(int barNumber)
        {
            var le = sec.Positions.GetLastActiveForSignal("LE");
            counter++;
            TrendByPivotPointsStrategy.ctx.Log("counter " + counter.ToString());
            var subBars = GetSubBars(barNumber);
            var lastBar = subBars.Last();

            var lows = pivotPointsIndicator.GetLows(subBars, 3, 3);
            var lowsValues = new List<double>();
            foreach (var low in lows)
                lowsValues.Add(low.Value);

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

            #region Long

            if (le == null)
            {
              

                #region Закомментированный код с фильтрами



                //var compressedSec2 = sec.CompressTo(new Interval(120, DataIntervals.MINUTE));
                //var compressedBars2 = new List<Bar>();

                //foreach (var compressedBar in compressedSec2.Bars)
                //    if (compressedBar.Date < sec.Bars[barNumber].Date)
                //    {
                //        compressedBars2.Add(new Bar() { Open = compressedBar.Open, High = compressedBar.High, Low = compressedBar.Low, Close = compressedBar.Close, Date = compressedBar.Date });
                //    }

                //var lowsFilter2 = pivotPointsIndicator.GetLows(compressedBars2, 3, 3);

                //var valuesFilterLows2 = new List<double>();
                //foreach (var low in lowsFilter2)
                //    valuesFilterLows2.Add(low.Value);

                //var highsFilter2 = pivotPointsIndicator.GetHighs(compressedBars2, 3, 3);

                //var valuesFilterHighs2 = new List<double>();
                //foreach (var high in highsFilter2)
                //    valuesFilterHighs2.Add(high.Value);


                #endregion


                //TrendByPivotPointsStrategy.ctx.Log("values.Count = " + valuesFilterHighs.Count.ToString());


                if (!IsAboutEndOfSession(lastBar.Date))
                {
                    //if (patternPivotPoints_1l2g3.Check(values)
                    //    && patternPivotPoints_1l2g3.Check(valuesFilterLows)
                    //    && patternPivotPoints_1l2g3.Check(valuesFilterLows2))
                    //if (patternPivotPoints_1l2g3.Check(values) && patternPivotPoints_1g2.Check(valuesFilterLows) && !patternPivotPoints_1l2.Check(valuesFilterHighs))
                      //if (patternPivotPoints_1l2g3.Check(lowsValues) && patternPivotPoints_1g2g3.Check(valuesFilterLows) && !patternPivotPoints_1l2.Check(valuesFilterHighs))
                        //if (patternPivotPoints_1l2g3.Check(values) && patternPivotPoints_1g2.Check(valuesFilterLows))
                        //if (patternPivotPoints_1l2g3.Check(values))

                        if (patternPivotPoints_1g2.Check(lowsValues) && patternPivotPoints_1g2g3.Check(valuesFilterLows) && !patternPivotPoints_1l2.Check(valuesFilterHighs))
                        {
                        if (barNumber == lows.Last().BarNumber + 3)
                        {
                            var lowLast = lows.Last();
                            var stopPrice = lowLast.Value - 1;
                            var lastPrice = subBars.Last().Close;
                            if (lastPrice > stopPrice)
                            {
                                //var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Long);
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
                var low = lows.Last();

                var riskValue = le.EntryPrice - (low.Value - 1);

                if (takeProfitLong == 0)
                    takeProfitLong = le.EntryPrice + riskValue*2;
                var stopLoss = low.Value - 1;

                if (IsAboutEndOfSession(lastBar.Date))
                    le.CloseAtMarket(barNumber + 1, "LXT");

                le.CloseAtStop(barNumber + 1, stopLoss, "LXS");
                le.CloseAtProfit(barNumber + 1, takeProfitLong, "LXP");
            }

            #endregion

            var se = sec.Positions.GetLastActiveForSignal("SE");

            var highs = pivotPointsIndicator.GetHighs(subBars, 3, 3);
            var highsValues = new List<double>();
            foreach (var high in highs)
                highsValues.Add(high.Value);


            if (se == null)
            {
                if (!IsAboutEndOfSession(lastBar.Date))
                {
                    //if (patternPivotPoints_1g2l3.Check(highsValues) && patternPivotPoints_1l2l3.Check(valuesFilterHighs) && !patternPivotPoints_1g2.Check(valuesFilterLows))
                    if (patternPivotPoints_1l2.Check(highsValues) && patternPivotPoints_1l2l3.Check(valuesFilterHighs) && !patternPivotPoints_1g2.Check(valuesFilterLows))
                    {
                        if (barNumber == highs.Last().BarNumber + 3)
                        {
                            var highLast = highs.Last();
                            var stopPrice = highLast.Value + 1;
                            var lastPrice = subBars.Last().Close;
                            if (lastPrice < stopPrice)
                            {
                                //var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Long);
                                var contracts = 1;
                                sec.Positions.SellAtMarket(barNumber + 1, contracts, "SE");
                                takeProfitShort = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                var high = highs.Last();

                var riskValue = (high.Value + 1) - se.EntryPrice;

                if (takeProfitShort == 0)
                    takeProfitShort = se.EntryPrice - riskValue * 2;
                var stopLoss = high.Value + 1;

                if (IsAboutEndOfSession(lastBar.Date))
                    se.CloseAtMarket(barNumber + 1, "SXT");

                se.CloseAtStop(barNumber + 1, stopLoss, "SXS");
                se.CloseAtProfit(barNumber + 1, takeProfitShort, "SXP");
            }

            previousBarDateTime = lastBar.Date;
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
            if (barDateTime.Hour >= 23 && barDateTime.Minute >= 40)
                return true;
            return false;
        }

        private bool IsNewTradingSession(DateTime currentBarDateTime)
        {
            if (currentBarDateTime.Date > previousBarDateTime.Date)               
                return true;            

            return false;
        }
    }
}