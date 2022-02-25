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
        private StopLoss stopLoss;
        private RealTimeTrading realTimeTrading;

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
                            //CheckPositionOpenShortCase();
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

        private void SetLastLowForOpenLongPosition()
        {
            lastLowForOpenLongPosition = lastLow;
        }

        private bool IsLastLowCaseLongCloseNotExist()
        {
            return lastLowForOpenLongPosition == null;
        }

        private bool IsLastLowCaseLongAlreadyUsed()
        {
            return lastLow.BarNumber == lastLowForOpenLongPosition.BarNumber;
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
                    if (IsLastLowCaseLongCloseNotExist() || !IsLastLowCaseLongAlreadyUsed())    //2
                    {
                        if (IsLastLowCaseLongCloseNotExist())
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
                                    stopLoss.CreateStopLossLong();
                                    realTimeTrading.SetFlagNewPositionOpened();
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
                stopLoss.UpdateStopLossLongPosition(barNumber, lows, lastLow, le);
                CheckLongPositionCloseCase(le, barNumber);
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
                    if (realTimeTrading.WasNewPositionOpened())
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
            realTimeTrading = RealTimeTrading.Create(positionSide, tradingSystemDescription, Ctx);            
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
            stopLoss = StopLoss.Create(parametersCombination, security, positionSide, atr, pivotPointBreakDownSide, realTimeTrading);// параметры стратегии здесь ещё неизвестны. Перенести эту строку в инициализацию.
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

                if (Ctx.IsLastBarClosed)
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
                    if (realTimeTrading.WasNewPositionOpened())
                    {
                        Logger.Log(string.Format("Да, была"));
                        Logger.Log(string.Format("Создаём стоп-лосс"));
                        CreateStopLoss(lastBarNumber);

                        Logger.Log(string.Format("Сбрасываем признак того, что была открыта новая позиция"));
                        realTimeTrading.ResetFlagNewPositionOpened();
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
            realTimeTrading.SaveBarToContainer(bar.Date);
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

        public bool CheckShortPositionCloseCase(IPosition se, int barNumber)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IContext ctx)
        {
            Ctx = ctx;
        }
    }
}