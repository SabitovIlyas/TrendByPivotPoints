using System;
using System.Collections.Generic;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.Helpers;
using TSLab.Script.Handlers;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class TradingSystemPivotPointsEmaRtUpdate : ITradingSystemPivotPointsEMA
    {
        public IContext ctx { get; set; }
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
        double breakdownLong = 0;

        Indicator lastHighForOpenShortPosition = null;
        double breakdownShort = 0;

        string textForLog = "";

        PositionSide positionSide;

        private double leftLocalSide;
        private double rightLocalSide;
        private double pivotPointBreakDownSide;
        private double EmaPeriodSide;
        private Account account;
        private int barNumber;
        private string signalNameForOpenPosition = "";
        private double lastPrice;

        public PositionSide PositionSide { get { return positionSide; } }

        private string tradingSystemDescription;
        private string name = "TradingSystemPivotPointsEMAtest";
        private string parametersCombination;

        public TradingSystemPivotPointsEmaRtUpdate(LocalMoneyManager localMoneyManager, Account account, Security security, PositionSide positionSide)
        {
            this.localMoneyManager = localMoneyManager;
            this.account = account;
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
                this.barNumber = barNumber;
                security.BarNumber = barNumber;
                if (!flagToDebugLog)
                {
                    Log("ГО на покупку: {0}; ГО на продажу: {1}; Шаг цены: {2}", security.BuyDeposit, security.SellDeposit, security.StepPrice);
                    flagToDebugLog = true;
                }

                var lastBar = security.LastBar;
                lastPrice = lastBar.Close;

                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                    Logger.SwitchOn();
                else
                    Logger.SwitchOff();

                switch (positionSide)
                {
                    case PositionSide.Long:
                        {
                            signalNameForOpenPosition = "LE";                            
                            convertable = new Converter(isConverted: false);
                            GetLows();
                            CheckPositionOpenLongCase();
                            break;
                        }
                    case PositionSide.Short:
                        {
                            signalNameForOpenPosition = "SE";
                            convertable = new Converter(isConverted: true);
                            CheckPositionOpenShortCase();
                            break;
                        }
                }
            }

            catch (Exception e)
            {
                Log("Исключение в методе Update(): " + e.ToString());
            }
        }

        private Indicator lastLow;
        private Indicator prevLastLow;
        private List<Indicator> lows;
        private Converter convertable;

        private void GetLows()
        {
            lows = pivotPointsIndicator.GetLows(barNumber, convertable.IsConverted);
            if (lows.Count < 2)
                throw new Exception("Количество экстремумов меньше двух");

            lastLow = lows[lows.Count - 1];
            prevLastLow = lows[lows.Count - 2];
        }

        private bool IsPositionOpen()
        {
            var position = sec.Positions.GetLastActiveForSignal(signalNameForOpenPosition, barNumber);
            return position != null;
        }

        private bool IsLastMinGreaterThanPrevious()
        {
            var lowsValues = new List<double>();
            foreach (var low in lows)
                lowsValues.Add(low.Value);

            return patternPivotPoints_1g2.Check(lowsValues, convertable.IsConverted);
        }

        private bool IsLastPriceGreaterEma()
        {            
            return convertable.IsGreater(lastPrice, ema[barNumber]);
        }

        private bool IsLastPriceGreaterStopPrice()
        {            
            return false;// convertable.IsGreater(lastPrice, stopPrice);
        }

        private void Log(string text)
        {
            Logger.Log("{0}: {1}", tradingSystemDescription, text);
        }

        private void Log(string text, params object[] args)
        {
            text = string.Format(text, args);
            Log(text);
        }    
        
        private void dd()
        {

        }

        private void SetLastLowForOpenLongPosition()
        {
            lastLowForOpenLongPosition = lastLow;
        }

        public void CheckPositionOpenLongCase()
        {
            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber); //убрать в перспективе

            breakdownLong = (pivotPointBreakDownSide / 100) * atr[barNumber]; //убрать в перспективе

            Log("бар № {0}. Открыта ли длинная позиция?", barNumber);
            if (!IsPositionOpen())
            {
                Log("Длинная позиция не открыта.");

                Log("Выполняется ли условие двух последовательных повышающихся минимумов?");
                if (IsLastMinGreaterThanPrevious())   //1
                {
                    Log("Да, выполняется: последний минимум б. №{0}: {1} выше предыдущего б. №{2}: {3}.", lastLow.BarNumber, lastLow.Value, prevLastLow.BarNumber, prevLastLow.Value);

                    Log("Использовался ли последний минимум в попытке открыть длинную позицию ранее?");
                    var isLastLowCaseLongCloseNotExist = lastLowForOpenLongPosition == null;

                    if (isLastLowCaseLongCloseNotExist || (lastLow.BarNumber != lastLowForOpenLongPosition.BarNumber))    //2
                    {
                        if (isLastLowCaseLongCloseNotExist)
                            Log("Последняя попытка открыть длинную позицию не обнаружена.");

                        else
                            Log("Нет, не использовался. Последний минимум, который использовался в попытке открыть длинную позицию ранее -- б. №{0}: {1}.",
                                lastLowForOpenLongPosition.BarNumber, lastLowForOpenLongPosition.Value);

                        Log("Не отсеивается ли потенциальная сделка фильтром EMA?");
                        if (IsLastPriceGreaterEma()) //3
                        {
                            Log("Нет, потенциальная сделка не отсеивается фильтром. Последняя цена закрытия {0} выше EMA: {1}. ", lastPrice, ema[barNumber]);
                            
                            Log("Вычисляем стоп-цену...");
                            var stopPrice = lastLow.Value - breakdownLong;

                            Log("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                                "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Последняя цена выше стоп-цены?", atr[barNumber], pivotPointBreakDownSide,
                                breakdownLong, lastLow.Value, stopPrice);

                            Log("Запоминаем минимум, использовавшийся для попытки открытия длинной позиции.");
                            SetLastLowForOpenLongPosition();                            

                            if (lastPrice > stopPrice)  //4
                            {
                                Log("Да, последняя цена ({0}) выше стоп-цены ({1}). Открываем длинную позицию...", lastPrice, stopPrice);

                                Log("Определяем количество контрактов...");

                                var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Long);

                                Log("Торгуем в лаборатории или в режиме реального времени?");
                                if (security.IsRealTimeTrading)
                                {
                                    contracts = 1;
                                    Log("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);

                                    var price = lastPrice + atr[barNumber];
                                    sec.Positions.BuyAtPrice(barNumber + 1, contracts, price, "LE");
                                }
                                else
                                {
                                    Log("Торгуем в лаборатории.");
                                    sec.Positions.BuyAtMarket(barNumber + 1, contracts, "LE");
                                }

                                stopLossLong = stopPrice;

                                Log("Проверяем актуальный ли это бар.");
                                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                                {
                                    Log("Бар актуальный.");
                                    CreateStopLossLong();
                                    SetFlagNewPositionOpened();
                                }
                                else
                                    Log("Бар не актуальный.");

                                Log("Открываем длинную позицию! Отправляем ордер.");
                            }
                            else
                                Log("Последняя цена ниже стоп-цены. Длинную позицию не открываем.");

                        }
                        else
                            Log("Cделка отсеивается фильтром, так как последняя цена закрытия {0} ниже или совпадает с EMA: {1}.", lastPrice, ema[barNumber]);
                    }
                    else
                        Log("Да, последний минимум использовался в попытке открыть длинную позицию ранее.");
                }
                else
                    Log("Нет, не выполняется: последний минимум б. №{0}: {1} не выше предыдущего б. №{2}: {3}.",
                        lastLow.BarNumber, lastLow.Value, prevLastLow.BarNumber, prevLastLow.Value);
            }
            else
            {
                Log("Длинная позиция открыта.");
                UpdateStopLossLongPosition(barNumber, lows, lastLow, le);
                CheckLongPositionCloseCase(le, barNumber);
            }
        }

        private void CreateStopLossLong()
        {
            var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
            Logger.Log(string.Format("Сохраним stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));
            var container = new NotClearableContainer<double>(stopLossLong);

            ctx.StoreObject(containerName, container);
            Logger.Log(string.Format("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));

            container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
            double value = 0d;
            if (container != null)
                value = container.Content;

            if (value != 0d)
                if (value == stopLossLong)
                    Logger.Log(string.Format("stopLossLong сохранился в контейнере. Значение в контейнере: value = {0}.", value));

                else
                    Logger.Log(string.Format("stopLossLong НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value));
        }

        private void CreateStopLossShort()
        {
            var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
            Logger.Log(string.Format("Сохраним stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));
            var container = new NotClearableContainer<double>(stopLossShort);

            ctx.StoreObject(containerName, container);
            Logger.Log(string.Format("Проверим, сохранился ли stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));

            container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
            double value = 0d;
            if (container != null)
                value = container.Content;

            if (value != 0d)
                if (value == stopLossLong)
                    Logger.Log(string.Format("stopLossShort сохранился в контейнере. Значение в контейнере: value = {0}.", value));

                else
                    Logger.Log(string.Format("stopLossShort НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value));
        }

        private void UpdateStopLossLongPosition(int barNumber, List<Indicator> lows, Indicator lastLow, IPosition le)
        {
            Logger.Log("Обновляем стоп...");

            if (lows.Count == 0)
            {
                Logger.Log("Это условие никогда не должно отрабатывать. Проверить и впоследствии убрать.");
                return;
            }

            var stopLoss = lastLow.Value - breakdownLong;

            textForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                            "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Новый стоп-лосс выше прежнего?", atr[barNumber], pivotPointBreakDownSide,
                            breakdownLong, lastLow.Value, stopLoss);
            Logger.Log(textForLog);

            Logger.Log("Проверяем актуальный ли это бар.");
            if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
            {
                Logger.Log("Бар актуальный.");

                var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                Logger.Log(string.Format("Загружаем stopLossLong из контейнера \"{0}\".", containerName));
                var container = ctx.LoadObject(containerName) as NotClearableContainer<double>;

                if (container != null)
                {
                    Logger.Log("Загрузили контейнер.");
                    var value = container.Content;
                    Logger.Log(string.Format("Значение в контейнере: value = {0}", value));
                    if (value != 0d)
                    {
                        Logger.Log("Записываем в stopLossLong ненулевое значение из контейнера.");
                        stopLossLong = value;
                    }
                    else
                    {
                        Logger.Log("Значение в контейнере равно нулю! Значение из контейнера отбрасываем!");
                    }
                }
                else
                {
                    Logger.Log("Не удалось загрузить контейнер.");
                }
            }
            else
            {
                Logger.Log("Бар не актуальный.");
            }

            if (stopLoss > stopLossLong)
            {
                textForLog = string.Format("Да, новый стоп-лосс ({0}) выше прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossLong);

                Logger.Log(textForLog);
                stopLossLong = stopLoss;

                Logger.Log("Проверяем актуальный ли это бар.");
                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                {
                    Logger.Log("Бар актуальный.");

                    var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                    Logger.Log(string.Format("Сохраним stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));
                    var container = new NotClearableContainer<double>(stopLossLong);

                    ctx.StoreObject(containerName, container);
                    Logger.Log(string.Format("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));

                    container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
                    double value = 0d;
                    if (container != null)
                        value = container.Content;

                    if (value != 0d)
                        if (value == stopLossLong)
                            Logger.Log(string.Format("stopLossLong сохранился в контейнере. Значение в контейнере: value = {0}.", value));

                        else
                            Logger.Log(string.Format("stopLossLong НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value));
                }
                else
                {
                    Logger.Log("Бар не актуальный.");
                }

            }
            else
            {
                textForLog = string.Format("Нет, новый стоп-лосс ({0}) не выше прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossLong);
                Logger.Log(textForLog);
            }

            if (WasNewPositionOpened())
            {
                Logger.Log("Открыта новая позиция. Устанавливаем стоп-лосс на текущем баре.");
                le.CloseAtStop(barNumber, stopLossLong, atr[barNumber], "LXS");
            }
            else
            {
                Logger.Log("Новая позиция не открыта. Устанавливаем стоп-лосс на следующем баре.");
                le.CloseAtStop(barNumber + 1, stopLossLong, atr[barNumber], "LXS");
            }
        }

        public bool CheckLongPositionCloseCase(IPosition le, int barNumber)
        {
            security.BarNumber = barNumber;
            var bar = security.LastBar;

            if (le != null)
            {
                Logger.Log("Проверяем пробил ли вниз минимум последнего бара стоп-лосс для лонга?");
                if (bar.Low <= stopLossLong)
                {
                    if (WasNewPositionOpened())
                    {
                        Logger.Log("Открыта новая позиция. Устанавливаем стоп-лосс на текущем баре.");
                        le.CloseAtMarket(barNumber, "LXE");
                    }
                    else
                    {
                        Logger.Log("Новая позиция не открыта. Устанавливаем стоп-лосс на следующем баре.");
                        le.CloseAtMarket(barNumber + 1, "LXE");
                    }

                    var message = string.Format("Да, минимум последнего бара {0} пробил вниз стоп-лосс для лонга {1}. Закрываем позицию по рынку на следующем баре.", bar.Low, stopLossLong);
                    Logger.Log(message);

                    return true;
                }
                else
                {
                    var message = string.Format("Нет, минимум последнего бара {0} выше стоп-лосса для лонга {1}. Оставляем позицию.", bar.Low, stopLossLong);
                    Logger.Log(message);
                }
            }
            return false;
        }

        public void CheckPositionOpenShortCase()
        {
            var se = sec.Positions.GetLastActiveForSignal("SE", barNumber);
            var highs = pivotPointsIndicator.GetHighs(barNumber);

            if (highs.Count < 2)
                return;

            var lastHigh = highs[highs.Count - 1];
            var prevLastHigh = highs[highs.Count - 2];

            var highsValues = new List<double>();
            foreach (var high in highs)
                highsValues.Add(high.Value);

            breakdownShort = (pivotPointBreakDownSide / 100) * atr[barNumber];

            textForLog = string.Format("Инструмент: {0}; номер бара (б. №) {1}. Открыта ли короткая позиция?", security.Name, barNumber);
            Logger.Log(textForLog);

            if (se == null)
            {
                Logger.Log("Короткая позиция не открыта. Выполняется ли условие двух последовательных понижающихся максимумов?");

                if (patternPivotPoints_1l2.Check(highsValues))   //1
                {
                    textForLog = string.Format("Да, выполняется: последний максимум б. №{0}: {1} ниже предыдущего б. №{2}: {3}. " +
                        "Использовался ли последний максимум в попытке открыть короткую позицию ранее?",
                        lastHigh.BarNumber, lastHigh.Value, prevLastHigh.BarNumber, prevLastHigh.Value);
                    Logger.Log(textForLog);

                    var isLastHighCaseShortCloseNotExist = lastHighForOpenShortPosition == null;

                    if (isLastHighCaseShortCloseNotExist || (lastHigh.BarNumber != lastHighForOpenShortPosition.BarNumber))    //2
                    {
                        if (isLastHighCaseShortCloseNotExist)
                            textForLog = "Последняя попытка открыть короткую позицию не обнаружена. Не отсеивается ли потенциальная сделка фильтром EMA?";

                        else
                            textForLog = string.Format("Нет, не использовался. Последний максимум, который использовался в попытке " +
                            "открыть короткую позицию ранее -- б. №{0}: {1}. Не отсеивается ли потенциальная сделка фильтром EMA?",
                            lastHighForOpenShortPosition.BarNumber, lastHighForOpenShortPosition.Value);

                        Logger.Log(textForLog);

                        if (lastPrice < ema[barNumber]) //3
                        {
                            textForLog = string.Format("Нет, потенциальная сделка не отсеивается фильтром. Последняя цена закрытия {0} ниже EMA: {1}.", lastPrice, ema[barNumber]);
                            Logger.Log(textForLog);

                            Logger.Log("Проверяем актуально ли открытие короткой позиции?");

                            if (true || IsOpenShortPositionCaseActual(lastHigh, barNumber))//заглушил
                            {
                                Logger.Log("Открытие короткой позции актуально?");

                                Logger.Log("Вычисляем стоп-цену...");
                                var stopPrice = lastHigh.Value + breakdownShort;

                                textForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                                    "стоп-лосс = последний максимум {3} + допустимый уровень пробоя {2} = {4}. Последняя цена ниже стоп-цены?", atr[barNumber], pivotPointBreakDownSide,
                                    breakdownShort, lastHigh.Value, stopPrice);
                                Logger.Log(textForLog);

                                textForLog = string.Format("Запоминаем максимум, использовавшийся для попытки открытия короткой позиции.");
                                Logger.Log(textForLog);

                                lastHighForOpenShortPosition = lastHigh;
                                
                                if (lastPrice < stopPrice)  //4
                                {
                                    textForLog = string.Format("Да, последняя цена ({0}) ниже стоп-цены ({1}). Открываем короткую позицию...", lastPrice, stopPrice);
                                    Logger.Log(textForLog);

                                    Logger.Log("Определяем количество контрактов...");

                                    var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Short);

                                    Logger.Log("Торгуем в лаборатории или в режиме реального времени?");
                                    if (security.IsRealTimeTrading)
                                    {
                                        contracts = 1;
                                        textForLog = string.Format("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                                        Logger.Log(textForLog);

                                        var price = lastPrice - atr[barNumber];
                                        sec.Positions.SellAtPrice(barNumber + 1, contracts, price, "SE");
                                    }
                                    else
                                    {
                                        Logger.Log("Торгуем в лаборатории.");
                                        sec.Positions.SellAtMarket(barNumber + 1, contracts, "SE");
                                    }

                                    stopLossShort = stopPrice;

                                    Logger.Log("Проверяем актуальный ли это бар.");
                                    if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                                    {
                                        Logger.Log("Бар актуальный.");
                                        CreateStopLossShort();
                                        SetFlagNewPositionOpened();
                                    }
                                    else
                                    {
                                        Logger.Log("Бар не актуальный.");
                                    }

                                    Logger.Log("Открываем короткую позицию! Отправляем ордер.");
                                }
                                else
                                {
                                    Logger.Log("Последняя цена выше стоп-цены. Короткую позицию не открываем.");
                                }
                            }
                            else
                            {
                                Logger.Log("Открытие короткой позиции не актуально.");
                            }
                        }
                        else
                        {
                            textForLog = string.Format("Cделка отсеивается фильтром, так как последняя цена закрытия {0} выше или совпадает с EMA: {1}.", lastPrice, ema[barNumber]);
                            Logger.Log(textForLog);
                        }
                    }
                    else
                    {
                        textForLog = string.Format("Да, последний максимум использовался в попытке открыть короткую позицию ранее.");
                        Logger.Log(textForLog);
                    }
                }
                else
                {
                    textForLog = string.Format("Нет, не выполняется: последний максимум б. №{0}: {1} не ниже предыдущего б. №{2}: {3}.",
                        lastHigh.BarNumber, lastHigh.Value, prevLastHigh.BarNumber, prevLastHigh.Value);
                    Logger.Log(textForLog);
                }
            }
            else
            {
                Logger.Log("Короткая позиция открыта.");
                UpdateStopLossShortPosition(barNumber, highs, lastHigh, se);
                CheckShortPositionCloseCase(se, barNumber);
            }
        }
        
        private bool IsOpenShortPositionCaseActual(Indicator lastHigh, int barNumber)
        {
            var previousBarNumber = security.RealTimeActualBarNumber - 1;
            return (barNumber == lastHigh.BarNumber + rightLocalSide) || (security.GetBarClose(previousBarNumber) > ema[previousBarNumber]);
        }

        private void UpdateStopLossShortPosition(int barNumber, List<Indicator> highs, Indicator lastHigh, IPosition se)
        {
            Logger.Log("Обновляем стоп...");

            if (highs.Count == 0)
            {
                Logger.Log("Это условие никогда не должно отрабатывать. Проверить и впоследствии убрать.");
                return;
            }

            var stopLoss = lastHigh.Value + breakdownShort;

            textForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                            "стоп-лосс = последний максимум {3} + допустимый уровень пробоя {2} = {4}. Новый стоп-лосс ниже прежнего?", atr[barNumber], pivotPointBreakDownSide,
                            breakdownShort, lastHigh.Value, stopLoss);
            Logger.Log(textForLog);

            Logger.Log("Проверяем актуальный ли это бар.");
            if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
            {
                Logger.Log("Бар актуальный.");

                var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
                Logger.Log(string.Format("Загружаем stopLossShort из контейнера \"{0}\".", containerName));
                var container = ctx.LoadObject(containerName) as NotClearableContainer<double>;

                if (container != null)
                {
                    Logger.Log("Загрузили контейнер.");
                    var value = container.Content;
                    Logger.Log(string.Format("Значение в контейнере: value = {0}", value));
                    if (value != 0d)
                    {
                        Logger.Log("Записываем в stopLossShort ненулевое значение из контейнера.");
                        stopLossLong = value;
                    }
                    else
                    {
                        Logger.Log("Значение в контейнере равно нулю! Значение из контейнера отбрасываем!");
                    }
                }
                else
                {
                    Logger.Log("Не удалось загрузить контейнер.");
                }
            }
            else
            {
                Logger.Log("Бар не актуальный.");
            }

            if (stopLoss < stopLossShort)
            {
                textForLog = string.Format("Да, новый стоп-лосс ({0}) ниже прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossShort);

                Logger.Log(textForLog);
                stopLossShort = stopLoss;

                Logger.Log("Проверяем актуальный ли это бар.");
                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                {
                    Logger.Log("Бар актуальный.");

                    var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
                    Logger.Log(string.Format("Сохраним stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));
                    var container = new NotClearableContainer<double>(stopLossShort);

                    ctx.StoreObject(containerName, container);
                    Logger.Log(string.Format("Проверим, сохранился ли stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));

                    container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
                    double value = 0d;
                    if (container != null)
                        value = container.Content;

                    if (value != 0d)
                        if (value == stopLossLong)
                            Logger.Log(string.Format("stopLossShort сохранился в контейнере. Значение в контейнере: value = {0}.", value));

                        else
                            Logger.Log(string.Format("stopLossShort НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value));
                }
                else
                {
                    Logger.Log("Бар не актуальный.");
                }
            }
            else
            {
                textForLog = string.Format("Нет, новый стоп-лосс ({0}) не ниже прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossShort);
                Logger.Log(textForLog);
            }

            if (WasNewPositionOpened())
            {
                Logger.Log("Открыта новая позиция. Устанавливаем стоп-лосс на текущем баре.");
                se.CloseAtStop(barNumber, stopLossShort, atr[barNumber], "SXS");
            }
            else
            {
                Logger.Log("Новая позиция не открыта. Устанавливаем стоп-лосс на следующем баре.");
                se.CloseAtStop(barNumber + 1, stopLossShort, atr[barNumber], "SXS");
            }
        }

        public bool CheckShortPositionCloseCase(IPosition se, int barNumber)
        {
            security.BarNumber = barNumber;
            var bar = security.LastBar;

            if (se != null)
            {
                Logger.Log("Проверяем пробил ли вверх максимум последнего бара стоп-лосс для шорта?");
                if (bar.High >= stopLossShort)
                {
                    if (WasNewPositionOpened())
                    {
                        Logger.Log("Открыта новая позиция. Устанавливаем стоп-лосс на текущем баре.");
                        se.CloseAtMarket(barNumber, "SXE");
                    }
                    else
                    {
                        Logger.Log("Новая позиция не открыта. Устанавливаем стоп-лосс на следующем баре.");
                        se.CloseAtMarket(barNumber + 1, "SXE");
                    }

                    var message = string.Format("Да, максимум последнего бара {0} пробил вверх стоп-лосс для шорта {1}. Закрываем позицию по рынку на следующем баре.", bar.High, stopLossShort);
                    Logger.Log(message);

                    return true;
                }
                else
                {
                    var message = string.Format("Нет, максимум последнего бара {0} ниже стоп-лосса для шорта {1}. Оставляем позицию.", bar.High, stopLossShort);
                    Logger.Log(message);
                }
            }
            return false;
        }

        public void CheckPositionCloseCase(int barNumber)
        {
            security.BarNumber = barNumber;
            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber);
            var se = sec.Positions.GetLastActiveForSignal("SE", barNumber);
            var bar = security.LastBar;

            if (le != null)
                if (bar.Low < stopLossLong)
                    le.CloseAtMarket(barNumber, "LXS");

            if (se != null)
                if (bar.High > stopLossShort)
                    se.CloseAtMarket(barNumber, "SXS");
        }

        public void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide)
        {
            this.leftLocalSide = leftLocalSide;
            this.rightLocalSide = rightLocalSide;
            this.pivotPointBreakDownSide = pivotPointBreakDownSide;
            this.EmaPeriodSide = EmaPeriodSide;

            parametersCombination = string.Format("leftLocal: {0}; rightLocal: {1}; breakDown: {2}; ema: {3}", leftLocalSide, rightLocalSide, pivotPointBreakDownSide, EmaPeriodSide);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
        }

        public void CalculateIndicators()
        {
            switch (positionSide)
            {
                case PositionSide.Long:
                    {
                        pivotPointsIndicator.CalculateLows(security, (int)leftLocalSide, (int)rightLocalSide);
                        break;
                    }
                case PositionSide.Short:
                    {
                        pivotPointsIndicator.CalculateHighs(security, (int)leftLocalSide, (int)rightLocalSide);
                        break;
                    }

            }
            ema = Series.EMA(sec.ClosePrices, (int)EmaPeriodSide);
            atr = Series.AverageTrueRange(sec.Bars, 20);
        }

        public void Paint(Context context)
        {
            var contextTSLab = context as ContextTSLab;
            var name = string.Format("{0} {1} {2}", sec.ToString(), positionSide, sec.Interval);
            var pane = contextTSLab.context.CreateGraphPane(name: name, title: name);
            var colorTSlab = new TSLab.Script.Color(SystemColor.Blue.ToArgb());
            var securityTSLab = (SecurityTSlab)security;
            pane.AddList(sec.ToString(), securityTSLab.security, CandleStyles.BAR_CANDLE, colorTSlab, PaneSides.RIGHT);

            colorTSlab = new TSLab.Script.Color(SystemColor.Gold.ToArgb());
            pane.AddList("EMA", ema, ListStyles.LINE, colorTSlab, LineStyles.SOLID, PaneSides.RIGHT);

            switch (positionSide)
            {
                case PositionSide.Long:
                    {
                        var lows = pivotPointsIndicator.GetLows(security.BarNumber);
                        var listLows = new List<bool>();

                        for (var i = 0; i <= security.BarNumber; i++)
                            listLows.Add(false);

                        foreach (var low in lows)
                            listLows[low.BarNumber] = true;

                        colorTSlab = new TSLab.Script.Color(SystemColor.Green.ToArgb());
                        pane.AddList("Lows", listLows, ListStyles.HISTOHRAM, colorTSlab, LineStyles.SOLID, PaneSides.LEFT);
                        break;
                    }
                case PositionSide.Short:
                    {
                        var highs = pivotPointsIndicator.GetHighs(security.BarNumber);
                        var listHighs = new List<bool>();

                        for (var i = 0; i <= security.BarNumber; i++)
                            listHighs.Add(false);

                        foreach (var high in highs)
                            listHighs[high.BarNumber] = true;

                        colorTSlab = new TSLab.Script.Color(SystemColor.Red.ToArgb());
                        pane.AddList("Highs", listHighs, ListStyles.HISTOHRAM, colorTSlab, LineStyles.SOLID, PaneSides.LEFT);
                        break;
                    }
            }           
        }

        public bool HasOpenPosition()
        {
            IPosition position = null;
            if (positionSide == PositionSide.Long)
                position = sec.Positions.GetLastActiveForSignal("LE");
            else if (positionSide == PositionSide.Short)
                position = sec.Positions.GetLastActiveForSignal("SE");
            return position != null;
        }

        public void Execute()
        {
            try
            {
                //ctx.ClearLog();                               
                security.ResetBarNumberToLastBarNumber();
                var lastBar = security.LastBar;
                var barsCount = security.GetBarsCountReal();

                var lastBarNumber = barsCount - 1;
                var prevLastBarNumber = lastBarNumber - 1;
                var prevLastBar = security.GetBar(prevLastBarNumber);

                if (ctx.IsLastBarClosed)
                {
                    Logger.Log("Последний бар закрыт!!!!!!!!!!!!!!!! Последний бар № + " + lastBarNumber + ": " + lastBar.Date);
                    Logger.Log("Пересчитываем индикаторы");
                    CalculateIndicators(lastBarNumber);
                }
                else
                {
                    Logger.Log("Последний бар не закрыт. Последний бар № + " + lastBarNumber + ": " + lastBar.Date);                   

                    Logger.Log("Пересчитываем индикаторы");
                    CalculateIndicators(prevLastBarNumber);
                    Logger.Log(string.Format("Была ли открыта новая позиция? (Заглушил)"));
                    if (WasNewPositionOpened())
                    {
                        Logger.Log(string.Format("Да, была"));
                        Logger.Log(string.Format("Создаём стоп-лосс"));
                        CreateStopLoss(lastBarNumber);

                        Logger.Log(string.Format("Сбрасываем признак того, что была открыта новая позиция"));
                        ResetFlagNewPositionOpened();
                    }
                    else
                    {
                        Logger.Log(string.Format("Нет, новая позиция не была открыта"));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        private void CalculateIndicators(int barNumber)
        {
            Logger.Log("Пересчёт индикатора на баре №" + barNumber);
            CalculateIndicators();

            var lastBarNumber = barNumber;
            if (lastBarNumber < 1)
                return;

            for (var i = 0; i <= lastBarNumber; i++)
            {
                Update(i);
                account.Update(i);
            }
            Logger.SwitchOn();
            var bar = GetBar(barNumber);
            SaveBarToContainer(bar.Date);
        }

        private void SaveBarToContainer(DateTime dateBar)
        {
            Logger.Log("Сохраняем бар в контейнер");
            SaveObjectToContainer("Последний бар", dateBar);
            Logger.Log("Проверим, сохранился ли бар в контейнере");

            if (WasBarSavedToContainer(dateBar))
                Logger.Log("Бар сохранился в контейнере");
            else
            {
                Logger.Log("Бар не сохранился в контейнере");
                throw new Exception("Вызываю исключение, так как бар не сохранился в контейнере!");
            }
        }

        private bool WasBarSavedToContainer(DateTime dateBar)
        {
            var dateBarLoaded = LoadBarFromContainer("Последний бар");
            return dateBar == dateBarLoaded;
        }

        private DateTime LoadBarFromContainer(string key)
        {
            Logger.Log("LoadBarFromContainer(key = " + key + ")");
            DateTime barDate = DateTime.MinValue;
            try
            {
                barDate = (DateTime)LoadObjectFromContainer(key);
            }
            catch (NullReferenceException e)
            {
                Logger.Log(e.Message + ". Возвращаем константу DateTime.MinValue");
            }

            return barDate;
        }

        private void SaveObjectToContainer(string key, object value)
        {
            var containerName = this.tradingSystemDescription + key;
            var container = new NotClearableContainer<object>(value);
            ctx.StoreObject(containerName, container);
        }

        private object LoadObjectFromContainer(string key)
        {
            var containerName = this.tradingSystemDescription + key;
            var container = ctx.LoadObject(containerName) as NotClearableContainer<object>;
            object value;
            if (container != null)
                value = container.Content;
            else
                throw new NullReferenceException("container равно null");

            return value;
        }      

        private bool WasNewPositionOpened()
        {
            return LoadFlagNewPositionOpened();
        }

        private void SetFlagNewPositionOpened()
        {
            SaveFlagNewPositionOpened(true);
        }

        private void ResetFlagNewPositionOpened()
        {
            SaveFlagNewPositionOpened(false);
        }

        private void SaveFlagNewPositionOpened(bool flag)
        {
            if (positionSide == PositionSide.Long || positionSide == PositionSide.Short)
            {
                Logger.Log("Взводим флаг открытия новой позиции в контейнере. Флаг: " + flag);
                SaveObjectToContainer("Новая позиция " + positionSide, flag);
                Logger.Log("Проверим, сохранился ли флаг в контейнере");

                if (WasFlagNewPositionOpenedSavedToContainer(flag))
                    Logger.Log("Флаг сохранился в контейнере");
                else
                {
                    Logger.Log("Флаг не сохранился в контейнере");
                    throw new Exception("Вызываю исключение, так как флаг не сохранился в контейнере!");
                }
            }
            else
            {
                throw new Exception("Вызываю исключение, так как значение positionSide ожидалось Long или Short, а оно " + positionSide);
            }
        }

        private bool LoadFlagNewPositionOpened()
        {
            if (positionSide == PositionSide.Long || positionSide == PositionSide.Short)
            {
                var containerName = "Новая позиция " + positionSide;
                Logger.Log("Название контейнера: " + containerName);
                var value = (bool)LoadObjectFromContainer(containerName);
                return value;
            }
            else
            {
                throw new Exception("Вызываю исключение, так как значение positionSide ожидалось Long или Short, а оно " + positionSide);
            }
        }

        private bool WasFlagNewPositionOpenedSavedToContainer(bool flag)
        {
            var flagSaved = LoadFlagNewPositionOpened();
            var message = string.Format("Сохранённый флаг = {0}, текущий флаг = {1}", flagSaved, flag);
            Logger.Log(message);
            return flag == flagSaved;
        }

        private void CreateStopLoss(int lastBarNumber)
        {
            Logger.LockCurrentStatus();
            Update(lastBarNumber);
            Logger.UnlockCurrentStatus();
        }

        private IDataBar GetBar(int barNumber)
        {
            Logger.Log("GetBar(barNumber = " + barNumber + ")");
            var bars = GetBars();
            return bars[barNumber];
        }

        private IReadOnlyList<IDataBar> GetBars()
        {
            Logger.Log("GetBars()");
            if (sec == null)
                throw new NullReferenceException("sec равно null");
            var bars = sec.Bars;
            if (bars == null)
                throw new NullReferenceException("sec.Bars равно null");
            if (bars.Count == 0)
                throw new ArgumentOutOfRangeException("bars.Count == 0");

            return sec.Bars;
        }

        public void CheckPositionOpenLongCase(double lastPrice, int barNumber)
        {
            throw new NotImplementedException();
        }

        public void CheckPositionOpenShortCase(double lastPrice, int barNumber)
        {
            throw new NotImplementedException();
        }
    }
}