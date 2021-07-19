using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.DataSource;
using TSLab.Script;
using TsLabColor = TSLab.Script.Color;
using SystemColor = System.Drawing.Color;
using TSLab.Script.GraphPane;

namespace TrendByPivotPointsStrategy
{
    public class TradingSystemPivotPointsTwoTimeFrames
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

        public TradingSystemPivotPointsTwoTimeFrames(LocalMoneyManager localMoneyManager, Account account, Security security)
        {   
            this.localMoneyManager = localMoneyManager;
            this.account = account;
            var securityTSLab = security as SecurityTSlab;
            sec = securityTSLab.security;
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
            var filterBarNumber = compressedSecurity.GetBarCompressedNumberFromBarBaseNumber(barNumber);
            var lowsFilter = pivotPointsIndicatorFilter.GetLows(filterBarNumber - 1);

            var valuesFilterLows = new List<double>();
            foreach (var low in lowsFilter)
                valuesFilterLows.Add(low.Value);

            var highsFilter = pivotPointsIndicatorFilter.GetHighs(filterBarNumber - 1);

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
                                var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Long);
                                //var contracts = 1;
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

            var highs = pivotPointsIndicator.GetHighs(barNumber);
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
                                var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Short);
                                //var contracts = 1;
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
            compressedSecurity = new SecurityTSlab(compressedSec, sec);            
            pivotPointsIndicatorFilter.CalculateLows(compressedSecurity, 3, 3);

            pivotPointsIndicator.CalculateHighs(security, 3, 3);                        
            pivotPointsIndicatorFilter.CalculateHighs(compressedSecurity, 3, 3);
        }        

        private bool IsAboutEndOfSession(DateTime barDateTime)
        {
            if (barDateTime.Hour >= 23 && barDateTime.Minute >= 40)
                return true;
            return false;
        }        

        public void Paint(Context context)
        {           
            var pane1 = context.CreateGraphPane("Инструмент (о. т.)", "Инструмент (основной таймфрейм)");
            var color = SystemColor.Green;                       

            pane1.AddList(sec.ToString(), security, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);           
            
            var pane2 = context.CreateGraphPane("Инструмент  (с. т.)", "Инструмент (средний таймфрейм)");
            pane2.AddList(compressedSecurity.ToString(), compressedSecurity, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);

            pane1.ClearInteractiveObjects();
            pane2.ClearInteractiveObjects();

            color = SystemColor.Blue;
            DateTime x;
            double y;
            MarketPoint position;
            int id = 0;

            var lows = pivotPointsIndicator.GetLows(security.BarNumber);                    

            foreach(var low in lows)
            {
                x = security.GetBarDateTime(low.BarNumber);
                y = low.Value - 50;
                position = new MarketPoint(x, y);                
                pane1.AddInteractivePoint(id.ToString(), PaneSides.RIGHT, false, color, position);
                id++;
            }

            var highs = pivotPointsIndicator.GetHighs(security.BarNumber);

            foreach (var high in highs)
            {
                x = security.GetBarDateTime(high.BarNumber);
                y = high.Value + 50;
                position = new MarketPoint(x, y);
                pane1.AddInteractivePoint(id.ToString(), PaneSides.RIGHT, false, color, position);
                id++;
            }

            var filterBarNumber = compressedSecurity.GetBarCompressedNumberFromBarBaseNumber(security.BarNumber);
            lows = pivotPointsIndicatorFilter.GetLows(filterBarNumber);

            foreach (var low in lows)
            {
                x = compressedSecurity.GetBarDateTime(low.BarNumber);
                y = low.Value - 50;
                position = new MarketPoint(x, y);
                pane2.AddInteractivePoint(id.ToString(), PaneSides.RIGHT, false, color, position);
                id++;
            }

            highs = pivotPointsIndicatorFilter.GetHighs(filterBarNumber);

            foreach (var high in highs)
            {
                x = compressedSecurity.GetBarDateTime(high.BarNumber);
                y = high.Value + 50;
                position = new MarketPoint(x, y);
                pane2.AddInteractivePoint(id.ToString(), PaneSides.RIGHT, false, color, position);
                id++;
            }
        }
    }
}