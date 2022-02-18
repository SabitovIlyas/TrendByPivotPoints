//using System.Collections.Generic;
//using TSLab.DataSource;

//namespace TrendByPivotPointsStrategy
//{
//    public class Short
//    {
//        public void CheckPositionOpenShortCase()
//        {
//            var se = sec.Positions.GetLastActiveForSignal("SE", barNumber);
//            var highs = pivotPointsIndicator.GetHighs(barNumber);

//            if (highs.Count < 2)
//                return;

//            var lastHigh = highs[highs.Count - 1];
//            var prevLastHigh = highs[highs.Count - 2];

//            var highsValues = new List<double>();
//            foreach (var high in highs)
//                highsValues.Add(high.Value);

//            breakdownShort = (pivotPointBreakDownSide / 100) * atr[barNumber];

//            textForLog = string.Format("Инструмент: {0}; номер бара (б. №) {1}. Открыта ли короткая позиция?", security.Name, barNumber);
//            Logger.Log(textForLog);

//            if (se == null)
//            {
//                Logger.Log("Короткая позиция не открыта. Выполняется ли условие двух последовательных понижающихся максимумов?");

//                if (patternPivotPoints_1l2.Check(highsValues))   //1
//                {
//                    textForLog = string.Format("Да, выполняется: последний максимум б. №{0}: {1} ниже предыдущего б. №{2}: {3}. " +
//                        "Использовался ли последний максимум в попытке открыть короткую позицию ранее?",
//                        lastHigh.BarNumber, lastHigh.Value, prevLastHigh.BarNumber, prevLastHigh.Value);
//                    Logger.Log(textForLog);

//                    var isLastHighCaseShortCloseNotExist = lastHighForOpenShortPosition == null;

//                    if (isLastHighCaseShortCloseNotExist || (lastHigh.BarNumber != lastHighForOpenShortPosition.BarNumber))    //2
//                    {
//                        if (isLastHighCaseShortCloseNotExist)
//                            textForLog = "Последняя попытка открыть короткую позицию не обнаружена. Не отсеивается ли потенциальная сделка фильтром EMA?";

//                        else
//                            textForLog = string.Format("Нет, не использовался. Последний максимум, который использовался в попытке " +
//                            "открыть короткую позицию ранее -- б. №{0}: {1}. Не отсеивается ли потенциальная сделка фильтром EMA?",
//                            lastHighForOpenShortPosition.BarNumber, lastHighForOpenShortPosition.Value);

//                        Logger.Log(textForLog);

//                        if (lastPrice < ema[barNumber]) //3
//                        {
//                            textForLog = string.Format("Нет, потенциальная сделка не отсеивается фильтром. Последняя цена закрытия {0} ниже EMA: {1}.", lastPrice, ema[barNumber]);
//                            Logger.Log(textForLog);

//                            Logger.Log("Проверяем актуально ли открытие короткой позиции?");

//                            if (true || IsOpenShortPositionCaseActual(lastHigh, barNumber))//заглушил
//                            {
//                                Logger.Log("Открытие короткой позции актуально?");

//                                Logger.Log("Вычисляем стоп-цену...");
//                                var stopPrice = lastHigh.Value + breakdownShort;

//                                textForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
//                                    "стоп-лосс = последний максимум {3} + допустимый уровень пробоя {2} = {4}. Последняя цена ниже стоп-цены?", atr[barNumber], pivotPointBreakDownSide,
//                                    breakdownShort, lastHigh.Value, stopPrice);
//                                Logger.Log(textForLog);

//                                textForLog = string.Format("Запоминаем максимум, использовавшийся для попытки открытия короткой позиции.");
//                                Logger.Log(textForLog);

//                                lastHighForOpenShortPosition = lastHigh;

//                                if (lastPrice < stopPrice)  //4
//                                {
//                                    textForLog = string.Format("Да, последняя цена ({0}) ниже стоп-цены ({1}). Открываем короткую позицию...", lastPrice, stopPrice);
//                                    Logger.Log(textForLog);

//                                    Logger.Log("Определяем количество контрактов...");

//                                    var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, PositionSide.Short);

//                                    Logger.Log("Торгуем в лаборатории или в режиме реального времени?");
//                                    if (security.IsRealTimeTrading)
//                                    {
//                                        contracts = 1;
//                                        textForLog = string.Format("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
//                                        Logger.Log(textForLog);

//                                        var price = lastPrice - atr[barNumber];
//                                        sec.Positions.SellAtPrice(barNumber + 1, contracts, price, "SE");
//                                    }
//                                    else
//                                    {
//                                        Logger.Log("Торгуем в лаборатории.");
//                                        sec.Positions.SellAtMarket(barNumber + 1, contracts, "SE");
//                                    }

//                                    stopLossShort = stopPrice;

//                                    Logger.Log("Проверяем актуальный ли это бар.");
//                                    if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
//                                    {
//                                        Logger.Log("Бар актуальный.");
//                                        CreateStopLossShort();
//                                        SetFlagNewPositionOpened();
//                                    }
//                                    else
//                                    {
//                                        Logger.Log("Бар не актуальный.");
//                                    }

//                                    Logger.Log("Открываем короткую позицию! Отправляем ордер.");
//                                }
//                                else
//                                {
//                                    Logger.Log("Последняя цена выше стоп-цены. Короткую позицию не открываем.");
//                                }
//                            }
//                            else
//                            {
//                                Logger.Log("Открытие короткой позиции не актуально.");
//                            }
//                        }
//                        else
//                        {
//                            textForLog = string.Format("Cделка отсеивается фильтром, так как последняя цена закрытия {0} выше или совпадает с EMA: {1}.", lastPrice, ema[barNumber]);
//                            Logger.Log(textForLog);
//                        }
//                    }
//                    else
//                    {
//                        textForLog = string.Format("Да, последний максимум использовался в попытке открыть короткую позицию ранее.");
//                        Logger.Log(textForLog);
//                    }
//                }
//                else
//                {
//                    textForLog = string.Format("Нет, не выполняется: последний максимум б. №{0}: {1} не ниже предыдущего б. №{2}: {3}.",
//                        lastHigh.BarNumber, lastHigh.Value, prevLastHigh.BarNumber, prevLastHigh.Value);
//                    Logger.Log(textForLog);
//                }
//            }
//            else
//            {
//                Logger.Log("Короткая позиция открыта.");
//                UpdateStopLossShortPosition(barNumber, highs, lastHigh, se);
//                CheckShortPositionCloseCase(se, barNumber);
//            }
//        }

//        private bool IsOpenShortPositionCaseActual(Indicator lastHigh, int barNumber)
//        {
//            var previousBarNumber = security.RealTimeActualBarNumber - 1;
//            return (barNumber == lastHigh.BarNumber + rightLocalSide) || (security.GetBarClose(previousBarNumber) > ema[previousBarNumber]);
//        }

//        private void UpdateStopLossShortPosition(int barNumber, List<Indicator> highs, Indicator lastHigh, IPosition se)
//        {
//            Logger.Log("Обновляем стоп...");

//            if (highs.Count == 0)
//            {
//                Logger.Log("Это условие никогда не должно отрабатывать. Проверить и впоследствии убрать.");
//                return;
//            }

//            var stopLoss = lastHigh.Value + breakdownShort;

//            textForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
//                            "стоп-лосс = последний максимум {3} + допустимый уровень пробоя {2} = {4}. Новый стоп-лосс ниже прежнего?", atr[barNumber], pivotPointBreakDownSide,
//                            breakdownShort, lastHigh.Value, stopLoss);
//            Logger.Log(textForLog);

//            Logger.Log("Проверяем актуальный ли это бар.");
//            if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
//            {
//                Logger.Log("Бар актуальный.");

//                var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
//                Logger.Log(string.Format("Загружаем stopLossShort из контейнера \"{0}\".", containerName));
//                var container = ctx.LoadObject(containerName) as NotClearableContainer<double>;

//                if (container != null)
//                {
//                    Logger.Log("Загрузили контейнер.");
//                    var value = container.Content;
//                    Logger.Log(string.Format("Значение в контейнере: value = {0}", value));
//                    if (value != 0d)
//                    {
//                        Logger.Log("Записываем в stopLossShort ненулевое значение из контейнера.");
//                        stopLossLong = value;
//                    }
//                    else
//                    {
//                        Logger.Log("Значение в контейнере равно нулю! Значение из контейнера отбрасываем!");
//                    }
//                }
//                else
//                {
//                    Logger.Log("Не удалось загрузить контейнер.");
//                }
//            }
//            else
//            {
//                Logger.Log("Бар не актуальный.");
//            }

//            if (stopLoss < stopLossShort)
//            {
//                textForLog = string.Format("Да, новый стоп-лосс ({0}) ниже прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossShort);

//                Logger.Log(textForLog);
//                stopLossShort = stopLoss;

//                Logger.Log("Проверяем актуальный ли это бар.");
//                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
//                {
//                    Logger.Log("Бар актуальный.");

//                    var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
//                    Logger.Log(string.Format("Сохраним stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));
//                    var container = new NotClearableContainer<double>(stopLossShort);

//                    ctx.StoreObject(containerName, container);
//                    Logger.Log(string.Format("Проверим, сохранился ли stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));

//                    container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
//                    double value = 0d;
//                    if (container != null)
//                        value = container.Content;

//                    if (value != 0d)
//                        if (value == stopLossLong)
//                            Logger.Log(string.Format("stopLossShort сохранился в контейнере. Значение в контейнере: value = {0}.", value));

//                        else
//                            Logger.Log(string.Format("stopLossShort НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value));
//                }
//                else
//                {
//                    Logger.Log("Бар не актуальный.");
//                }
//            }
//            else
//            {
//                textForLog = string.Format("Нет, новый стоп-лосс ({0}) не ниже прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossShort);
//                Logger.Log(textForLog);
//            }

//            if (WasNewPositionOpened())
//            {
//                Logger.Log("Открыта новая позиция. Устанавливаем стоп-лосс на текущем баре.");
//                se.CloseAtStop(barNumber, stopLossShort, atr[barNumber], "SXS");
//            }
//            else
//            {
//                Logger.Log("Новая позиция не открыта. Устанавливаем стоп-лосс на следующем баре.");
//                se.CloseAtStop(barNumber + 1, stopLossShort, atr[barNumber], "SXS");
//            }
//        }

//        public bool CheckShortPositionCloseCase(IPosition se, int barNumber)
//        {
//            security.BarNumber = barNumber;
//            var bar = security.LastBar;

//            if (se != null)
//            {
//                Logger.Log("Проверяем пробил ли вверх максимум последнего бара стоп-лосс для шорта?");
//                if (bar.High >= stopLossShort)
//                {
//                    if (WasNewPositionOpened())
//                    {
//                        Logger.Log("Открыта новая позиция. Устанавливаем стоп-лосс на текущем баре.");
//                        se.CloseAtMarket(barNumber, "SXE");
//                    }
//                    else
//                    {
//                        Logger.Log("Новая позиция не открыта. Устанавливаем стоп-лосс на следующем баре.");
//                        se.CloseAtMarket(barNumber + 1, "SXE");
//                    }

//                    var message = string.Format("Да, максимум последнего бара {0} пробил вверх стоп-лосс для шорта {1}. Закрываем позицию по рынку на следующем баре.", bar.High, stopLossShort);
//                    Logger.Log(message);

//                    return true;
//                }
//                else
//                {
//                    var message = string.Format("Нет, максимум последнего бара {0} ниже стоп-лосса для шорта {1}. Оставляем позицию.", bar.High, stopLossShort);
//                    Logger.Log(message);
//                }
//            }
//            return false;
//        }

//        private void CreateStopLossShort()
//        {
//            var containerName = string.Format("stopLossShort {0} {1}", security.Name, positionSide);
//            Logger.Log(string.Format("Сохраним stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));
//            var container = new NotClearableContainer<double>(stopLossShort);

//            ctx.StoreObject(containerName, container);
//            Logger.Log(string.Format("Проверим, сохранился ли stopLossShort = {0} в контейнере \"{1}\".", stopLossShort, containerName));

//            container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
//            double value = 0d;
//            if (container != null)
//                value = container.Content;

//            if (value != 0d)
//                if (value == stopLossLong)
//                    Logger.Log(string.Format("stopLossShort сохранился в контейнере. Значение в контейнере: value = {0}.", value));

//                else
//                    Logger.Log(string.Format("stopLossShort НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value));
//        }
//    }
//}