﻿using System;
using System.Collections.Generic;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.Helpers;
using TSLab.Script.Handlers;
using TSLab.DataSource;

namespace TradingSystems
{
    public class TradingSystemPivotPointsEMA : TradingStrategy
    {
        public IContext Ctx { get; set; }
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

        string messageForLog = "";

        PositionSide positionSide;

        private double leftLocalSide;
        private double rightLocalSide;
        private double pivotPointBreakDownSide;
        private double EmaPeriodSide;

        public PositionSide PositionSide { get { return positionSide; } }

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

                if (security.IsRealTimeActualBar(barNumber))
                    Logger.SwitchOn();
                else
                    Logger.SwitchOff();

                switch (positionSide)
                {                    
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
                Logger.Log("Исключение в методе Update(): " + e.ToString());
            }
        }

        public void CheckPositionOpenLongCase(double lastPrice, int barNumber)
        {
            var le = sec.Positions.GetLastActiveForSignal("LE", barNumber);

            var lows = pivotPointsIndicator.GetLows(barNumber);
            if (lows.Count < 2)
                return;

            var lastLow = lows[lows.Count - 1];
            var prevLastLow = lows[lows.Count - 2];

            var lowsValues = new List<double>();
            foreach (var low in lows)
                lowsValues.Add(low.Value);

            breakdownLong = (pivotPointBreakDownSide / 100) * atr[barNumber];

            messageForLog = string.Format("Инструмент: {0}; номер бара (б. №) {1}. Открыта ли длинная позиция?", security.Name, barNumber);
            Logger.Log(messageForLog);

            if (le == null)
            {
                Logger.Log("Длинная позиция не открыта. Выполняется ли условие двух последовательных повышающихся минимумов?");

                if (patternPivotPoints_1g2.Check(lowsValues))   //1
                {
                    messageForLog = string.Format("Да, выполняется: последний минимум б. №{0}: {1} выше предыдущего б. №{2}: {3}. " +
                        "Использовался ли последний минимум в попытке открыть длинную позицию ранее?",
                        lastLow.BarNumber, lastLow.Value, prevLastLow.BarNumber, prevLastLow.Value);
                    Logger.Log(messageForLog);

                    var isLastLowCaseLongCloseNotExist = lastLowForOpenLongPosition == null;

                    if (isLastLowCaseLongCloseNotExist || (lastLow.BarNumber != lastLowForOpenLongPosition.BarNumber))    //2
                    {
                        if (isLastLowCaseLongCloseNotExist)
                            messageForLog = "Последняя попытка открыть длинную позицию не обнаружена. Не отсеивается ли потенциальная сделка фильтром EMA?";

                        else
                            messageForLog = string.Format("Нет, не использовался. Последний минимум, который использовался в попытке " +
                            "открыть длинную позицию ранее -- б. №{0}: {1}. Не отсеивается ли потенциальная сделка фильтром EMA?",
                            lastLowForOpenLongPosition.BarNumber, lastLowForOpenLongPosition.Value);

                        Logger.Log(messageForLog);

                        if (lastPrice > ema[barNumber]) //3
                        {
                            messageForLog = string.Format("Нет, потенциальная сделка не отсеивается фильтром. Последняя цена закрытия {0} выше EMA: {1}. ", lastPrice, ema[barNumber]);
                            Logger.Log(messageForLog);

                            Logger.Log("Проверяем актуально ли открытие длинной позиции?");

                            if (true || IsOpenLongPositionCaseActual(lastLow, barNumber)) //заглушил
                            {
                                Logger.Log("Открытие длинной позиции актуально.");

                                Logger.Log("Вычисляем стоп-цену...");
                                var stopPrice = lastLow.Value - breakdownLong;

                                messageForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                                    "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Последняя цена выше стоп-цены?", atr[barNumber], pivotPointBreakDownSide,
                                    breakdownLong, lastLow.Value, stopPrice);
                                Logger.Log(messageForLog);

                                messageForLog = string.Format("Запоминаем минимум, использовавшийся для попытки открытия длинной позиции.");
                                Logger.Log(messageForLog);

                                lastLowForOpenLongPosition = lastLow;

                                if (lastPrice > stopPrice)  //4
                                {
                                    messageForLog = string.Format("Да, последняя цена ({0}) выше стоп-цены ({1}). Открываем длинную позицию...", lastPrice, stopPrice);
                                    Logger.Log(messageForLog);

                                    Logger.Log("Определяем количество контрактов...");

                                    var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Long);

                                    Logger.Log("Торгуем в лаборатории или в режиме реального времени?");
                                    if (security.IsRealTimeTrading)
                                    {
                                        contracts = 1;
                                        messageForLog = string.Format("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                                        Logger.Log(messageForLog);

                                        var price = lastPrice + atr[barNumber];
                                        sec.Positions.BuyAtPrice(barNumber + 1, contracts, price, "LE");
                                    }
                                    else
                                    {
                                        Logger.Log("Торгуем в лаборатории.");
                                        sec.Positions.BuyAtMarket(barNumber + 1, contracts, "LE");
                                    }

                                    stopLossLong = stopPrice;

                                    Logger.Log("Проверяем актуальный ли это бар.");
                                    if (security.IsRealTimeActualBar(barNumber))
                                    {
                                        Logger.Log("Бар актуальный.");

                                        var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                                        Logger.Log(string.Format("Сохраним stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));
                                        var container = new NotClearableContainer<double>(stopLossLong);

                                        Ctx.StoreObject(containerName, container);
                                        Logger.Log(string.Format("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));

                                        container = Ctx.LoadObject(containerName) as NotClearableContainer<double>;
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

                                    Logger.Log("Открываем длинную позицию! Отправляем ордер.");
                                }
                                else
                                {
                                    Logger.Log("Последняя цена ниже стоп-цены. Длинную позицию не открываем.");
                                }
                            }
                            else
                            {
                                Logger.Log("Открытие длинной позиции не актуально.");
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
                    messageForLog = string.Format("Нет, не выполняется: последний минимум б. №{0}: {1} не выше предыдущего б. №{2}: {3}.",
                        lastLow.BarNumber, lastLow.Value, prevLastLow.BarNumber, prevLastLow.Value);
                    Logger.Log(messageForLog);
                }
            }
            else
            {
                Logger.Log("Длинная позиция открыта.");
                UpdateStopLossLongPosition(barNumber, lows, lastLow, le);
                CheckLongPositionCloseCase(le, barNumber);
            }
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

            messageForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                            "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Новый стоп-лосс выше прежнего?", atr[barNumber], pivotPointBreakDownSide,
                            breakdownLong, lastLow.Value, stopLoss);
            Logger.Log(messageForLog);

            Logger.Log("Проверяем актуальный ли это бар.");
            if (security.IsRealTimeActualBar(barNumber))
            {
                Logger.Log("Бар актуальный.");

                var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                Logger.Log(string.Format("Загружаем stopLossLong из контейнера \"{0}\".", containerName));
                if (Ctx == null)
                    Logger.Log("!ctx==null!");
                var container = Ctx.LoadObject(containerName) as NotClearableContainer<double>;

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
                messageForLog = string.Format("Да, новый стоп-лосс ({0}) выше прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossLong);

                Logger.Log(messageForLog);
                stopLossLong = stopLoss;

                Logger.Log("Проверяем актуальный ли это бар.");
                if (security.IsRealTimeActualBar(barNumber))
                {
                    Logger.Log("Бар актуальный.");

                    var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                    Logger.Log(string.Format("Сохраним stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));
                    var container = new NotClearableContainer<double>(stopLossLong);

                    Ctx.StoreObject(containerName, container);
                    Logger.Log(string.Format("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));

                    container = Ctx.LoadObject(containerName) as NotClearableContainer<double>;
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
                messageForLog = string.Format("Нет, новый стоп-лосс ({0}) не выше прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossLong);
                Logger.Log(messageForLog);
            }

            le.CloseAtStop(barNumber + 1, stopLossLong, atr[barNumber], "LXS");
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
                    le.CloseAtMarket(barNumber + 1, "LXE");

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

        public void CheckPositionOpenShortCase(double lastPrice, int barNumber)
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

            messageForLog = string.Format("Инструмент: {0}; номер бара (б. №) {1}. Открыта ли короткая позиция?", security.Name, barNumber);
            Logger.Log(messageForLog);

            if (se == null)
            {
                Logger.Log("Короткая позиция не открыта. Выполняется ли условие двух последовательных понижающихся максимумов?");

                if (patternPivotPoints_1l2.Check(highsValues))   //1
                {
                    messageForLog = string.Format("Да, выполняется: последний максимум б. №{0}: {1} ниже предыдущего б. №{2}: {3}. " +
                        "Использовался ли последний максимум в попытке открыть короткую позицию ранее?",
                        lastHigh.BarNumber, lastHigh.Value, prevLastHigh.BarNumber, prevLastHigh.Value);
                    Logger.Log(messageForLog);

                    var isLastHighCaseShortCloseNotExist = lastHighForOpenShortPosition == null;

                    if (isLastHighCaseShortCloseNotExist || (lastHigh.BarNumber != lastHighForOpenShortPosition.BarNumber))    //2
                    {
                        if (isLastHighCaseShortCloseNotExist)
                            messageForLog = "Последняя попытка открыть короткую позицию не обнаружена. Не отсеивается ли потенциальная сделка фильтром EMA?";

                        else
                            messageForLog = string.Format("Нет, не использовался. Последний максимум, который использовался в попытке " +
                            "открыть короткую позицию ранее -- б. №{0}: {1}. Не отсеивается ли потенциальная сделка фильтром EMA?",
                            lastHighForOpenShortPosition.BarNumber, lastHighForOpenShortPosition.Value);

                        Logger.Log(messageForLog);

                        if (lastPrice < ema[barNumber]) //3
                        {
                            messageForLog = string.Format("Нет, потенциальная сделка не отсеивается фильтром. Последняя цена закрытия {0} ниже EMA: {1}.", lastPrice, ema[barNumber]);
                            Logger.Log(messageForLog);

                            Logger.Log("Проверяем актуально ли открытие короткой позиции?");

                            if (true || IsOpenShortPositionCaseActual(lastHigh, barNumber))//заглушил
                            {
                                Logger.Log("Открытие короткой позции актуально?");

                                Logger.Log("Вычисляем стоп-цену...");

                                var stopPrice = lastHigh.Value + breakdownShort;

                                messageForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                                    "стоп-лосс = последний максимум {3} + допустимый уровень пробоя {2} = {4}. Последняя цена ниже стоп-цены?", atr[barNumber], pivotPointBreakDownSide,
                                    breakdownShort, lastHigh.Value, stopPrice);
                                Logger.Log(messageForLog);

                                messageForLog = string.Format("Запоминаем максимум, использовавшийся для попытки открытия короткой позиции.");
                                Logger.Log(messageForLog);

                                lastHighForOpenShortPosition = lastHigh;

                                if (lastPrice < stopPrice)  //4
                                {
                                    messageForLog = string.Format("Да, последняя цена ({0}) ниже стоп-цены ({1}). Открываем короткую позицию...", lastPrice, stopPrice);
                                    Logger.Log(messageForLog);

                                    Logger.Log("Определяем количество контрактов...");

                                    var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Short);

                                    Logger.Log("Торгуем в лаборатории или в режиме реального времени?");
                                    if (security.IsRealTimeTrading)
                                    {
                                        contracts = 1;
                                        messageForLog = string.Format("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                                        Logger.Log(messageForLog);

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
                                    if (security.IsRealTimeActualBar(barNumber))
                                    {
                                        Logger.Log("Бар актуальный.");

                                        var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
                                        Logger.Log(string.Format("Сохраним stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));
                                        var container = new NotClearableContainer<double>(stopLossShort);

                                        Ctx.StoreObject(containerName, container);

                                        Logger.Log(string.Format("Проверим, сохранился ли stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));

                                        container = Ctx.LoadObject(containerName) as NotClearableContainer<double>;

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
                            messageForLog = string.Format("Cделка отсеивается фильтром, так как последняя цена закрытия {0} выше или совпадает с EMA: {1}.", lastPrice, ema[barNumber]);
                            Logger.Log(messageForLog);
                        }
                    }
                    else
                    {
                        messageForLog = string.Format("Да, последний максимум использовался в попытке открыть короткую позицию ранее.");
                        Logger.Log(messageForLog);
                    }
                }
                else
                {
                    messageForLog = string.Format("Нет, не выполняется: последний максимум б. №{0}: {1} не ниже предыдущего б. №{2}: {3}.",
                        lastHigh.BarNumber, lastHigh.Value, prevLastHigh.BarNumber, prevLastHigh.Value);
                    Logger.Log(messageForLog);
                }
            }
            else
            {
                Logger.Log("Короткая позиция открыта.");
                UpdateStopLossShortPosition(barNumber, highs, lastHigh, se);
                CheckShortPositionCloseCase(se, barNumber);
            }
        }

        private bool IsOpenLongPositionCaseActual(Indicator lastLow, int barNumber)
        {
            var previousBarNumber = security.RealTimeActualBarNumber - 1;
            return (barNumber == lastLow.BarNumber + rightLocalSide) || (security.GetBarClose(previousBarNumber) < ema[previousBarNumber]);
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

            messageForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                            "стоп-лосс = последний максимум {3} + допустимый уровень пробоя {2} = {4}. Новый стоп-лосс ниже прежнего?", atr[barNumber], pivotPointBreakDownSide,
                            breakdownShort, lastHigh.Value, stopLoss);
            Logger.Log(messageForLog);

            Logger.Log("Проверяем актуальный ли это бар.");
            if (security.IsRealTimeActualBar(barNumber))
            {
                Logger.Log("Бар актуальный.");

                var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
                Logger.Log(string.Format("Загружаем stopLossShort из контейнера \"{0}\".", containerName));
                var container = Ctx.LoadObject(containerName) as NotClearableContainer<double>;

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
                messageForLog = string.Format("Да, новый стоп-лосс ({0}) ниже прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossShort);

                Logger.Log(messageForLog);
                stopLossShort = stopLoss;

                Logger.Log("Проверяем актуальный ли это бар.");
                if (security.IsRealTimeActualBar(barNumber))
                {
                    Logger.Log("Бар актуальный.");

                    var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
                    Logger.Log(string.Format("Сохраним stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));
                    var container = new NotClearableContainer<double>(stopLossShort);

                    Ctx.StoreObject(containerName, container);

                    Logger.Log(string.Format("Проверим, сохранился ли stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));

                    container = Ctx.LoadObject(containerName) as NotClearableContainer<double>;

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
                messageForLog = string.Format("Нет, новый стоп-лосс ({0}) не ниже прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossShort);
                Logger.Log(messageForLog);
            }

            se.CloseAtStop(barNumber + 1, stopLossShort, atr[barNumber], "SXS");
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
                    se.CloseAtMarket(barNumber + 1, "SXE");

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
            var pane = contextTSLab.CreateGraphPane(name: name, title: name);
            //var colorTSlab = new TSLab.Script.Color(SystemColor.Blue.ToArgb());
            var colorTSlab = SystemColor.Blue;
            var securityTSLab = (SecurityTSlab)security;
            pane.AddList(sec.ToString(), securityTSLab.security, CandleStyles.BAR_CANDLE, colorTSlab, PaneSides.RIGHT);

            //colorTSlab = new TSLab.Script.Color(SystemColor.Gold.ToArgb());
            colorTSlab = SystemColor.Gold;
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

                        //colorTSlab = new TSLab.Script.Color(SystemColor.Green.ToArgb());
                        colorTSlab = SystemColor.Green;
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

                        //colorTSlab = new TSLab.Script.Color(SystemColor.Red.ToArgb());
                        colorTSlab = SystemColor.Red;
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

        public void Initialize(IContext ctx)
        {
            Ctx = ctx;
        }

        public void SetParameters(SystemParameters systemParameters)
        {
            throw new NotImplementedException();
        }
    }
}