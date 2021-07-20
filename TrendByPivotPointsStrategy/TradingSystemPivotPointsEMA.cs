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
        IList<double> ema;
        IList<double> atr;

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

            var lastPrice = lastBar.Close;

            #region Long

            if (le == null)
            {
                if (patternPivotPoints_1g2.Check(lowsValues) && (lastPrice > ema[barNumber]))
                {
                    Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие входа выполнено! ema.Last() = " + ema[barNumber]);
                    var lowLast = lows.Last();
                    var stopPrice = lowLast.Value - atr.Last();                    
                    if (lastPrice > stopPrice)
                    {
                        var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Long);
                        sec.Positions.BuyAtMarket(barNumber + 1, contracts, "LE");
                    }
                }
            }
            else
            {
                //if (lastPrice <= ema[barNumber])
                //{
                //    le.CloseAtMarket(barNumber + 1, "LXF");
                //    return;
                //}

                if (lows.Count == 0)
                    return;
                var low = lows.Last();
                stopLossLong = low.Value - atr.Last();
                le.CloseAtStop(barNumber + 1, stopLossLong, 100, "LXS");                                
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
                if (patternPivotPoints_1l2.Check(highsValues) && (lastPrice < ema[barNumber]))
                {
                    Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие входа выполнено! ema.Last() = " + ema[barNumber]);
                    var highLast = highs.Last();
                    var stopPrice = highLast.Value + atr.Last();
                    if (lastPrice < stopPrice)
                    {
                        var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, Position.Short);
                        sec.Positions.SellAtMarket(barNumber + 1, contracts, "SE");
                    }
                }
            }
            else
            {
                //if (lastPrice >= ema[barNumber])
                //{
                //    se.CloseAtMarket(barNumber + 1, "SXF");
                //    return;
                //}

                if (highs.Count == 0)
                    return;

                var high = highs.Last();

                stopLossShort = high.Value + atr.Last();
                se.CloseAtStop(barNumber + 1, stopLossShort, 100, "SXS");                
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
            pivotPointsIndicator.CalculateLows(security, 6, 5);
            pivotPointsIndicator.CalculateHighs(security, 5, 4);
            ema = Series.EMA(sec.ClosePrices, 100);
            atr = Series.AverageTrueRange(sec.Bars, 2);                
        }       

        public void Paint(Context context)
        {           
            var pane1 = context.CreateGraphPane(sec.ToString(), "Инструмент (основной таймфрейм)");
            var color = SystemColor.Green;                       

            pane1.AddList(sec.ToString(), security, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);
            //if (id == 0) pane1.ClearInteractiveObjects();
            pane1.ClearInteractiveObjects();
            color = SystemColor.Blue;
            DateTime x;
            double y;
            MarketPoint position; 
            
            var lows = pivotPointsIndicator.GetLows(security.BarNumber);                    

            foreach(var low in lows)
            {
                x = security.GetBarDateTime(low.BarNumber);
                //y = low.Value - 50;
                y = low.Value - 50;
                position = new MarketPoint(x, y);
                var id = low.BarNumber.ToString()+ " " + x.ToLongTimeString() + " " + low.Value.ToString();
                //Logger.Log("id: " + id.ToString());
                pane1.AddInteractivePoint(id, PaneSides.RIGHT, false, color, position);                
            }

            var highs = pivotPointsIndicator.GetHighs(security.BarNumber);

            foreach (var high in highs)
            {
                x = security.GetBarDateTime(high.BarNumber);
                //y = high.Value + 50;
                y = high.Value + 50;
                position = new MarketPoint(x, y);
                var id = high.BarNumber.ToString() + " " + x.ToLongTimeString() + " " + high.Value.ToString();
                //Logger.Log("id: " + id.ToString());
                pane1.AddInteractivePoint(id, PaneSides.RIGHT, false, color, position);                
            }

            //pane1.AddList("EMA", ema, CandleStyles.BAR_CANDLE, color, PaneSides.RIGHT);
            pane1.AddList("EMA", ema, color, PaneSides.RIGHT);
        }
    }
}