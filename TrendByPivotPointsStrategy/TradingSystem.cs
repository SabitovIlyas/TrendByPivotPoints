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
        Security security;

        public Logger Logger
        {
            get
            {
                return logger;
            }

            set
            {
                logger = value;
            }
        }

        Logger logger = new NullLogger();

        public TradingSystem(List<Bar> bars, LocalMoneyManager localMoneyManager, Account account, Security security)
        {
            this.bars = bars;
            if (bars == null)
                throw new ArgumentNullException("List<Bar> bars", "В конструктор передан null вместо списка баров");

            if (bars.Count == 0)
                throw new Exception("В конструктор передан пустой список баров");

            this.localMoneyManager = localMoneyManager;
            this.account = account;
            sec = account.Security;
            this.security = security;
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
            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber);
            counter++;
            logger.Log("counter " + counter.ToString());
            var subBars = GetSubBars(barNumber);

            var lastBar = subBars.Last();

            logger.Log("subBars.Count = " + subBars.Count.ToString());
            logger.Log("barNumber = " + barNumber.ToString());
            var lows = pivotPointsIndicator.GetLows(subBars, 3, 3);
            logger.Log("1 lows.Count = " + lows.Count.ToString());

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
                if (!IsAboutEndOfSession(lastBar.Date))
                {
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
                if (lows.Count == 0)
                    return;
                var low = lows.Last();
                var stopLoss = low.Value - 1;
                var riskValue = le.EntryPrice - stopLoss;

                logger.Log("low.Value = " + low.Value.ToString());
                logger.Log("stopLoss = " + stopLoss.ToString());
                logger.Log("riskValue = " + riskValue.ToString());
                logger.Log("riskValue * 2 = " + (riskValue * 2).ToString());
                logger.Log("le.EntryPrice = " + le.EntryPrice.ToString());
                logger.Log("takeProfitLong = " + takeProfitLong.ToString());

                if (takeProfitLong == 0)
                    takeProfitLong = le.EntryPrice + riskValue * 2;

                logger.Log("takeProfitLong = " + takeProfitLong.ToString());

                if (IsAboutEndOfSession(lastBar.Date))
                    le.CloseAtMarket(barNumber + 1, "LXT");
                
                le.CloseAtStop(barNumber + 1, stopLoss, 100, "LXS");                
                le.CloseAtProfit(barNumber + 1, takeProfitLong, 100, "LXP");
            }

            #endregion

            #region Short

            var se = sec.Positions.GetLastActiveForSignal("SE", barNumber);

            var highs = pivotPointsIndicator.GetHighs(subBars, 3, 3);
            var highsValues = new List<double>();
            foreach (var high in highs)
                highsValues.Add(high.Value);


            if (se == null)
            {
                if (!IsAboutEndOfSession(lastBar.Date))
                {
                    if (patternPivotPoints_1l2.Check(highsValues) && patternPivotPoints_1l2l3.Check(valuesFilterHighs) && !patternPivotPoints_1g2.Check(valuesFilterLows))
                    {
                        if (barNumber == highs.Last().BarNumber + 3)
                        {
                            var highLast = highs.Last();
                            var stopPrice = highLast.Value + 1;
                            var lastPrice = subBars.Last().Close;
                            if (lastPrice < stopPrice)
                            {
                                //var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Short);
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
                if (highs.Count == 0)
                    return;

                var high = highs.Last();

                var stopLoss = high.Value + 1;
                var riskValue = stopLoss - se.EntryPrice;

                logger.Log("high.Value = " + high.Value.ToString());
                logger.Log("stopLoss = " + stopLoss.ToString());
                logger.Log("riskValue = " + riskValue.ToString());
                logger.Log("riskValue * 2 = " + (riskValue * 2).ToString());
                logger.Log("se.EntryPrice = " + se.EntryPrice.ToString());
                logger.Log("takeProfitShort = " + takeProfitShort.ToString());

                if (takeProfitShort == 0)
                    takeProfitShort = se.EntryPrice - riskValue * 2;

                logger.Log("takeProfitShort = " + takeProfitShort.ToString());

                if (IsAboutEndOfSession(lastBar.Date))
                    se.CloseAtMarket(barNumber + 1, "SXT");

                se.CloseAtStop(barNumber + 1, stopLoss, 100, "SXS");
                se.CloseAtProfit(barNumber + 1, takeProfitShort, 100, "SXP");  
            }

            #endregion
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
    }
}