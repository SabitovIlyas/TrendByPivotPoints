using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.GraphPane;
using TSLab.Script.Helpers;
using TSLab.Script.Optimization;
using TSLab.Script.Realtime;

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
                
        Indicator lastLowForOpenLongPosition = null;

        Indicator lastLowCaseLongClose = null;
        double lastPriceOpenLongPosition = 0;
        double breakdownLong = 0;

        double lastHighForOpenShortPosition = 0;
        double lastHighCaseShortClose = 0;
        double lastPriceOpenShortPosition = 0;
        double breakdownShort = 0;

        string messageForLog = "";


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
            try
            {
                security.BarNumber = barNumber;
                if (!flagToDebugLog)
                {
                    messageForLog = string.Format("ГО на покупку: {0}; ГО на продажу: {1}; Шаг цены: {2}", security.BuyDeposit, security.SellDeposit, security.StepPrice);
                    Logger.Log(messageForLog);
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

            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }           
        }
        
        public void CheckPositionOpenLongCase(double lastPrice, int barNumber)
        {
            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber);

            var lows = pivotPointsIndicator.GetLows(barNumber);
            //Logger.Log("lows.Count = " + lows.Count.ToString());
            if (lows.Count < 2)
                return;

            var lastLow = lows[lows.Count - 1];
            var prevLastLow = lows[lows.Count - 2];

            var lowsValues = new List<double>();
            foreach (var low in lows)
                lowsValues.Add(low.Value);                        
            
            breakdownLong = (pivotPointBreakDownSide / 100) * atr[barNumber];

            //var highs = pivotPointsIndicator.GetHighs(barNumber);

            //var lastHighValue = double.MaxValue;

            //if (highs.Count != 0)
            //{
            //    var lastHigh = highs.Last();
            //    lastHighValue = lastHigh.Value;
            //}

            messageForLog = string.Format("Инструмент: {0}; номер бара (б. №) {1}. Открыта ли длинная позиция?", security.Name, barNumber);
            Logger.Log(messageForLog);

            if (le == null)
            {                
                Logger.Log("Длинная позиция не открыта. Выполняется ли условие двух последовательных повышающихся минимумов?");

                if (lastLowForOpenLongPosition != null)
                    lastLowCaseLongClose = lastLowForOpenLongPosition;
                lastLowForOpenLongPosition = null;

                if (patternPivotPoints_1g2.Check(lowsValues))   //1
                {
                    messageForLog = string.Format("Да, выполняется: последний минимум б. №{0}: {1} выше предыдущего б. №{2}: {3}. " +
                        "Использовался ли последний минимум в попытке открыть длинную позицию ранее?",
                        barNumber, lastLow.BarNumber, lastLow.Value, prevLastLow.BarNumber, prevLastLow.Value);
                    Logger.Log(messageForLog);

                    if (lastLow.BarNumber != lastLowCaseLongClose.BarNumber)    //2
                    {
                        messageForLog = string.Format("Нет, не использовался. Последний минимум, который использовался в попытке " +
                            "открыть длинную позицию ранее -- б. №{0}: {1}. Запоминаем попытку открытия длинной позиции. " +
                            "Не отсеивается ли потенциальная сделка фильтром EMA?", lastLowCaseLongClose.BarNumber, lastLowCaseLongClose.Value);
                        Logger.Log(messageForLog);

                        lastLowForOpenLongPosition = lastLow;

                        if (lastPrice > ema[barNumber]) //3
                        {
                            messageForLog = string.Format("Нет, потенциальная сделка не отсеивается фильтром. Последняя цена закрытия {0} выше EMA: {1}. " +
                                "Вычисляем стоп-цену...", lastPrice, ema[barNumber]);
                            Logger.Log(messageForLog);

                            var stopPrice = lastLow.Value - breakdownLong;

                            messageForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                                "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Последняя цена выше стоп-цены?", atr[barNumber], pivotPointBreakDownSide,
                                breakdownLong, lastLow.Value, stopPrice);
                            Logger.Log(messageForLog);

                            if (lastPrice > stopPrice)  //4
                            {
                                messageForLog = string.Format("Да, последняя цена ({0}) выше стоп-цены ({1}). Открываем длинную позицию...", lastPrice, stopPrice);
                                Logger.Log(messageForLog);
                                                                
                                Logger.Log("Определяем количество контрактов...");

                                var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Long);

                                Logger.Log("Торгуем в лаборатории или в режиме реального времени?");
                                var s = sec as ISecurityRt;
                                if (s != null)
                                {
                                    contracts = 1;
                                    messageForLog = string.Format("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                                    Logger.Log(messageForLog);                                    
                                }
                                else
                                {
                                    Logger.Log("Торгуем в лаборатории.");
                                }

                                sec.Positions.BuyAtMarket(barNumber + 1, contracts, "LE");
                                lastPriceOpenLongPosition = lastPrice;
                                stopLossLong = 0;
                                Logger.Log("Открываем длинную позицию! Отправляем ордер.");
                            }
                            else
                            {                                
                                Logger.Log("Последняя цена ниже стоп-цены. Длинную позицию не открываем.");
                            }
                        }
                        else
                        {
                            messageForLog = string.Format("Cделка отсеивается фильтром, так как последняя цена закрытия {0} ниже или совпадает с EMA: {1}.", lastPrice, ema[barNumber]);
                            Logger.Log(messageForLog);
                        }
                    }
                    else
                    {
                        messageForLog = string.Format("Да, последний минимум использовался в попытке открыть длинную позицию ранее.");
                        Logger.Log(messageForLog);
                    }                    
                }
                else
                {                    
                    Logger.Log("Нет, не выполняется");
                }                
            }
            else
            {
                Logger.Log("Длинная позиция открыта. Проверяем условия наращивания позиции: выполняется ли условие двух последовательных повышающихся минимумов?");

                if (patternPivotPoints_1g2.Check(lowsValues))   //1
                {
                    messageForLog = string.Format("Да, выполняется: последний минимум б. №{0}: {1} выше предыдущего б. №{2}: {3}. " +
                        "Использовался ли последний минимум в попытке открыть длинную позицию ранее?",
                        barNumber, lastLow.BarNumber, lastLow.Value, prevLastLow.BarNumber, prevLastLow.Value);
                    Logger.Log(messageForLog);

                    if (lastLow.BarNumber != lastLowForOpenLongPosition.BarNumber)    //2
                    {
                        messageForLog = string.Format("Нет, не использовался. Последний минимум, который использовался в попытке " +
                            "открыть длинную позицию ранее -- б. №{0}: {1}. Запоминаем попытку открытия длинной позиции. " +
                            "Не отсеивается ли потенциальная сделка фильтром EMA?", lastLowCaseLongClose.BarNumber, lastLowCaseLongClose.Value);
                        Logger.Log(messageForLog);

                        lastLowForOpenLongPosition = lastLow;

                        if (lastPrice > ema[barNumber]) //3
                        {
                            messageForLog = string.Format("Нет, потенциальная сделка не отсеивается фильтром. Последняя цена закрытия {0} выше EMA: {1}. " +
                                "Вычисляем стоп-цену...", lastPrice, ema[barNumber]);
                            Logger.Log(messageForLog);

                            var stopPrice = lastLow.Value - breakdownLong;

                            messageForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                                "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Последняя цена выше стоп-цены?", atr[barNumber], pivotPointBreakDownSide,
                                breakdownLong, lastLow.Value, stopPrice);
                            Logger.Log(messageForLog);

                            if (lastPrice > stopPrice)  //4
                            {
                                messageForLog = string.Format("Да, последняя цена ({0}) выше стоп-цены ({1}). Открываем длинную позицию...", lastPrice, stopPrice);
                                Logger.Log(messageForLog);

                                Logger.Log("Определяем количество контрактов...");

                                var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Long);

                                Logger.Log("Торгуем в лаборатории или в режиме реального времени?");
                                var s = sec as ISecurityRt;
                                if (s != null)
                                {
                                    contracts = 1;
                                    messageForLog = string.Format("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                                    Logger.Log(messageForLog);
                                }
                                else
                                {
                                    Logger.Log("Торгуем в лаборатории.");
                                }                                

                                Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие наращивания позиции лонг выполнено!");                                
                                var shares = le.Shares + contracts;
                                le.ChangeAtMarket(barNumber + 1, shares, "LE");
                                lastPriceOpenLongPosition = lastPrice;
                                Logger.Log("Наращиваем длинную позицию! Отправляем ордер.");
                            }
                            else
                            {
                                Logger.Log("Последняя цена ниже стоп-цены. Длинную позицию не открываем.");
                            }
                        }
                    }
                    else
                    {
                        messageForLog = string.Format("Да, последний минимум использовался в попытке открыть длинную позицию ранее.");
                        Logger.Log(messageForLog);
                    }
                }
                else
                {
                    Logger.Log("Нет, не выполняется");
                }
                                
                Logger.Log("Обновляем стоп...");


                if (lows.Count == 0)
                {
                    Logger.Log("Это условие никогда не должно отрабатывать. Проверить и впоследствии убрать.");
                    return;
                }

                var stopLoss = lastLow.Value - breakdownLong;

                messageForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                                "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Новый стоп-лосс выше прежнего?", atr[barNumber], pivotPointBreakDownSide,
                                breakdownLong, lastLow.Value, stopLoss);
                Logger.Log(messageForLog);

                if (stopLoss > stopLossLong)
                {                    
                    messageForLog = string.Format("Да, новый стоп-лосс ({0}) выше прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossLong);
                    
                    Logger.Log(messageForLog);
                    stopLossLong = stopLoss;
                }
                else
                {
                    messageForLog = string.Format("Нет, новый стоп-лосс ({0}) не выше прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossLong);
                    Logger.Log(messageForLog);
                }

                le.CloseAtStop(barNumber + 1, stopLossLong, 100, "LXS");
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

            breakdownShort = (pivotPointBreakDownSide / 100) * atr[barNumber];

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
                    var stopPrice = highLast.Value + breakdownShort;
                    if (lastPrice < stopPrice)
                    {                        
                        var contracts = 1;
                        var s = sec as ISecurityRt;
                        if (s == null)
                            contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Short);

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
                        var stopPrice = highLast.Value + breakdownShort;
                        if (lastPrice < stopPrice)
                        {
                            Logger.Log("Номер бара = " + barNumber.ToString() + "; Условие наращивания позиции шорт выполнено!");
                            var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Short);                            
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
                    var stopLoss = high.Value + breakdownShort;
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
            switch (positionSide)
            {
                case PositionSide.LongAndShort:
                    {
                        pivotPointsIndicator.CalculateLows(security, (int)leftLocalSide, (int)rightLocalSide);
                        pivotPointsIndicator.CalculateHighs(security, (int)leftLocalSide, (int)rightLocalSide);
                        break;
                    }
                case PositionSide.Long:
                    {
                        pivotPointsIndicator.CalculateLows(security, (int)leftLocalSide, (int)rightLocalSide);
                        //pivotPointsIndicator.CalculateLows(security, 10, 10);
                        //pivotPointsIndicator.CalculateHighs(security, 10, 10);
                        break;
                    }
                case PositionSide.Short:
                    {
                        pivotPointsIndicator.CalculateHighs(security, (int)leftLocalSide, (int)rightLocalSide);                        
                        break;
                    }

            }            
            ema = Series.EMA(sec.ClosePrices, (int)EmaPeriodSide);
            //ema = Series.EMA(sec.ClosePrices, 50);
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