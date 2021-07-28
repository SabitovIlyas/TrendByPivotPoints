using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.GraphPane;
using TSLab.Script.Helpers;
using TSLab.Script.Optimization;

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
        
        double stopLossLong;
        double stopLossShort;

        public Logger Logger { get; set; } = new NullLogger();
        bool flagToDebugLog = false;
        
        double lastLowForOpenLongPosition = 0;
        double lastLowCaseLongClose = 0;
        double lastPriceOpenLongPosition = 0;

        double lastHighForOpenShortPosition = 0;
        double lastHighCaseShortClose = 0;
        double lastPriceOpenShortPosition = 0;
        PositionSide positionSide;

        public TradingSystemPivotPointsEMA(LocalMoneyManager localMoneyManager, Account account, Security security, PositionSide positionSide)
        {   
            this.localMoneyManager = localMoneyManager;            
            var securityTSLab = security as SecurityTSlab;
            sec = securityTSLab.security;
            this.security = security;
            pivotPointsIndicator = new PivotPointsIndicator();            
            patternPivotPoints_1g2 = new PatternPivotPoints_1g2();
            patternPivotPoints_1l2 = new PatternPivotPoints_1l2();
            this.positionSide = positionSide;
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

            var lastBar = security.LastBar;
            var lastPrice = lastBar.Close;

            switch (positionSide)
            {
                case PositionSide.LongAndShort:
                    {
                        CheckPositionOpenLongCase(lastPrice, barNumber);
                        CheckPositionOpenShortCase(lastPrice, barNumber);
                        break;
                    }
                case PositionSide.Long:
                    {
                        CheckPositionOpenLongCase(lastPrice, barNumber);
                        break;
                    }
                case PositionSide.Short:
                    {
                        CheckPositionOpenShortCase(lastPrice, barNumber);
                        break;
                    }

            }
        }
        
        public void CheckPositionOpenLongCase(double lastPrice, int barNumber)
        {
            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber);

            var lows = pivotPointsIndicator.GetLows(barNumber);
            //Logger.Log("lows.Count = " + lows.Count.ToString());

            var lowsValues = new List<double>();
            foreach (var low in lows)
                lowsValues.Add(low.Value);            

            var lastLowValue = 0d;
            if (lows.Count != 0)
                lastLowValue = lowsValues.Last();

            if (le == null)
            {
                if (lastLowForOpenLongPosition != 0)
                    lastLowCaseLongClose = lastLowForOpenLongPosition;
                lastLowForOpenLongPosition = 0;
                if (patternPivotPoints_1g2.Check(lowsValues) && (lastPrice > ema[barNumber]) && (lastLowValue != lastLowCaseLongClose))
                {
                    lastLowForOpenLongPosition = lastLowValue;
                    Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие входа в лонг выполнено!");
                    var lowLast = lows.Last();
                    var stopPrice = lowLast.Value - pivotPointBreakDownSide * atr[barNumber];
                    if (lastPrice > stopPrice)
                    {
                        var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Long);
                        //var contracts = 1;
                        sec.Positions.BuyAtMarket(barNumber + 1, contracts, "LE");//174: 78583
                        lastPriceOpenLongPosition = lastPrice;
                        stopLossLong = 0;
                    }
                }
            }
            else
            {
                if (patternPivotPoints_1g2.Check(lowsValues) && (lastPrice > ema[barNumber]) && (lastLowForOpenLongPosition != lastLowValue))
                {
                    lastLowForOpenLongPosition = lastLowValue; //минимум для открытия (в том числе и потенциального)
                    if (lastPrice > lastPriceOpenLongPosition)
                    {
                        var lowLast = lows.Last();
                        var stopPrice = lowLast.Value - pivotPointBreakDownSide * atr[barNumber];
                        if (lastPrice > stopPrice)
                        {
                            Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие наращивания позиции лонг выполнено!");
                            var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Long);
                            var shares = le.Shares + contracts;
                            le.ChangeAtMarket(barNumber + 1, shares, "LE");
                            lastPriceOpenLongPosition = lastPrice;
                        }
                    }
                }

                else
                {
                    if (lows.Count == 0)
                        return;

                    var low = lows.Last();
                    var stopLoss = low.Value - pivotPointBreakDownSide * atr[barNumber];
                    if (stopLoss > stopLossLong) stopLossLong = stopLoss;
                    le.CloseAtStop(barNumber + 1, stopLossLong, 100, "LXS");
                }
            }
        }

        public void CheckPositionOpenShortCase(double lastPrice, int barNumber)
        {
            var se = sec.Positions.GetLastActiveForSignal("SE", barNumber);

            var highs = pivotPointsIndicator.GetHighs(barNumber);
            var highsValues = new List<double>();
            foreach (var high in highs)
                highsValues.Add(high.Value);

            var lastHighValue = 0d;
            if (highs.Count != 0)
                lastHighValue = highsValues.Last();

            if (se == null)
            {
                if (lastHighForOpenShortPosition != 0)
                    lastHighCaseShortClose = lastHighForOpenShortPosition;
                lastHighForOpenShortPosition = 0;
                if (patternPivotPoints_1l2.Check(highsValues) && (lastPrice < ema[barNumber]) && (lastHighValue != lastHighCaseShortClose))
                {
                    lastHighForOpenShortPosition = lastHighValue;
                    Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие входа в шорт выполнено!");
                    var highLast = highs.Last();
                    var stopPrice = highLast.Value + pivotPointBreakDownSide * atr[barNumber];
                    if (lastPrice < stopPrice)
                    {
                        Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие наращивания позиции шорт выполнено!");
                        var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Short);
                        //var contracts = 1;
                        sec.Positions.SellAtMarket(barNumber + 1, contracts, "SE");
                        lastPriceOpenShortPosition = lastPrice;
                        stopLossShort = double.MaxValue;
                    }
                }
            }
            else
            {
                if (patternPivotPoints_1l2.Check(highsValues) && (lastPrice < ema[barNumber]) && (lastHighForOpenShortPosition != lastHighValue))
                {
                    lastHighForOpenShortPosition = lastHighValue; //максимум для открытия (в том числе и потенциального)
                    if (lastPrice < lastPriceOpenShortPosition)
                    {
                        var highLast = highs.Last();
                        var stopPrice = highLast.Value + pivotPointBreakDownSide * atr[barNumber];
                        if (lastPrice < stopPrice)
                        {
                            var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Short);
                            //var contracts = 1;
                            var shares = se.Shares + contracts;
                            se.ChangeAtMarket(barNumber + 1, -shares, "SE");
                            lastPriceOpenShortPosition = lastPrice;
                        }
                    }
                }

                else
                {
                    if (highs.Count == 0)
                        return;

                    var high = highs.Last();
                    var stopLoss = high.Value + pivotPointBreakDownSide *  atr[barNumber];
                    if (stopLoss < stopLossShort) stopLossShort = stopLoss;
                    se.CloseAtStop(barNumber + 1, stopLossShort, 100, "SXS");
                }
            }
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

        public void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide)
        {
            this.leftLocalSide = leftLocalSide;
            this.rightLocalSide = rightLocalSide;
            this.pivotPointBreakDownSide = pivotPointBreakDownSide;
            this.EmaPeriodSide = EmaPeriodSide;
        }

        private double leftLocalSide;
        private double rightLocalSide;
        private double pivotPointBreakDownSide;
        private double EmaPeriodSide;

        public void CalculateIndicators()
        {
            pivotPointsIndicator.CalculateLows(security, (int)leftLocalSide, (int)rightLocalSide);
            pivotPointsIndicator.CalculateHighs(security, (int)leftLocalSide, (int)rightLocalSide);
            ema = Series.EMA(sec.ClosePrices, (int)EmaPeriodSide);
            atr = Series.AverageTrueRange(sec.Bars, 20);                
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

            //var pane2 = context.CreateGraphPane(sec.ToString(), "Equity");            
        }
    }
}