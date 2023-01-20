using System;
using System.Collections.Generic;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.Helpers;
using TSLab.Script.Handlers;
using System.Linq;
using System.Diagnostics.Contracts;
using TSLab.Script.Realtime;
using TrendByPivotPoints;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class TradingSystemBollingerBands : TradingStrategy
    {
        public Logger Logger { get; set; } = new NullLogger();
        public PositionSide PositionSide { get { return positionSide; } }
        public IContext Ctx { get; set; }

        private PositionSide positionSide;
        private int barNumber;
        private Converter convertable;
        private string signalNameForOpenPosition = string.Empty;
        private string signalNameForClosePositionByTakeProfit = string.Empty;
        private string tradingSystemDescription;
        private string name = nameof(TradingSystemBollingerBands);
        private string parametersCombination;
        private ISecurity sec;
        private Security security;

        private int periodBollingerBandAndEma;
        private double standartDeviationCoef;
        private double profitPercent;
        private IList<double> ema;
        private IList<double> bollingerBand;
        private bool isPriceCrossedEmaAfterOpenOrChangePosition;

        private int startLots;
        private int lastUsedLots = 1;
        private int limitLots = int.MaxValue;

        private int hourStopTrading = 23;
        private int minuteStopTrading = 45;

        private int currentOpenedShares = 0;
        private int changePositionCounter = 0;
        private RealTimeTrading realTimeTrading;
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
            switch (positionSide)
            {
                case PositionSide.Long:
                    {
                        signalNameForOpenPosition = "LE";
                        signalNameForClosePositionByTakeProfit = "LXT";
                        convertable = new Converter(isConverted: false);
                        break;
                    }
                case PositionSide.Short:
                    {
                        signalNameForOpenPosition = "SE";
                        signalNameForClosePositionByTakeProfit = "SXT";
                        convertable = new Converter(isConverted: true);
                        break;
                    }
            }
            ema = Series.EMA(sec.ClosePrices, periodBollingerBandAndEma);
            bollingerBand = Series.BollingerBands(sec.ClosePrices, ema, periodBollingerBandAndEma, standartDeviationCoef, isTopLine: convertable.IsConverted);
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

                CheckPositionOpenLongCase();
            }

            catch (Exception e)
            {
                Log("Исключение в методе Update(): " + e.ToString());
            }
        }

        public void CheckPositionOpenLongCase()
        {
            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, convertable.Long);
            var notes = GetSignalNotesName();

            if (!IsPositionOpen(notes))
            {
                Log("{0} позиция не открыта.", convertable.Long);
                if (!IsTimeForTrading())
                    return;

                changePositionCounter = 0;
                currentOpenedShares = 0;
                if (security.IsRealTimeTrading && security.IsRealTimeActualBar(barNumber))
                {
                    SaveChangePositionCounterToLocalCache();
                    SaveCurrentOpenedSharesToLocalCache();
                }
                
                SetLimitOrdersForOpenPosition(notes);
            }
            else
            {
                Log("{0} позиция открыта.", convertable.Long);
                var currentPosition = GetPosition(notes);

                UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition();
                UpdateParametersOfCurrentPosition(currentPosition);

                var currentPrice = security.GetBarClose(barNumber);
                var priceTakeProfit = convertable.Plus(currentPosition.iPosition.AverageEntryPrice,
                    profitPercent / 100 * currentPosition.iPosition.AverageEntryPrice);
                var priceChangePosition = bollingerBand[barNumber];

                var isTakeProfitPriceNearestThanChangePositionPriceForCurrentPrice = 
                    Math.Abs(priceTakeProfit - currentPrice) <=
                Math.Abs(priceChangePosition - currentPrice);

                if (!isTakeProfitPriceNearestThanChangePositionPriceForCurrentPrice &&
                    isPriceCrossedEmaAfterOpenOrChangePosition)                
                    SetLimitOrdersForChangePosition(currentPosition, notes);                
                else
                    SetLimitOrdersForClosePosition(currentPosition, notes);
            }
        }

        private void UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition()
        {
            Log(nameof(UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition) + ": Обновляю флаг \" Пересечение цены EMA после открытия" +
                " или изменения позиции\". Текущее состояние флага: " + isPriceCrossedEmaAfterOpenOrChangePosition);

            if (convertable.IsGreater(security.GetBarClose(barNumber), ema[barNumber]))
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
                    changePositionCounter = LoadChangePositionCounterFromLocalCache();
                }
                catch (KeyNotFoundException) 
                {
                    Log("{0}: Получено исключение обновления значений. Оставляем прежние значения", methodName);
                }
            }
            
            Log("{0}: Значение, хранящееся в позиции: {1}", methodName, currentPosition.iPosition.Shares);

            if (currentPosition.iPosition.Shares != currentOpenedShares)
            {
                Log("{0}: Обновляем значение, сбрасываем флаг, наращиваем счётчик", methodName);
                currentOpenedShares = (int)currentPosition.iPosition.Shares;
                isPriceCrossedEmaAfterOpenOrChangePosition = false;
                changePositionCounter++;

                SaveCurrentOpenedSharesToLocalCache();
                SaveFlagIsPriceCrossedEmaAfterOpenOrChangePositionToLocalCache();
                SaveChangePositionCounterToLocalCache();
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
            for (var i = 1; i < ema.Count;i++)            
                emaPaint[i]= ema[i - 1];

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

            parametersCombination = string.Format("Period Bollinger Band: {0}; Standart Deviation Coefficient: {1}; Profit Percent", periodBollingerBandAndEma, standartDeviationCoef, profitPercent);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
            realTimeTrading = RealTimeTrading.Create(positionSide, tradingSystemDescription, Ctx);
        }

        private string GetSignalNotesName()
        {
            return " Вход №" + 1;
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
            var position = GetPosition(notes);
            return position != null;
        }

        private Position GetPosition(string notes)
        {
            var fullSignalName = signalNameForOpenPosition + notes;
            Log(nameof(GetPosition) + ": Получить последнюю активную позицию для сигнала: " + fullSignalName + "; для бара: " + barNumber);
            var position = sec.Positions.GetLastActiveForSignal(fullSignalName, barNumber);
            
            if (position == null)
            {
                Log(nameof(GetPosition) + ": Позиции нет.");
                return null; 
            }

            Log(nameof(GetPosition) + ": Позиция найдена. Состояние позиции:\r\n"+position.GetPropertiesState());
            return Position.Create(position);
        }

        private int GetLots()
        {
            Log("Определяем количество лотов...");

            var lots = startLots * (int)Math.Pow(2, changePositionCounter);
            
            if (lots > limitLots)
                lots = limitLots;

            Log("Количество лотов: " + lots);
            return lots;
        }

        private int GetLotsForOpenPosition()
        {
            lastUsedLots = 0;
            return GetLots();
        }

        private int GetLotsForChangePosition(IPosition position)
        {
            var newLots = GetLots();
            var result = (int)position.Shares + newLots;
            if (convertable.IsConverted)
                result = -result;
            return result;
        }

        private void SetLimitOrdersForOpenPosition(string notes)
        {
            if (barNumber < periodBollingerBandAndEma)
                return;

            var lots = GetLotsForOpenPosition();

            if (positionSide == PositionSide.Long)
                sec.Positions.BuyAtPrice(barNumber + 1, lots, bollingerBand[barNumber], signalNameForOpenPosition + notes);

            if (positionSide == PositionSide.Short)
                sec.Positions.SellAtPrice(barNumber + 1, lots, bollingerBand[barNumber], signalNameForOpenPosition + notes);
        }

        private void SetLimitOrdersForChangePosition(Position position, string notes)
        {
            var iPosition = position.iPosition;
            var lots = GetLotsForChangePosition(iPosition);
            iPosition.ChangeAtPrice(barNumber + 1, bollingerBand[barNumber], lots, signalNameForOpenPosition + notes);
        }

        private void SetLimitOrdersForClosePosition(Position position, string notes)
        {
            var price = convertable.Plus(position.iPosition.AverageEntryPrice, profitPercent/100 * position.iPosition.AverageEntryPrice);
            var iPosition = position.iPosition;
            iPosition.CloseAtPrice(barNumber + 1, price, signalNameForClosePositionByTakeProfit + notes);
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
    }
}