using System;
using System.Collections.Generic;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.Helpers;
using TSLab.Script.Handlers;
using System.Linq;
using System.Diagnostics.Contracts;
using TSLab.DataSource;
using System.IO;
using System.Threading;

namespace TradingSystems
{
    public class TradingSystemScalper : TradingStrategy
    {
        public Logger Logger { get; set; } = new NullLogger();
        public PositionSide PositionSide { get { return positionSide; } }
        public IContext Ctx { get; set; }

        private PositionSide positionSide;
        private int barNumber;
        private Converter convertable;
        private string signalNameForOpenPosition = string.Empty;
        private string signalNameForClosePositionByStopLoss = string.Empty;
        private string signalNameForClosePositionByTakeProfit = string.Empty;
        private string signalNameForClosePositionByTime = string.Empty;
        private string tradingSystemDescription;
        private string name = "TradingSystemScalper";
        private string parametersCombination;

        private ISecurity sec;
        private ISecurity secCompressed;
        private ISecurity secCompressedFilter;
        private Security security;

        private IList<double> atr;
        private IList<double> rsi;
        private IList<double> adx;
        private IList<double> dip;
        private IList<double> dim;

        private int periodRsiAndAtr;
        private int bandRsi;
        private int periodAdx;
        private int bandAdx;


        private int limitOpenedPositions = 1;
        
        private int firstPositionLots = 1;          //8
        private int secondPositionLots = 0;         //1
        private int thirdPositionLots = 0;          //1
        private int[] openPositionLots;

        private double firstTakeLevelAtr = 3;       //0.5
        private double secondTakeLevelAtr = 1.5;    //1.5
        private double thirdTakeLevelAtr = 3;       //3
        private double[] positionTakeLevelsAtr;

        private double firstStopLevelAtr = -2;      //-2
        private double secondStopLevelAtr = -1.5;   //-1.5
        private double thirdStopLevelAtr = -1;      //-1
        private double[] positionStopLevelsAtr;

        private int intervalToCompressInMinutes = 60;
        private int filterIntervalToCompressInMinutes = 540;

        private int lastUsedLots = 1;

        private double fixedAtr;
        private int hourStopTrading = 23;
        private int minuteStopTrading = 45;
        public TradingSystemScalper(Security security, PositionSide positionSide)
        {
            var securityTSLab = security as TSLabSecurity;
            sec = securityTSLab.security;

            this.security = security;
            secCompressed = sec.CompressTo(intervalToCompressInMinutes);
            secCompressedFilter = sec.CompressTo(filterIntervalToCompressInMinutes);
            this.positionSide = positionSide;

            openPositionLots = new int[limitOpenedPositions];
            openPositionLots[0] = firstPositionLots;
            //openPositionLots[1] = secondPositionLots;
            //openPositionLots[2] = thirdPositionLots;

            positionTakeLevelsAtr = new double[limitOpenedPositions];
            positionTakeLevelsAtr[0] = firstTakeLevelAtr;
            //positionTakeLevelsAtr[1] = secondTakeLevelAtr;
            //positionTakeLevelsAtr[2] = thirdTakeLevelAtr;

            positionStopLevelsAtr = new double[limitOpenedPositions];
            positionStopLevelsAtr[0] = firstStopLevelAtr;
            //positionStopLevelsAtr[1] = secondStopLevelAtr;
            //positionStopLevelsAtr[2] = thirdStopLevelAtr;
        }

        public void CalculateIndicators()
        {
            switch (positionSide)
            {
                case PositionSide.Long:
                    {
                        signalNameForOpenPosition = "LE";
                        signalNameForClosePositionByStopLoss = "LXS";
                        signalNameForClosePositionByTakeProfit = "LXT";
                        signalNameForClosePositionByTime = "LXD";
                        convertable = new Converter(isConverted: false);
                        break;
                    }
                case PositionSide.Short:
                    {
                        signalNameForOpenPosition = "SE";
                        signalNameForClosePositionByStopLoss = "SXS";
                        signalNameForClosePositionByTakeProfit = "SXT";
                        signalNameForClosePositionByTime = "SXD";
                        convertable = new Converter(isConverted: true);
                        break;
                    }
            }


            atr = Series.AverageTrueRange(secCompressed.Bars, periodRsiAndAtr);
            atr = secCompressed.Decompress(atr);
            rsi = Series.RSI(secCompressed.ClosePrices, periodRsiAndAtr);
            rsi = secCompressed.Decompress(rsi);
            
            dip = ADXHelper.CalcDIP(secCompressedFilter, periodAdx);
            dim = ADXHelper.CalcDIM(secCompressedFilter, periodAdx);
            adx = ADXHelper.CalcADX(dip, dim, periodAdx);

            //var list = new List<double>();
            //var result = 0d;
            //for (int i = 0; i < dip.Count; i++)
            //{
            //    if (dip[i] == 0.0 && dim[i] == 0.0)
            //        result = 0d;
            //    else
            //        result = (Math.Abs(dip[i] - dim[i]) / (dip[i] + dim[i])) * 100.0;

            //    list.Add(result);
            //}

            //adx = Series.EMA(list, periodAdx);

            dip = secCompressedFilter.Decompress(dip);
            dim = secCompressedFilter.Decompress(dim);
            adx = secCompressedFilter.Decompress(adx);            
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

        public void Initialize(IContext ctx)
        {
            Ctx = ctx;
        }

        public void Paint(Context context)
        {
            //if (Ctx.IsOptimization)
            //    return;

            //var pane = Ctx.CreatePane("Первая панель", 50, true);
            //var colorTSlab1 = new Color(SystemColor.Blue.ToArgb());

            //pane.AddList(secCompressed.ToString(), secCompressed, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);

            //pane = Ctx.CreatePane("Вторая панель", 50, true);
            //pane.AddList(sec.ToString(), sec, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);

            ////pane = Ctx.CreatePane("RSI", 50, true);
            ////pane.AddList(rsi.ToString(), sec, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);
            ////pane.AddList()
        }

        public void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide)
        {
            throw new NotImplementedException();    //это нормально. Но надо рефакторить
        }

        public void SetParameters(SystemParameters systemParameters)
        {            
            periodRsiAndAtr = systemParameters.GetInt("periodRsiAndAtr");
            bandRsi = systemParameters.GetInt("bandRsi");
            periodAdx = systemParameters.GetInt("periodAdx");
            bandAdx = systemParameters.GetInt("bandAdx");
            parametersCombination = string.Format("Period: {0}; RSI band: {1}", periodRsiAndAtr, bandRsi);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
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

                //CheckPositionOpenLongCase();
                CheckPositionOpenLongCaseTestEvents();
            }

            catch (Exception e)
            {
                Log("Исключение в методе Update(): " + e.ToString());
            }
        }

        private void CheckPositionOpenLongCaseTestEvents()
        {
            //Log("Попробую записать информацию в текстовый файла.");
            //var path = System.IO.Directory.GetCurrentDirectory();
            //Log("Текущая директория: {0}.", path);

            //var file = "testIO.txt";
            //StreamWriter sw = new StreamWriter(file);
            //sw.WriteLine("1-ая тестовая строка");
            //sw.WriteLine("2-ая тестовая строка");
            //sw.Close();

            var listReasons = Ctx.Runtime.LastRecalcReasons.Select(x => x.Name).Distinct().ToList();
            var listReasonsSt = string.Join(", ", listReasons);
            Log($"RecalcReasons: " + listReasonsSt, MessageType.Info, true);

            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, convertable.Long);
            var notes = GetSignalNotesName(0);
            if (!IsPositionOpen(notes))
            {
                Log("{0} позиция не открыта.", convertable.Long);

                Log("Есть ли сигнал?");
                if (security.GetBar(barNumber).Date.Minute != new DateTime(2024, 02, 13, 14,30,00).Minute)
                {
                    Log("Нет, сигнала нет.");
                    return;
                }

                Log("Да, появился сигнал!");
                sec.Positions.BuyAtMarket(barNumber + 1, 1, signalNameForOpenPosition + notes);
            }
            else
            {
                var realTimeTrading = RealTimeTrading.Create(PositionSide.Long, nameof(TradingSystemScalper), Ctx);
                if (Ctx.Runtime.LastRecalcReasons.Any(x => x.Name == EventKind.PositionOpening.ToString()))
                {
                    Log("Позиция открыта! Надо выставлять стоп-лосс!");                    
                    realTimeTrading.SaveObjectToContainer("Количество юнитов", 1);

                    //Нужен DataSourceHelper
                    //var cache = new BinaryCache<BidAskBar>(Folder, CacheName);
                    //cache.SaveCached("BTCUSDT", "1M", bars, BaseBar.CacheVersion);
                }

                if (Ctx.Runtime.LastRecalcReasons.Any(x => x.Name == EventKind.PositionClosing.ToString()))
                {
                    Log("Позиция закрыта!");                    
                    realTimeTrading.SaveObjectToContainer("Количество юнитов", 0);
                }
            }            
        }

        public void CheckPositionOpenLongCase()
        {            
            for (var i = 0; i < limitOpenedPositions; i++)
                CheckPositionOpenLongCase(i);
        }

        public void CheckPositionOpenLongCase(int positionNumber)
        {
            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, convertable.Long);
            var notes = GetSignalNotesName(positionNumber);
            

            if (!IsPositionOpen(notes))
            {
                if (!IsTimeForTrading())
                    return;

                Log("{0} позиция не открыта.", convertable.Long);
                
                Log("Есть ли сигнал?");
                if (!GotSignal())
                {
                    Log("Нет, сигнала нет.");
                    return;
                }

                Log("Да, появился сигнал!");

                if (positionNumber == 0)
                    fixedAtr = atr[barNumber];

                Log("Определяем количество контрактов...");
                var contracts = GetQntContracts(positionNumber);
                Log("Количество контрактов: " + contracts);

                Log("Торгуем в лаборатории или в режиме реального времени?");
                if (security.IsRealTimeTrading)
                    Log("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                else
                    Log("Торгуем в лаборатории.");

                SetOrderForOpenPosition(contracts, notes);
            }
            else
            {
                Log("{0} позиция открыта.", convertable.Long);
                var currentPosition = GetPosition(notes);

                if (!IsTimeForTrading())
                {
                    Log("Закрываем позицию по окончанию дня: {0}", currentPosition);
                    ClosePositionsByTime(currentPosition, notes);
                    return;
                }
                SetLimitOrdersForClosePosition(positionNumber, currentPosition, notes);
                SetStopLoss(currentPosition, notes);                 
            }
        }

        private string GetSignalNotesName(int positionNumber)
        {
            return " Вход №" + (positionNumber + 1);
        }

        private bool IsTimeForTrading()
        {
            return true;

            var hour = security.GetBarDateTime(barNumber).Hour;
            var minute = security.GetBarDateTime(barNumber).Minute;
            var year = security.GetBarDateTime(barNumber).Year;
            var month = security.GetBarDateTime(barNumber).Month;
            var day = security.GetBarDateTime(barNumber).Day;

            var currentTime = new DateTime(year, month, day, hour, minute, second: 0);
            var stopTradingTime = new DateTime(year, month, day, hourStopTrading, minuteStopTrading, second: 0);
            return currentTime < stopTradingTime;
        }

        private bool IsPositionOpen(string notes)
        {
            var position = GetPosition(notes);
            return position != null;
        }

        private Position GetPosition(string notes)
        {               
            var position = sec.Positions.GetLastActiveForSignal(signalNameForOpenPosition + notes, barNumber);
            if (position == null)
                return null;
            return Position.Create(position);
        }

        private int GetPositionCount()
        {
            var positions = sec.Positions;
            var activePositions = 0;
            for (var i = 0; i < limitOpenedPositions; i++)
            {
                if (positions.GetLastActiveForSignal(signalNameForOpenPosition + GetSignalNotesName(positionNumber: i), barNumber) != null)
                    activePositions++;
            }
            return activePositions;
        }

        private int GetQntContracts(int positionNumber)
        {
            var lastClosedPosition = sec.Positions.GetLastPositionClosed(barNumber);

            int lots;

            if (lastClosedPosition != null)
            {
                if (lastClosedPosition.Profit() <= 0)
                {
                    lots = lastUsedLots * 1;//12
                    //if (lots > 8)
                    //    lots = 1;
                }
                else
                {
                    lots = openPositionLots[positionNumber];
                }
            }
            else
            {
                lots = openPositionLots[positionNumber];
            }            

            lastUsedLots = lots;
            
            return lots;
        }

        private bool GotSignal()
        {
            if (barNumber < (periodRsiAndAtr + 1) * intervalToCompressInMinutes)
                return false;
            var wasSignal = convertable.IsLess(rsi[barNumber - 1], bandRsi) && convertable.IsGreaterOrEqual(rsi[barNumber], bandRsi);
            var wasPassedFilter = WasPassedFilter();
            return wasSignal && wasPassedFilter;
        }

        private bool WasPassedFilter()
        {
            return convertable.IsGreaterOrEqual(adx[barNumber], bandAdx) && convertable.IsGreater(dip[barNumber], dim[barNumber]);
        }

        private void SetOrderForOpenPosition(int contracts, string notes)
        { 
            if (positionSide == PositionSide.Long)
                sec.Positions.BuyAtMarket(barNumber + 1, contracts, signalNameForOpenPosition + notes);

            if (positionSide == PositionSide.Short)
                sec.Positions.SellAtMarket(barNumber + 1, contracts, signalNameForOpenPosition + notes);
        }

        private void SetLimitOrdersForClosePosition(int positionNumber, Position position, string notes)
        {            
            var price = convertable.Plus(position.EntryPrice, positionTakeLevelsAtr[positionNumber] * fixedAtr);
            var iPosition = position.iPosition;
            iPosition.CloseAtPrice(barNumber + 1, price, signalNameForClosePositionByTakeProfit + notes);
        }
        
        private void SetStopLoss(Position position, string notes)
        {
            var count = GetPositionCount();

            var price = convertable.Plus(position.EntryPrice, positionStopLevelsAtr[limitOpenedPositions-count] * fixedAtr);
            var iPosition = position.iPosition;
            iPosition.CloseAtStop(barNumber + 1, price, signalNameForClosePositionByStopLoss + notes);            
        }

        private void ClosePositionsByTime(Position position, string notes)
        {
            var iPosition = position.iPosition;
            iPosition.CloseAtMarket(barNumber + 1, signalNameForClosePositionByTime + notes);
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
    }
}