using System;
using System.Collections.Generic;
using System.Linq;
using TrendByPivotPoints;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;
using TSLab.Script.Realtime;
using SystemColor = System.Drawing.Color;

namespace TradingSystems
{
    public class TradingSystemBollingerBands : TradingStrategy
    {
        public Logger Logger { get; set; } = new NullLogger();
        public PositionSide PositionSide { get { return positionSide; } }
        public IContext Ctx { get; set; }

        private PositionSide positionSide;
        private int barNumber;
        private Converter convertedLong;
        private string signalNameForOpenPosition = string.Empty;
        private string signalNameForClosePositionByTakeProfit = string.Empty;
        private string tradingSystemDescription;
        private string name = nameof(TradingSystemBollingerBands);
        private string parametersCombination;
        private ISecurity sec;
        private Security security;

        private int periodBollingerBandAndEma;
        private double standartDeviationCoef;
        private bool useRelatedOrders;
        private double profitPercent;
        private IList<double> ema;
        private IList<double> bollingerBand;
        private bool isPriceCrossedEmaAfterOpenOrChangePosition;

        private int startLots;
        private int maxLots;
        private double lastUsedPrice = 0;
        private double changePositionLastDealPrice = 0;

        private int hourStopTrading = 23;
        private int minuteStopTrading = 45;

        private int currentOpenedShares = 0;
        private int changePositionCounter = 0;
        private RealTimeTrading realTimeTrading;
        private IOrder lastExecutedOrderForOpenOrChangePosition;
        private int constParam = 400;
        private int multiplyCoef = 2;
        private Position currentPosition;

        public TradingSystemBollingerBands(Security security, PositionSide positionSide)
        {
            var securityTSLab = security as SecurityTSlab;
            sec = securityTSLab.security;
            this.security = security;
            this.positionSide = positionSide;
        }

        public void Initialize(IContext ctx)
        {
            Ctx = ctx;
        }

        public void CalculateIndicators()
        {
            var methodName = nameof(CalculateIndicators);
            Log("{0}: Рассчитываем индикаторы и осуществляем первоначальные настройки", methodName);

            switch (positionSide)
            {
                case PositionSide.Long:
                    {
                        signalNameForOpenPosition = "LE";
                        signalNameForClosePositionByTakeProfit = "LXT";
                        convertedLong = new Converter(isConverted: false);
                        break;
                    }
                case PositionSide.Short:
                    {
                        signalNameForOpenPosition = "SE";
                        signalNameForClosePositionByTakeProfit = "SXT";
                        convertedLong = new Converter(isConverted: true);
                        break;
                    }
            }
            ema = Series.EMA(sec.ClosePrices, periodBollingerBandAndEma);
            bollingerBand = Series.BollingerBands(sec.ClosePrices, ema, periodBollingerBandAndEma, standartDeviationCoef, isTopLine: convertedLong.IsConverted);

            Log("{0}: Расчёт осуществлён", methodName);
        }

        public void Update(int barNumber)
        {
            try
            {
                this.barNumber = barNumber;
                security.BarNumber = barNumber;

                if (security.IsRealTimeActualBar(barNumber))
                    Logger.SwitchOn();
                else
                    Logger.SwitchOff();

                LogLocalCacheContent();
                CheckPositionOpenLongCase();
            }

            catch (Exception e)
            {
                Log("Исключение в методе Update(): {0}.\r\n{1}.\r\n{2}.\r\n{3}.\r\n{4}.", e.ToString(), e.Message, e.Data, e.Source, e.StackTrace);
            }
        }

        private void LogLocalCacheContent()
        {
            var methodName = nameof(LogLocalCacheContent);
            Log("{0}: Проверяем локальный кеш", methodName);
            var key = string.Empty;
            var value = string.Empty;

            try
            {
                key = nameof(isPriceCrossedEmaAfterOpenOrChangePosition);
                value = realTimeTrading.LoadObjectFromContainer(key).ToString();
                Log("{0}: {1}: {2} = {3}", methodName, nameof(realTimeTrading), key, value);
            }
            catch
            {
                Log("{0}: {1}: {2} = {3}", methodName, nameof(realTimeTrading), key, "Значения нет");
            }

            try
            {
                key = nameof(currentOpenedShares);
                value = realTimeTrading.LoadObjectFromContainer(key).ToString();
                Log("{0}: {1}: {2} = {3}", methodName, nameof(realTimeTrading), key, value);
            }
            catch
            {
                Log("{0}: {1}: {2} = {3}", methodName, nameof(realTimeTrading), key, "Значения нет");
            }

            try
            {
                key = nameof(changePositionLastDealPrice);
                value = realTimeTrading.LoadObjectFromContainer(key).ToString();
                Log("{0}: {1}: {2} = {3}", methodName, nameof(realTimeTrading), key, value);
            }
            catch
            {
                Log("{0}: {1}: {2} = {3}", methodName, nameof(realTimeTrading), key, "Значения нет");
            }

            Log("{0}: Проверка кеша завершена", methodName);
        }

        public void CheckPositionOpenLongCase()
        {
            var methodName = nameof(CheckPositionOpenLongCase);
            Log("{0}: бар № {1}. Открыта ли {2} позиция?", methodName, barNumber, convertedLong.Long);

            var notes = GetSignalNotesName();

            if (!IsPositionOpen(notes))
            {
                Log("{0} позиция не открыта.", convertedLong.Long);
                if (!IsTimeForTrading())
                    return;

                changePositionCounter = 0;
                currentOpenedShares = 0;
                lastUsedPrice = 0;
                changePositionLastDealPrice = 0;

                Log("{0}: Это режим реальной торговли, и текущий бар актуален?", methodName);

                if (security.IsRealTimeTrading && security.IsRealTimeActualBar(barNumber))
                {
                    Log("{0}: Да, это режим реальной торговли, и текущий бар актуален.", methodName);
                    SaveChangePositionCounterToLocalCache();
                    SaveCurrentOpenedSharesToLocalCache();
                }
                else
                    Log("{0}: Да, это режим реальной торговли, и текущий бар актуален.", methodName);

                SetLimitOrdersForOpenPosition(notes);
            }
            else
            {
                Log("{0} позиция открыта.", convertedLong.Long);
                currentPosition = GetPosition(notes);

                UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition();
                UpdateParametersOfCurrentPosition(currentPosition);

                var currentPrice = security.GetBarClose(barNumber);

                //if (currentPrice <= 0)
                //{
                //    Log("{0}: Текущая цена неположительная. Закрываем позицию по рынку.", methodName);
                //    var signalName = signalNameForClosePositionByTakeProfit + notes;
                //    currentPosition.iPosition.CloseAtMarket(barNumber + 1, signalName);
                //    return;
                //}

                var priceTakeProfit = convertedLong.Plus(currentPosition.iPosition.AverageEntryPrice,
                    Math.Abs(GetAdaptiveTakeProfitPercent() / 100 * currentPosition.iPosition.AverageEntryPrice));
                var priceChangePosition = bollingerBand[barNumber];

                var isTakeProfitPriceNearestThanChangePositionPriceForCurrentPrice =
                    Math.Abs(priceTakeProfit - currentPrice) <=
                Math.Abs(priceChangePosition - currentPrice);

                if (useRelatedOrders)
                {
                    //if (isPriceCrossedEmaAfterOpenOrChangePosition || IsLastExecutedOrderForOpenOrChangePositionNotActiveAndHasRestQuantity())
                    if (isPriceCrossedEmaAfterOpenOrChangePosition || !IsCurrentPositionSharesCorrect())
                        SetLimitOrdersForChangePosition(currentPosition, notes);
                    SetLimitOrdersForClosePosition(currentPosition, notes);
                }
                else
                {
                    //if (!isTakeProfitPriceNearestThanChangePositionPriceForCurrentPrice &&
                    //(isPriceCrossedEmaAfterOpenOrChangePosition || IsLastExecutedOrderForOpenOrChangePositionNotActiveAndHasRestQuantity()))
                    
                    if (!isTakeProfitPriceNearestThanChangePositionPriceForCurrentPrice &&
                    (isPriceCrossedEmaAfterOpenOrChangePosition || !IsCurrentPositionSharesCorrect()))
                            SetLimitOrdersForChangePosition(currentPosition, notes);
                    else
                        SetLimitOrdersForClosePosition(currentPosition, notes);
                }
            }
        }

        private void UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition()
        {
            Log(nameof(UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition) + ": Обновляю флаг \" Пересечение цены EMA после открытия" +
                " или изменения позиции\". Текущее состояние флага: " + isPriceCrossedEmaAfterOpenOrChangePosition);

            if (convertedLong.IsGreater(security.GetBarClose(barNumber), ema[barNumber]))
            {
                isPriceCrossedEmaAfterOpenOrChangePosition = true;
                if (security.IsRealTimeTrading && security.IsRealTimeActualBar(barNumber))
                    SaveFlagIsPriceCrossedEmaAfterOpenOrChangePositionToLocalCache();
            }

            Log(nameof(UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition) + ": Новое состояние флага: " +
                isPriceCrossedEmaAfterOpenOrChangePosition);
        }

        private void UpdateParametersOfCurrentPosition(Position currentPosition)
        {
            var methodName = nameof(UpdateParametersOfCurrentPosition);
            Log("{0}: Обновляю значение \"Текущее количество открытых лотов\".", methodName);

            if (security.IsRealTimeTrading && security.IsRealTimeActualBar(barNumber))
            {
                try
                {
                    currentOpenedShares = LoadCurrentOpenedSharesFromLocalCache();
                    isPriceCrossedEmaAfterOpenOrChangePosition = LoadFlagIsPriceCrossedEmaAfterOpenOrChangePositionFromLocalCache();
                    changePositionCounter = LoadChangePositionCounterFromLocalCache();                               //changePositionCounter не использую, в текущей версии робота
                    changePositionLastDealPrice = LoadChangePositionLastDealPriceFromLocalCache();

                    if (lastExecutedOrderForOpenOrChangePosition != null)
                    {
                        changePositionLastDealPrice = lastExecutedOrderForOpenOrChangePosition.Price;
                        SaveChangePositionLastDealPriceToLocalCache();
                    }
                }
                catch (KeyNotFoundException)
                {
                    Log("{0}: Получено исключение обновления значений. Оставляем прежние значения", methodName);
                    SaveCurrentOpenedSharesToLocalCache();
                    SaveFlagIsPriceCrossedEmaAfterOpenOrChangePositionToLocalCache();
                    SaveChangePositionCounterToLocalCache();
                    SaveChangePositionLastDealPriceToLocalCache();
                }
            }

            Log("{0}: Значение, хранящееся в позиции: {1}", methodName, currentPosition.iPosition.Shares);

            if (currentPosition.iPosition.Shares != currentOpenedShares)
            {
                Log("{0}: Обновляем значение, сбрасываем флаг, наращиваем счётчик", methodName);
                currentOpenedShares = (int)currentPosition.iPosition.Shares;
                isPriceCrossedEmaAfterOpenOrChangePosition = false;
                changePositionLastDealPrice = lastUsedPrice;

                changePositionCounter++;

                if (security.IsRealTimeTrading && security.IsRealTimeActualBar(barNumber))
                {
                    SaveCurrentOpenedSharesToLocalCache();
                    SaveFlagIsPriceCrossedEmaAfterOpenOrChangePositionToLocalCache();
                    SaveChangePositionCounterToLocalCache();
                    SaveChangePositionLastDealPriceToLocalCache();
                }
            }
            else
                Log("{0}: Обновлять ничего не нужно", methodName);
        }

        private int LoadCurrentOpenedSharesFromLocalCache()
        {
            var methodName = nameof(LoadCurrentOpenedSharesFromLocalCache);
            Log("{0}: Считываем \"Текущее количество открытых лотов\" из локального кеша", methodName);
            var key = nameof(currentOpenedShares);
            var value = 0;

            try
            {
                value = (int)realTimeTrading.LoadObjectFromContainer(key);
                Log("{0}: Значение: \"Текущее количество открытых лотов\", равное {1} считано из кеша.",
                    methodName, value);
            }
            catch (NullReferenceException)
            {
                Log("{0}: Значение: \"Текущее количество открытых лотов\" не содержится в кеше. " +
                    "Генерируем исключение", methodName);
                throw new KeyNotFoundException(key);
            }
            catch (Exception ex)
            {
                Log("{0}: {1}", methodName, ex.Message);
            }

            return value;
        }

        private bool LoadFlagIsPriceCrossedEmaAfterOpenOrChangePositionFromLocalCache()
        {
            var methodName = nameof(LoadFlagIsPriceCrossedEmaAfterOpenOrChangePositionFromLocalCache);
            Log("{0}: Считываем флаг \"Пересечение цены EMA после открытия\" из локального кеша", methodName);
            var key = nameof(isPriceCrossedEmaAfterOpenOrChangePosition);
            var value = false;

            try
            {
                value = (bool)realTimeTrading.LoadObjectFromContainer(key);
                Log("{0}: Флаг: \"Пересечение цены EMA после открытия\", равное {1} считано из кеша.",
                    methodName, value);
            }
            catch (NullReferenceException)
            {
                Log("{0}: Флаг: \"Пересечение цены EMA после открытия\" не содержится в кеше. " +
                    "Генерируем исключение", methodName);
                throw new KeyNotFoundException(key);
            }
            catch (Exception ex)
            {
                Log("{0}: {1}", methodName, ex.Message);
            }

            return value;
        }

        private int LoadChangePositionCounterFromLocalCache()
        {
            var methodName = nameof(LoadChangePositionCounterFromLocalCache);
            Log("{0}: Считываем \"Текущее значение счётчика изменения позиции\" из локального кеша", methodName);
            var key = nameof(changePositionCounter);
            var value = 0;

            try
            {
                value = (int)realTimeTrading.LoadObjectFromContainer(key);
                Log("{0}: Значение: \"Текущее значение счётчика изменения позиции\", равное {1} считано из кеша.",
                    methodName, value);
            }
            catch (NullReferenceException)
            {
                Log("{0}: Значение: \"Текущее значение счётчика изменения позиции\" не содержится в кеше. " +
                    "Генерируем исключение", methodName);
                throw new KeyNotFoundException(key);
            }
            catch (Exception ex)
            {
                Log("{0}: {1}", methodName, ex.Message);
            }

            return value;
        }

        private double LoadChangePositionLastDealPriceFromLocalCache()
        {
            var methodName = nameof(LoadChangePositionLastDealPriceFromLocalCache);
            Log("{0}: Считываем последнюю цену, использованную в сделке, из локального кеша", methodName);
            var key = nameof(changePositionLastDealPrice);
            var value = 0d;

            try
            {
                value = (double)realTimeTrading.LoadObjectFromContainer(key);
                Log("{0}: Значение: \"Последня цена, использованная в сделке,\", равное {1} считано из кеша.",
                    methodName, value);
            }
            catch (NullReferenceException)
            {
                Log("{0}: Значение: \"Последня цена, использованная в сделке,\" не содержится в кеше. " +
                    "Генерируем исключение", methodName);
                throw new KeyNotFoundException(key);
            }
            catch (Exception ex)
            {
                Log("{0}: {1}", methodName, ex.Message);
            }

            return value;
        }

        private void SaveCurrentOpenedSharesToLocalCache()
        {
            var methodName = nameof(SaveCurrentOpenedSharesToLocalCache);
            Log("{0}: Сохраняем \"Текущее количество открытых лотов\" в локальный кеш", methodName);
            var key = nameof(currentOpenedShares);
            realTimeTrading.SaveObjectToContainer(key, currentOpenedShares);
        }

        private void SaveFlagIsPriceCrossedEmaAfterOpenOrChangePositionToLocalCache()
        {
            var methodName = nameof(SaveFlagIsPriceCrossedEmaAfterOpenOrChangePositionToLocalCache);
            Log("{0}: Сохраняем флаг \"Пересечение цены EMA после открытия\" в локальный кеш", methodName);
            var key = nameof(isPriceCrossedEmaAfterOpenOrChangePosition);
            realTimeTrading.SaveObjectToContainer(key, isPriceCrossedEmaAfterOpenOrChangePosition);
        }

        private void SaveChangePositionCounterToLocalCache()
        {
            var methodName = nameof(SaveChangePositionCounterToLocalCache);
            Log("{0}: Сохраняем \"Текущее значение счётчика изменения позиции\" в локальный кеш", methodName);
            var key = nameof(changePositionCounter);
            realTimeTrading.SaveObjectToContainer(key, changePositionCounter);
        }

        private void SaveChangePositionLastDealPriceToLocalCache()
        {
            var methodName = nameof(SaveChangePositionLastDealPriceToLocalCache);
            Log("{0}: Сохраняем последнюю цену, использованную в сделке, в локальный кеш", methodName);
            var key = nameof(changePositionLastDealPrice);
            realTimeTrading.SaveObjectToContainer(key, changePositionLastDealPrice);
        }

        public void CheckPositionCloseCase(int barNumber)
        {
            throw new NotImplementedException();    //это нормально. Но надо рефакторить
        }

        public void CheckPositionOpenLongCase(double lastPrice, int barNumber)
        {
            throw new NotImplementedException();    //это нормально. Но надо рефакторить
        }

        public void CheckPositionOpenShortCase(double lastPrice, int barNumber)
        {
            throw new NotImplementedException();    //это нормально. Но надо рефакторить
        }

        public bool CheckShortPositionCloseCase(IPosition se, int barNumber)
        {
            throw new NotImplementedException();    //это нормально. Но надо рефакторить
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

        public void Paint(Context context)
        {
            if (Ctx.IsOptimization || sec.Bars.Count == 0)
                return;

            var pane = Ctx.CreateGraphPane(security.Name, security.Name);

            var colorGreen = new Color(SystemColor.Green.ToArgb());
            var colorRed = new Color(SystemColor.Red.ToArgb());
            var colorBlue = new Color(SystemColor.Blue.ToArgb());

            var list = pane.AddList(id: security.Name, caption: security.Name, sec, CandleStyles.BAR_CANDLE, CandleFillStyle.Decreasing, showTrades: true, colorGreen, PaneSides.LEFT);
            for (int i = 0; i < sec.Bars.Count; i++)
            {
                var bar = sec.Bars[i];
                if (bar.Close < bar.Open)
                    list.SetColor(i, colorRed);
            }

            var emaPaint = new double[ema.Count];
            emaPaint[0] = ema[0];
            for (var i = 1; i < ema.Count; i++)
                emaPaint[i] = ema[i - 1];

            var bollingerBandPaint = new double[bollingerBand.Count];
            bollingerBandPaint[0] = bollingerBand[0];
            for (var i = 1; i < bollingerBand.Count; i++)
                bollingerBandPaint[i] = bollingerBand[i - 1];

            pane.AddList(id: ema.ToString(), caption: ema.ToString(), emaPaint, ListStyles.LINE, colorBlue, LineStyles.SOLID, PaneSides.LEFT);
            pane.AddList(id: bollingerBand.ToString(), caption: bollingerBand.ToString(), bollingerBandPaint, ListStyles.LINE, colorRed, LineStyles.SOLID, PaneSides.LEFT);
        }

        public void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide)
        {
            throw new NotImplementedException();    //это нормально. Но надо рефакторить
        }

        public void SetParameters(SystemParameters systemParameters)
        {
            periodBollingerBandAndEma = systemParameters.GetInt("periodBollingerBandAndEma");
            standartDeviationCoef = systemParameters.GetDouble("standartDeviationCoef");
            profitPercent = systemParameters.GetDouble("profitPercent");
            startLots = systemParameters.GetInt("startLots");
            maxLots = systemParameters.GetInt("maxLots");
            var useRelatedOrdersInt = systemParameters.GetInt("useRelatedOrders");
            useRelatedOrders = Convert.ToBoolean(useRelatedOrdersInt);

            parametersCombination = string.Format("Period Bollinger Band: {0}; Standart Deviation Coefficient: {1}; Profit Percent", periodBollingerBandAndEma, standartDeviationCoef, profitPercent);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
            realTimeTrading = RealTimeTrading.Create(positionSide, tradingSystemDescription: "", Ctx);
        }

        private string GetSignalNotesName()
        {
            var methodName = nameof(GetSignalNotesName);
            var result = " Вход №" + 1;
            Log("{0}: Сгенерировали заметку для позиции: {1}.", methodName, result);
            return result;
        }

        private bool IsTimeForTrading()
        {
            var methodName = nameof(IsTimeForTrading);
            Log("{0}: Проверяем, подходящее ли сейчас время для торговли?", methodName);

            var hour = security.GetBarDateTime(barNumber).Hour;
            var minute = security.GetBarDateTime(barNumber).Minute;
            var year = security.GetBarDateTime(barNumber).Year;
            var month = security.GetBarDateTime(barNumber).Month;
            var day = security.GetBarDateTime(barNumber).Day;

            var currentTime = new DateTime(year, month, day, hour, minute, second: 0);
            var stopTradingTime = new DateTime(year, month, day, hourStopTrading, minuteStopTrading, second: 0);
            var result = currentTime < stopTradingTime;
            if (result)
                Log("{0}: Да, сейчас подходящее время для торговли.", methodName);

            else
                Log("{0}: Нет, сейчас не подходящее время для торговли.", methodName);

            return result;
        }

        private bool IsPositionOpen(string notes)
        {
            var methodName = nameof(IsPositionOpen);
            Log("{0}: Запрашиваем позицию со следующей заметкой: {1}", methodName, notes);

            var position = GetPosition(notes);
            var result = position != null;

            if (result)
                Log("{0}: Да, позиция найдена.", methodName);

            else
                Log("{0}: Нет, позиция не найдена.", methodName);

            return result;
        }

        private Position GetPosition(string notes)
        {
            var methodName = nameof(GetPosition);
            var fullSignalName = signalNameForOpenPosition + notes;
            Log(methodName + ": Получить последнюю активную позицию для сигнала: " + fullSignalName + "; для бара: " + barNumber);
            var position = sec.Positions.GetLastActiveForSignal(fullSignalName, barNumber);

            if (position == null)
            {
                Log(methodName + ": Позиции нет.");
                return null;
            }

            Log(methodName + ": Позиция найдена. Состояние позиции:\r\n" + position.GetPropertiesState());
            if (position is IPositionRt)
            {
                var positionRt = position as IPositionRt;
                IEnumerable<IOrder> executedOrdersForOpenOrChangePosition = null;

                var isBuy = true;
                if (convertedLong.IsConverted)
                    isBuy = false;

                if (positionRt.EntryOrders != null)
                    executedOrdersForOpenOrChangePosition = positionRt.EntryOrders.Where(p => p.IsExecuted && p.IsBuy == isBuy);

                if (executedOrdersForOpenOrChangePosition != null && executedOrdersForOpenOrChangePosition.Count() > 0)
                {
                    lastExecutedOrderForOpenOrChangePosition = executedOrdersForOpenOrChangePosition.Last();
                    Log("{0} Последний ордер на открытие:\r\n{1}: {2}\r\n{3}: {4}\r\n{5}: {6}\r\n{7}: {8}\r\n{9}: {10}\r\n{11}: {12}\r\n{13}: {14}\r\n" +
                        "{15}: {16}\r\n{17}: {18}\r\n{19}: {20}\r\n{21}: {22}\r\n{23}: {24}\r\n{25}: {26}\r\n{27}: {28}\r\n{29}: {30}\r\n{31}: {32}\r\n" +
                        "{33}: {34}\r\n", methodName, nameof(lastExecutedOrderForOpenOrChangePosition.Id), lastExecutedOrderForOpenOrChangePosition.Id, nameof(lastExecutedOrderForOpenOrChangePosition.Price), lastExecutedOrderForOpenOrChangePosition.Price, nameof(lastExecutedOrderForOpenOrChangePosition.Quantity), lastExecutedOrderForOpenOrChangePosition.Quantity,
                        nameof(lastExecutedOrderForOpenOrChangePosition.IsExecuted), lastExecutedOrderForOpenOrChangePosition.IsExecuted, nameof(lastExecutedOrderForOpenOrChangePosition.RestQuantity), lastExecutedOrderForOpenOrChangePosition.RestQuantity, nameof(lastExecutedOrderForOpenOrChangePosition.Date), lastExecutedOrderForOpenOrChangePosition.Date,
                        nameof(lastExecutedOrderForOpenOrChangePosition.IsPriceFromTrades), lastExecutedOrderForOpenOrChangePosition.IsPriceFromTrades, nameof(lastExecutedOrderForOpenOrChangePosition.OrderPrice), lastExecutedOrderForOpenOrChangePosition.OrderPrice, nameof(lastExecutedOrderForOpenOrChangePosition.IsActive), lastExecutedOrderForOpenOrChangePosition.IsActive,
                        nameof(lastExecutedOrderForOpenOrChangePosition.Status), lastExecutedOrderForOpenOrChangePosition.Status, nameof(lastExecutedOrderForOpenOrChangePosition.IsBuy), lastExecutedOrderForOpenOrChangePosition.IsBuy, nameof(lastExecutedOrderForOpenOrChangePosition.Security), lastExecutedOrderForOpenOrChangePosition.Security,
                        nameof(lastExecutedOrderForOpenOrChangePosition.OrderType), lastExecutedOrderForOpenOrChangePosition.OrderType, nameof(lastExecutedOrderForOpenOrChangePosition.Notes), lastExecutedOrderForOpenOrChangePosition.Notes, nameof(lastExecutedOrderForOpenOrChangePosition.Comment), lastExecutedOrderForOpenOrChangePosition.Comment,
                        nameof(lastExecutedOrderForOpenOrChangePosition.Commission), lastExecutedOrderForOpenOrChangePosition.Commission, nameof(lastExecutedOrderForOpenOrChangePosition.Slippage), lastExecutedOrderForOpenOrChangePosition.Slippage);
                }
            }
            return Position.Create(position);
        }

        private int GetLots()
        {
            Log("Определяем количество лотов...");

            var lots = startLots * (int)Math.Pow(2, changePositionCounter);

            if (lots > maxLots)
                lots = maxLots;

            Log("Количество лотов: " + lots);
            return lots;
        }

        private int GetLotsForOpenPosition()
        {
            return GetLots();
        }

        private int GetLotsForChangePositionBasedOnOpenedLots(IPosition position)
        {
            var methodName = nameof(GetLotsForChangePositionBasedOnOpenedLots);
            Log("{0}: Получаем новый объём позиции, основанный на уже открытом объёме", methodName);

            var result = (int)position.Shares * multiplyCoef;

            if (result > maxLots)
                result = maxLots;

            if (convertedLong.IsConverted)
                result = -result;
            return result;
        }

        private void SetLimitOrdersForOpenPosition(string notes)
        {
            var methodName = nameof(SetLimitOrdersForOpenPosition);
            Log("{0}: Устанавливаем лимитный ордер для открытия позиции", methodName);

            if (barNumber < periodBollingerBandAndEma)
            {
                Log("{0}: Номер последнего бара меньше периода индикаторов. Устанавливать ордер не будем. Выходим из метода.", methodName);
                return;
            }

            //if (lastExecutedOrderForOpenOrChangePosition != null && lastExecutedOrderForOpenOrChangePosition.IsActive)
            //{
            //    Log("{0}: Последний исполненный ордер для открытия или изменения позиции активный. Устанавливать новый ордер не будем. Выходим из метода.", methodName);
            //    return;
            //}

            //Log("{0}: Последний исполненный ордер не существует, или он не активный.", methodName);

            var signalName = signalNameForOpenPosition + notes;
            var price = 0d;
            var lots = 0d;
            GetPriceAndLotsForOpenPosition(out price, out lots);

            if (price == 0)
            {
                price = convertedLong.Minus(price, sec.Tick);
                //Log("{0}: Цена нового ордера нулевая. Ордер выставлять не будем.", methodName);
                //return;
            }

            lastUsedPrice = price;
            Log("{0}: Выставляем лимитный ордер на открытие позиции. Номер бара = {1}, Количество лотов = {2}, Цена = {3}, Название сигнала = {4}.", methodName, barNumber + 1, lots, price, signalName);

            if (positionSide == PositionSide.Long)
                sec.Positions.BuyAtPrice(barNumber + 1, Math.Abs(lots), price, signalName);

            if (positionSide == PositionSide.Short)
                sec.Positions.SellAtPrice(barNumber + 1, Math.Abs(lots), price, signalName);
        }

        private void GetPriceAndLotsForOpenPosition(out double price, out double lots)
        {
            var methodName = nameof(GetPriceAndLotsForOpenPosition);
            //Log("{0}: Существует ли последний неактивный ордер исполненный для открытия или наращивания позиции, в котором есть неисполненные лоты?", methodName);

            //if (IsLastExecutedOrderForOpenOrChangePositionNotActiveAndHasRestQuantity())
            //{
            //    Log("{0}: Да, существует. Его Id: {1}; оставшееся количество неисполненных лотов: {2}.", methodName, lastExecutedOrderForOpenOrChangePosition.Id, lastExecutedOrderForOpenOrChangePosition.RestQuantity);
            //    GetPriceAndLotsForOrderForChangePositionIfRestQuantityGreaterThanZero(out price, out lots); 
            //}
            //else
            //{
            //Log("{0}: Нет, не существует.", methodName);
            lots = GetLotsForOpenPosition();
            price = bollingerBand[barNumber];
            //}
        }

        private void GetPriceAndLotsForOrderForChangePositionIfRestQuantityGreaterThanZero(out double price, out double lots)
        {
            var methodName = nameof(GetPriceAndLotsForOrderForChangePositionIfRestQuantityGreaterThanZero);
            Log("{0}: Получаем количество неисполненных лотов последнего неактивного ордера, исполненного для открытия или наращивания позиции, и цену, по которой можно выставить новый ордер, " +
                "исходя из цены последнего исполненного ордера.", methodName);

            Log("{0}: Цена закрытия бара {1} цены ордера?", methodName, convertedLong.Under);

            if (convertedLong.IsLessOrEqual(security.GetBarClose(barNumber), lastExecutedOrderForOpenOrChangePosition.OrderPrice))
            {
                Log("{0}: Да, цена закрытия бара {1} цены ордера.", methodName, convertedLong.Under);
                Log("{0}: Цена шага равна: {1}.", methodName, sec.Tick);
                price = convertedLong.Minus(security.GetBarClose(barNumber), sec.Tick);

            }
            else
            {
                Log("{0}: Нет, цена закрытия бара {1} цены ордера.", methodName, convertedLong.Above);
                price = lastExecutedOrderForOpenOrChangePosition.OrderPrice;
            }

            //var newPositionFinalLots = lastExecutedOrderForOpenOrChangePosition.Quantity;   //здесь ошибка
            var newPositionFinalLots = GetLotsBasedOnStartLot();
            lots = newPositionFinalLots;

            if (convertedLong.IsConverted)
                lots = -lots;
        }

        private void SetLimitOrdersForChangePosition(Position position, string notes)
        {
            var methodName = nameof(SetLimitOrdersForChangePosition);
            try
            {
                Log("{0}: Устанавливаем лимитный ордер для изменения позиции", methodName);

                //if (lastExecutedOrderForOpenOrChangePosition != null && lastExecutedOrderForOpenOrChangePosition.IsActive)
                //{
                //    Log("{0}: Последний исполненный ордер активный. Устанавливать новый ордер не будем.", methodName);
                //    return;
                //}

                var iPosition = position.iPosition;
                var signalName = signalNameForOpenPosition + notes;
                var price = 0d;
                var lots = 0d;
                GetPriceAndLotsForChangePosition(position, out price, out lots);

                if (price == 0)
                {
                    price = convertedLong.Minus(price, sec.Tick);
                    //Log("{0}: Цена нового ордера нулевая. Ордер выставлять не будем.", methodName);
                    //return;
                }

                Log("{0}: Выставляем лимитный ордер на изменение позиции. Номер бара = {1}, Цена = {2}, Новое количество лотов = {3}, Название сигнала = {4}.",
                    methodName, barNumber + 1, price, lots, signalName);
                iPosition.ChangeAtPrice(barNumber + 1, price, lots, signalName);
            }
            catch (Exception exception)
            {
                Log("{0}: Лимитный ордер для изменения позиции выставлять не будем по причине: {1}", methodName, exception.Message);
            }
        }

        private void GetPriceAndLotsForChangePosition(Position position, out double price, out double lots)
        {
            var methodName = nameof(GetPriceAndLotsForChangePosition);

            if (IsLastExecutedOrderForOpenOrChangePositionNotActiveAndHasRestQuantity())
            {
                Log("{0}: Последний исполненный ордер был исполнен не полностью.", methodName);
                GetPriceAndLotsForOrderForChangePositionIfRestQuantityGreaterThanZero(out price, out lots);
            }
            else
            {
                var iPosition = position.iPosition;
                var lotsResult = GetLotsForChangePositionBasedOnOpenedLots(iPosition);
                var changePositionIntervalPercent = GetAdaptiveTakeProfitPercent();
                var priceTakeProfit = convertedLong.Plus(currentPosition.iPosition.AverageEntryPrice, Math.Abs(changePositionIntervalPercent / 100 * iPosition.AverageEntryPrice));
                var priceLevel = convertedLong.Minus(changePositionLastDealPrice, Math.Abs(priceTakeProfit - changePositionLastDealPrice));
                if (convertedLong.IsGreater(bollingerBand[barNumber], priceLevel))
                {
                    var message = string.Format("{0}: Не выставляем ордер, потому что цена ордера находится {1} полосы Боллинджера", methodName, convertedLong.Under);
                    throw new Exception(message);
                }

                if (convertedLong.IsLessOrEqual(security.GetBarClose(barNumber), bollingerBand[barNumber]))
                {
                    var message = string.Format("{0}: Не выставляем ордер, потому что цена закрытия последнего бара {1} цены линии Боллинджера.", methodName, convertedLong.Under);
                    throw new Exception(message);
                }

                price = bollingerBand[barNumber];
                lastUsedPrice = price;
                lots = lotsResult;

                Log("{0}: Устанавливаем цену ордера, соответствующую полосе Боллинджера: {1}; и количество лотов: {2}", methodName, price, lots);
            }
        }
        /// <summary>
        /// Неактивность убрал
        /// </summary>
        /// <returns></returns>
        private bool IsLastExecutedOrderForOpenOrChangePositionNotActiveAndHasRestQuantity()
        {
            var methodName = nameof(IsLastExecutedOrderForOpenOrChangePositionNotActiveAndHasRestQuantity);
            //Log("{0}: Последний исполненный ордер не активный и был исполнен не полностью?", methodName);
            Log("{0}: Последний исполненный ордер был исполнен не полностью?", methodName);

            //var result = lastExecutedOrderForOpenOrChangePosition != null && !lastExecutedOrderForOpenOrChangePosition.IsActive && lastExecutedOrderForOpenOrChangePosition.RestQuantity > 0;
            var result = lastExecutedOrderForOpenOrChangePosition != null && lastExecutedOrderForOpenOrChangePosition.RestQuantity > 0;
            if (result)
                Log("{0}: Да.", methodName);
            else
                Log("{0}: Нет.", methodName);
            return result;
        }

        //private bool IsCurrentPositionSharesCorrect(Position position)
        //{
        //    var methodName = nameof(IsCurrentPositionSharesCorrect);
        //    //Log("{0}: Последний исполненный ордер не активный и был исполнен не полностью?", methodName);
        //    //Log("{0}: Последний исполненный ордер был исполнен не полностью?", methodName);
        //    var iPosition = position.iPosition;

        //    if (iPosition == null)
        //        return true;

        //    iPosition.Shares

        //    //var result = lastExecutedOrderForOpenOrChangePosition != null && !lastExecutedOrderForOpenOrChangePosition.IsActive && lastExecutedOrderForOpenOrChangePosition.RestQuantity > 0;
        //    var result = lastExecutedOrderForOpenOrChangePosition != null && lastExecutedOrderForOpenOrChangePosition.RestQuantity > 0;
        //    if (result)
        //        Log("{0}: Да.", methodName);
        //    else
        //        Log("{0}: Нет.", methodName);
        //    return result;
        //}

        private double GetAdaptiveTakeProfitPercent()
        {
            //var constParam = 400;
            var longEma = Series.EMA(sec.ClosePrices, period: 200);
            var longAtr = Series.AverageTrueRange(sec.Bars, period: 200);
            return constParam * longAtr[barNumber] / Math.Abs(longEma[barNumber]);
        }

        private void SetLimitOrdersForClosePosition(Position position, string notes)
        {
            var methodName = nameof(SetLimitOrdersForClosePosition);
            Log("{0}: Устанавливаем лимитный ордер для закрытия позиции", methodName);

            var price = convertedLong.Plus(position.iPosition.AverageEntryPrice, Math.Abs(GetAdaptiveTakeProfitPercent() / 100 * position.iPosition.AverageEntryPrice));
            var iPosition = position.iPosition;

            if (price == 0)
            {
                price = convertedLong.Minus(price, sec.Tick);
                //Log("{0}: Цена нового ордера нулевая. Ордер выставлять не будем.", methodName);
                //return;
            }

            var signalName = signalNameForClosePositionByTakeProfit + notes;
            Log("{0}: Выставляем лимитный ордер на закрытие позиции. Номер бара = {1}, Цена = {2}, Название сигнала = {3}.", methodName, barNumber + 1, price, signalName);
            iPosition.CloseAtPrice(barNumber + 1, price, signalName);
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

        private bool IsLaboratory()
        {
            var methodName = nameof(IsLaboratory);
            Log("{0}: Это лаборатория?.", methodName);

            var realTimeSecurity = sec as ISecurityRt;
            var result = realTimeSecurity == null;

            if (result)
                Log("{0}: Да. Это лаборатория.", methodName);

            else
                Log("{0}: Нет, это торговля в реальном времени.", methodName);

            return result;
        }

        private int GetLotsBasedOnStartLot()
        {
            var positionShares = GetCurrentSharesInt();

            if (multiplyCoef <= 1)
                throw new Exception("Коэффициент должен быть больше 1");
            var lots = startLots;
            while (positionShares > lots)
                lots *= multiplyCoef;

            return lots;
        }

        private bool IsCurrentPositionSharesCorrect()
        {           
            var positionShares = GetCurrentSharesInt();

            if (multiplyCoef <= 1)
                throw new Exception("Коэффициент должен быть больше 1");
            var lots = startLots;
            while (positionShares > lots)
                lots *= multiplyCoef;

            return positionShares == lots;
        }

        private int GetCurrentSharesInt()
        {
            if (currentPosition == null)
                return 0;

            if (currentPosition.iPosition == null)
                return 0;

            return (int)currentPosition.iPosition.Shares;
        }
    }
}