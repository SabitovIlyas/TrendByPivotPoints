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
        private string signalNameForClosePosition = "";
        private double lastPrice;

        public PositionSide PositionSide { get { return positionSide; } }

        private string tradingSystemDescription;
        private string name = "TradingSystemPivotPointsEMAtest";
        private string parametersCombination;
        private StopLossExtremums stopLoss;
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
                            signalNameForClosePosition = "LXE";
                            convertable = new Converter(isConverted: false);
                            GetLows();
                            CheckPositionOpenLongCase();
                            break;
                        }
                    case PositionSide.Short:
                        {
                            signalNameForOpenPosition = "SE";
                            signalNameForClosePosition = "SXE";
                            convertable = new Converter(isConverted: true);
                            GetLows();
                            CheckPositionOpenLongCase();
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

        private void OpenPositionAtPrice(int contracts)
        {
            var price = convertable.Plus(lastPrice, atr[barNumber]);

            if (positionSide == PositionSide.Long)                        
                sec.Positions.BuyAtPrice(barNumber + 1, contracts, price, signalNameForOpenPosition);            
            if (positionSide == PositionSide.Short)                            
                sec.Positions.SellAtPrice(barNumber + 1, contracts, price, signalNameForOpenPosition);            
        }

        private void OpenPositionAtMarket(int contracts)
        {
            if (positionSide == PositionSide.Long)
                sec.Positions.BuyAtMarket(barNumber + 1, contracts, signalNameForOpenPosition);
            if (positionSide == PositionSide.Short)
                sec.Positions.SellAtMarket(barNumber + 1, contracts, signalNameForOpenPosition);
        }        

        public void CheckPositionOpenLongCase()
        {
            var le = sec.Positions.GetLastActiveForSignal(signalNameForOpenPosition, barNumber); //убрать в перспективе
            breakdownLong = (pivotPointBreakDownSide / 100) * atr[barNumber]; //убрать в перспективе

            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, convertable.Long);
            if (!IsPositionOpen())
            {
                Log("{0} позиция не открыта.", convertable.Long);

                Log("Выполняется ли условие двух последовательных {0}ихся {1}ов?", convertable.Rising, convertable.Minimum);
                if (IsLastMinGreaterThanPrevious())   //1
                {
                    Log("Да, выполняется: последний {0} б. №{1}: {2} {3} предыдущего б. №{4}: {5}.", convertable.Minimum, lastLow.BarNumber, lastLow.Value, convertable.Above, prevLastLow.BarNumber, prevLastLow.Value);

                    Log("Использовался ли последний {0} в попытке открыть {1} позицию ранее?", convertable.Minimum, convertable.Long);
                    if (IsLastLowCaseLongCloseNotExist() || !IsLastLowCaseLongAlreadyUsed())    //2
                    {
                        if (IsLastLowCaseLongCloseNotExist())
                            Log("Последняя попытка открыть {0} позицию не обнаружена.", convertable.Long);

                        else
                            Log("Нет, не использовался. Последний {0}, который использовался в попытке открыть {1} позицию ранее -- б. №{2}: {3}.",
                                convertable.Minimum, convertable.Long, lastLowForOpenLongPosition.BarNumber, lastLowForOpenLongPosition.Value);

                        Log("Не отсеивается ли потенциальная сделка фильтром EMA?");
                        if (IsLastPriceGreaterEma()) //3
                        {
                            Log("Нет, потенциальная сделка не отсеивается фильтром. Последняя цена закрытия {0} {1} EMA: {2}. ", lastPrice, convertable.Above, ema[barNumber]);

                            Log("Вычисляем стоп-цену...");
                            //var stopPrice = lastLow.Value - breakdownLong;
                            var stopPrice = convertable.Minus(lastLow.Value, breakdownLong);

                            Log("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2}; стоп-лосс = последний {3} {4} {6} допустимый уровень пробоя {2} = {5}. ",
                                atr[barNumber], pivotPointBreakDownSide, breakdownLong, convertable.Minimum, lastLow.Value, stopPrice, convertable.SymbolMinus);

                            Log("Последняя цена {0} стоп-цены?",convertable.Above);
                            if (convertable.IsGreater(lastPrice, stopPrice))  //4
                            {
                                Log("Да, последняя цена ({0}) {1} стоп-цены ({2}). Открываем {3} позицию...", lastPrice, convertable.Above, stopPrice, convertable.Long);

                                Log("Определяем количество контрактов...");
                                var contracts = localMoneyManager.GetQntContracts(lastPrice, stopPrice, positionSide);

                                Log("Торгуем в лаборатории или в режиме реального времени?");
                                if (security.IsRealTimeTrading)
                                {
                                    contracts = 1;
                                    Log("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                                    OpenPositionAtPrice(contracts);
                                }
                                else
                                {
                                    Log("Торгуем в лаборатории.");
                                    OpenPositionAtMarket(contracts);
                                }

                                stopLossLong = stopPrice;

                                Log("Проверяем актуальный ли это бар.");
                                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                                {
                                    Log("Бар актуальный.");
                                    stopLoss.CreateStopLoss(stopLossLong, breakdownLong);
                                    realTimeTrading.SetFlagNewPositionOpened();
                                }
                                else
                                    Log("Бар не актуальный.");

                                Log("Открываем {0} позицию! Отправляем ордер.", convertable.Long);
                            }
                            else
                                Log("Последняя цена {0} стоп-цены. {1} позицию не открываем.", convertable.Under, convertable.Long);

                            Log("Запоминаем {0}, использовавшийся для попытки открытия {1} позиции.", convertable.Minimum, convertable.Long);
                            SetLastLowForOpenLongPosition();
                        }
                        else
                            Log("Cделка отсеивается фильтром, так как последняя цена закрытия {0} {1} или совпадает с EMA: {2}.", lastPrice, convertable.Under, ema[barNumber]);
                    }
                    else
                        Log("Да, последний {0} использовался в попытке открыть {1} позицию ранее.", convertable.Minimum, convertable.Long);
                }
                else
                    Log("Нет, не выполняется: последний {0} б. №{1}: {2} не {3} предыдущего б. №{4}: {5}.",
                        convertable.Minimum, lastLow.BarNumber, lastLow.Value, convertable.Above, prevLastLow.BarNumber, prevLastLow.Value);
            }
            else
            {
                Log("{0} позиция открыта.", convertable.Long);
                stopLoss.UpdateStopLossLongPosition(barNumber, lastLow, le);
                CheckPositionCloseCase(le, barNumber);
            }
        }                                      

        public void CheckPositionCloseCase(IPosition position, int barNumber)
        {
            security.BarNumber = barNumber;            
            var bar = security.LastBar;
            var barLow = convertable.GetBarLow(bar);

            if (IsPositionOpen())
            {
                Log("Проверяем пробил ли {0} последнего бара стоп-лосс для {1}?", convertable.Minimum, convertable.Long);
                if (convertable.IsLessOrEqual(barLow, stopLossLong))
                {
                    Log("Да, пробил");
                    
                    Log("Открыта ли новая необработанная позиция?");
                    if (realTimeTrading.WasNewPositionOpened())
                    {
                        Log("Открыта новая необработанная позиция. Закрываем позицию на текущем баре.");
                        position.CloseAtMarket(barNumber, signalNameForClosePosition);
                    }
                    else
                    {
                        Log("Необработанной позиции нет. Закрываем позицию на следующем баре.");
                        position.CloseAtMarket(barNumber + 1, signalNameForClosePosition);
                    }                    
                }
            }

            else
            {
                Log("Нет, {0} последнего бара {1} {2} стоп-лосса для {3} {4}. Оставляем позицию.", convertable.Minimum, barLow, convertable.Above, convertable.Long, stopLossLong);                
            }
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
            stopLoss = StopLossExtremums.Create(parametersCombination, security, positionSide, atr, pivotPointBreakDownSide, realTimeTrading);            
            stopLoss.Logger = Logger;
            stopLoss.ctx = Ctx;
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

                        //Logger.Log(string.Format("Сбрасываем признак того, что была открыта новая позиция"));
                        //realTimeTrading.ResetFlagNewPositionOpened();
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

        public void CheckPositionCloseCase(int barNumber)
        {
            throw new NotImplementedException();
        }

        public void SetParameters(SystemParameters systemParameters)
        {
            throw new NotImplementedException();
        }
    }
}