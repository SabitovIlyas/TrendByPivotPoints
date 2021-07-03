using System;
using System.Collections.Generic;
using System.Linq;
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
        PivotPointsIndicator pivotPointsIndicatorFilter;
        PatternPivotPoints_1l2g3 patternPivotPoints_1l2g3;
        PatternPivotPoints_1g2 patternPivotPoints_1g2;
        PatternPivotPoints_1l2 patternPivotPoints_1l2;
        PatternPivotPoints_1g2g3 patternPivotPoints_1g2g3;
        PatternPivotPoints_1g2l3 patternPivotPoints_1g2l3;
        PatternPivotPoints_1l2l3 patternPivotPoints_1l2l3;
        double takeProfitLong;
        double takeProfitShort;
        Security security;
        Security compressedSecurity;
        double stopLossLong;
        double stopLossShort;

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

        public TradingSystem(LocalMoneyManager localMoneyManager, Account account, Security security)
        {   
            this.localMoneyManager = localMoneyManager;
            this.account = account;
            sec = account.Security;
            this.security = security;
            pivotPointsIndicator = new PivotPointsIndicator();
            pivotPointsIndicatorFilter = new PivotPointsIndicator();
            patternPivotPoints_1l2g3 = new PatternPivotPoints_1l2g3();
            patternPivotPoints_1g2 = new PatternPivotPoints_1g2();
            patternPivotPoints_1l2 = new PatternPivotPoints_1l2();
            patternPivotPoints_1g2g3 = new PatternPivotPoints_1g2g3();
            patternPivotPoints_1g2l3 = new PatternPivotPoints_1g2l3();
            patternPivotPoints_1l2l3 = new PatternPivotPoints_1l2l3();
        }

        bool flagToDebugLog = false;
        public void Update(int barNumber)
        {
            security.BarNumber = barNumber;
            if (!flagToDebugLog)
            {
                var message = string.Format("ГО на покупку: {0}; ГО на продажу: {1}; Шаг цены: {2}", security.BuyDeposit, security.SellDeposit, security.StepPrice);                
                logger.Log(message);
                flagToDebugLog = true;
            }            

            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber);            
            var subBars = security.GetBars(barNumber);

            var lastBar = security.LastBar;           
            var lows = pivotPointsIndicator.GetLows(barNumber);
            logger.Log("lows.Count = " + lows.Count.ToString());

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

            //var lowsFilter = pivotPointsIndicator.GetLows(compressedBars, 3, 3);
            var filterBarNumber = compressedSecurity.GetBarCompressedNumberFromBarBaseNumber(barNumber);
            var lowsFilter = pivotPointsIndicatorFilter.GetLows(filterBarNumber - 1);

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
                    var message = string.Format("Номер бара = {0}; patternPivotPoints_1g2.Check(lowsValues) = {1}; patternPivotPoints_1g2g3.Check(valuesFilterLows) = {2}; " +
                        "!patternPivotPoints_1l2.Check(valuesFilterHighs) = {3}", barNumber, patternPivotPoints_1g2.Check(lowsValues), patternPivotPoints_1g2g3.Check(valuesFilterLows),
                        !patternPivotPoints_1l2.Check(valuesFilterHighs));
                    logger.Log(message);

                    message = string.Format("lowsValues.Count = {0}", lowsValues.Count);
                    logger.Log(message);

                    if (patternPivotPoints_1g2.Check(lowsValues) && patternPivotPoints_1g2g3.Check(valuesFilterLows) && !patternPivotPoints_1l2.Check(valuesFilterHighs))
                    {
                        logger.Log("Номер бара = " + barNumber.ToString() + "; Условие входа выполнено!");
                        if (barNumber == lows.Last().BarNumber + 3)
                        {
                            var lowLast = lows.Last();
                            var stopPrice = lowLast.Value - 1;                            
                            var lastPrice = lastBar.Close;
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
                stopLossLong = low.Value - 1;
                var riskValue = le.EntryPrice - stopLossLong;             

                if (takeProfitLong == 0)
                    takeProfitLong = le.EntryPrice + riskValue * 2;
              
                if (IsAboutEndOfSession(lastBar.Date))
                    le.CloseAtMarket(barNumber + 1, "LXT");
                
                le.CloseAtStop(barNumber + 1, stopLossLong, 100, "LXS");                
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
                            var lastPrice = lastBar.Close;
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

                stopLossShort = high.Value + 1;
                var riskValue = stopLossShort - se.EntryPrice;               

                if (takeProfitShort == 0)
                    takeProfitShort = se.EntryPrice - riskValue * 2;               

                if (IsAboutEndOfSession(lastBar.Date))
                    se.CloseAtMarket(barNumber + 1, "SXT");

                se.CloseAtStop(barNumber + 1, stopLossShort, 100, "SXS");
                se.CloseAtProfit(barNumber + 1, takeProfitShort, 100, "SXP");  
            }

            #endregion
        }     
        
        public void CheckPositionCloseCase(int barNumber)
        {
            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber);
            if (le != null)
            {
                var bar = security.LastBar;

                if (bar.Low < stopLossLong)
                    le.CloseAtMarket(barNumber, "LXS");
            }                       

            var se = sec.Positions.GetLastActiveForSignal("SE", barNumber);
            if (se != null)
            {
                var bar = security.LastBar;

                if (bar.High > stopLossShort)
                    se.CloseAtMarket(barNumber, "SXS");               
            }
        }    

        public void CalculateIndicators()
        {
            pivotPointsIndicator.CalculateLows(security, 3, 3);
            var compressedSec = sec.CompressTo(new Interval(30, DataIntervals.MINUTE));            
            compressedSecurity = new SecurityReal(compressedSec, sec);            
            pivotPointsIndicatorFilter.CalculateLows(compressedSecurity, 3, 3);
        }

        //private int GetFilterBarNumber()
        //{
        //    compressedSecurity.GetBarsBaseFromBarCompressed

        //}

        private bool IsAboutEndOfSession(DateTime barDateTime)
        {
            if (barDateTime.Hour >= 23 && barDateTime.Minute >= 40)
                return true;
            return false;
        }
    }
}