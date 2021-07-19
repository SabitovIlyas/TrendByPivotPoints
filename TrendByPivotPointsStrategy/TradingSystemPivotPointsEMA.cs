using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.GraphPane;
using TSLab.Script.Helpers;

namespace TrendByPivotPointsStrategy
{
    public class TradingSystemPivotPointsEMA
    {
        LocalMoneyManager localMoneyManager;
        ISecurity sec;
        PivotPointsIndicator pivotPointsIndicator;
        Security security;

        PatternPivotPoints_1g2 patternPivotPoints_1g2;
        PatternPivotPoints_1l2 patternPivotPoints_1l2;                
        
        double takeProfitLong;
        double takeProfitShort;
        double stopLossLong;
        double stopLossShort;

        public Logger Logger { get; set; } = new NullLogger();
        bool flagToDebugLog = false;

        public TradingSystemPivotPointsEMA(LocalMoneyManager localMoneyManager, Account account, Security security)
        {   
            this.localMoneyManager = localMoneyManager;            
            var securityTSLab = security as SecurityTSlab;
            sec = securityTSLab.security;
            this.security = security;
            pivotPointsIndicator = new PivotPointsIndicator();            
            patternPivotPoints_1g2 = new PatternPivotPoints_1g2();
            patternPivotPoints_1l2 = new PatternPivotPoints_1l2();            
        }

        public void Update(int barNumber)
        {
            security.BarNumber = barNumber;
            if (!flagToDebugLog)
            {
                var message = string.Format("ГО на покупку: {0}; ГО на продажу: {1}; Шаг цены: {2}", security.BuyDeposit, security.SellDeposit, security.StepPrice);                
                Logger.Log(message);
                flagToDebugLog = true;
            }            

            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber);            
            var subBars = security.GetBars(barNumber);

            var lastBar = security.LastBar;           
            var lows = pivotPointsIndicator.GetLows(barNumber);
            Logger.Log("lows.Count = " + lows.Count.ToString());

            var lowsValues = new List<double>();
            foreach (var low in lows)
                lowsValues.Add(low.Value);

            #region Long

            if (le == null)
            {
                if (patternPivotPoints_1g2.Check(lowsValues))
                {
                    Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие входа выполнено!");
                    if (barNumber == lows.Last().BarNumber + 3)
                    {
                        var lowLast = lows.Last();
                        var stopPrice = lowLast.Value - 1;
                        var lastPrice = lastBar.Close;
                        if (lastPrice > stopPrice)
                        {
                            var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Long);
                            sec.Positions.BuyAtMarket(barNumber + 1, contracts, "LE");
                            takeProfitLong = 0;
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
                if (patternPivotPoints_1l2.Check(highsValues))
                {
                    if (barNumber == highs.Last().BarNumber + 3)
                    {
                        var highLast = highs.Last();
                        var stopPrice = highLast.Value + 1;
                        var lastPrice = lastBar.Close;
                        if (lastPrice < stopPrice)
                        {
                            var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Short);                 
                            sec.Positions.SellAtMarket(barNumber + 1, contracts, "SE");
                            takeProfitShort = 0;
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
            pivotPointsIndicator.CalculateHighs(security, 3, 3);
            var ema = Series.EMA(sec.ClosePrices, 12);
            var atr = Series.AverageTrueRange(sec.Bars, 12);                
        }

        public void Paint(Context context)
        {           
            var pane1 = context.CreateGraphPane("Инструмент (о. т.)", "Инструмент (основной таймфрейм)");
            var color = SystemColor.Green;                       

            pane1.AddList(sec.ToString(), security, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);           
            pane1.ClearInteractiveObjects();            

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
        }
    }
}