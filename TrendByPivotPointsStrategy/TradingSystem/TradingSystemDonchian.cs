using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TSLab.DataSource;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;
using TSLab.Script.Optimization;
using SystemColor = System.Drawing.Color;

namespace TradingSystems
{
    public class TradingSystemDonchian : TradingSystem
    {        
        public IContext Ctx { get; set; }
        public Logger Logger { get; set; } = new LoggerNull();
        public PositionSide PositionSide { get { return positionSide; } }

        private ISecurity sec;
        private ISecurity secCompressed;
        
        private IList<double> atr;
        private double fixedAtr;

        //Список, а не связный список: ElementAt(barNumber) по LinkedList проходит
        //его от головы, и на каждом баре это обходится всё дороже. У List тот же
        //ElementAt берёт элемент по индексу.
        private IList<double> highest;
        private IList<double> lowest;

        private int slowDonchian;
        private int fastDonchian;
        private int atrPeriod;
        private double kAtrForStopLoss;
        private double kAtrForOpenPosition = 0.5;
        private double openPositionPrice;
        private double firstPositionEntryPrice;
        private int digitsAfterPoint = 0;

        /// <summary>
        /// Через сколько баров позиция закрывается по рынку. Замеры по Si
        /// показывают, что преимущество пробоя канала проявляется на 100–400 часах,
        /// а выход по каналу срабатывает заметно раньше и до этого момента позиция
        /// не доживает.
        /// </summary>
        private int maxBarsInPosition;

        /// <summary>
        /// Включён ли выход по времени. Отдельный переключатель, а не нулевая
        /// длительность: на сетке из восьмисот узлов вариант «выключено» почти
        /// никогда не попадал бы в стартовую популяцию, а это базовое поведение,
        /// с которым сравнивают остальные.
        /// </summary>
        private bool useTimeExit;

        /// <summary>
        /// Держать ли стоп по противоположной границе канала. Он тянется за ценой
        /// и закрывает позицию раньше времени; при выключенном канале остаётся
        /// только стоп по ATR, который отвечает за риск и никуда не девается.
        /// </summary>
        private bool useChannelExit = true;

        public TradingSystemDonchian(List<Security> securities,
            ContractsManager contractsManager, Indicators indicators, Context context,
            Logger logger, List<NonTradingPeriod> nonTradingPeriods = null):
            base(securities, contractsManager, indicators, context, logger, nonTradingPeriods)
        {
            digitsAfterPoint = CountDecimalPlaces(security.Bars.Last().Close);
        }                               

        private void BuyIfGreater(double price, int contracts, string notes)
        {
            if (contracts <= 0)
                return;

            if (positionSide == PositionSide.Long)
                security.BuyIfGreater(barNumber + 1, contracts, price, signalNameForOpenPosition + notes);
            if (positionSide == PositionSide.Short)
                security.SellIfLess(barNumber + 1, contracts, price, signalNameForOpenPosition + notes);
        }

        protected override double GetStopPrice(string notes = "")
        {
            double stopPriceAtr;
            if (IsPositionOpen(notes))
            {
                var position = GetOpenedPosition(notes);
                stopPriceAtr = converter.Minus(position.EntryPrice, Math.Round(kAtrForStopLoss * fixedAtr,
                    digitsAfterPoint));
            }
            else
                stopPriceAtr = converter.Minus(highest.ElementAt(barNumber), Math.Round(kAtrForStopLoss *
                    fixedAtr, digitsAfterPoint));
            //Без канального выхода позицию держит только стоп по ATR: он дальше от
            //цены, поэтому позиция доживает до выхода по времени. Отключать канал
            //можно лишь когда ATR-стоп существует: при нулевом множителе он равен
            //цене входа и выбил бы позицию сразу.
            if (!useChannelExit && kAtrForStopLoss > 0)
                return stopPriceAtr;

            var stopPriceDonchian = lowest.ElementAt(barNumber);
            if (kAtrForStopLoss > 0)
                return converter.Maximum(stopPriceAtr, stopPriceDonchian);
            return stopPriceDonchian;
        }        

        protected override void CheckPositionOpenLongCase(int positionNumber)
        {
            var recalcReasons = context.LastRecalcReasons();
            if (recalcReasons.Any(x => x.Name == EventKind.OrderCanceled.ToString()))
                Log("Внеочередной пересчёт из-за отмены ордера!");

            if (recalcReasons.Any(x => x.Name == EventKind.OrderRejected.ToString()))
                Log("Внеочередной пересчёт из-за отклонения ордера!");

            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, converter.Long);
            double stopPrice;

            var notes = " Вход №" + (positionNumber + 1);

            if (!IsPositionOpen(notes))
            {
                Log("{0} позиция не открыта.", converter.Long);

                if (positionNumber == 0)                                    
                    fixedAtr = Math.Round(atr[barNumber], digitsAfterPoint);
                
                Log("Фиксированный ATR = {0}", fixedAtr);
                Log("Вычисляем стоп-цену...");
                stopPrice = GetStopPrice(notes);

                Log("Определяем количество контрактов...");
                var contracts = contractsManager.GetQntContracts(security, highest.ElementAt(barNumber), stopPrice, positionSide);

                Log("Торгуем в лаборатории или в режиме реального времени?");
                if (security.IsRealTimeTrading)
                {
                    Log("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                }
                else
                {
                    Log("Торгуем в лаборатории.");
                }

                if (positionNumber == 0)
                {
                    openPositionPrice = highest.ElementAt(barNumber);
                    firstPositionEntryPrice = 0;
                }
                else
                {
                    if (firstPositionEntryPrice != 0)
                        openPositionPrice = firstPositionEntryPrice;
                }

                var price = converter.Plus(openPositionPrice, Math.Round(positionNumber * fixedAtr * kAtrForOpenPosition,
                    digitsAfterPoint));
                Log("Рассчитаем цену для открытия позиции, исходя из следующих данных: {0} {1} {2} * {3} * {4} = {5}", openPositionPrice, converter.SymbolPlus, positionNumber, fixedAtr, kAtrForOpenPosition, price);

                BuyIfGreater(price, contracts, notes);

                Log("Отправляем ордер.", converter.Long);

                if (security.IsRealTimeTrading && security.IsRealTimeActualBar(barNumber))
                {
                    var sleepTimeInSec = 4;
                    Log("Ждём {0} сек.", sleepTimeInSec);
                    System.Threading.Thread.Sleep(sleepTimeInSec * 1000);
                }
            }

            else
            {
                var reasons = context.LastRecalcReasons();
                if (reasons.Any(x => x.Name == EventKind.PositionOpening.ToString()))
                    Log("Внеочередной пересчёт по открытию позиции! Надо выставлять стоп-лосс!");

                var position = GetOpenedPosition(notes);
                Log("{0} позиция открыта.", converter.Long);

                var exitNotes = " Выход №" + (positionNumber + 1);

                //Выход по времени идёт раньше стопа: если срок вышел, стоп на
                //следующий бар не выставляем, позиция закрывается по рынку.
                var barsInPosition = barNumber - position.BarNumberOpenPosition;
                if (useTimeExit && barsInPosition >= maxBarsInPosition)
                {
                    Log("Позиция держится {0} баров при пределе {1}. Закрываем по рынку.",
                        barsInPosition, maxBarsInPosition);
                    security.CloseAtMarket(barNumber + 1, signalNameForClosePosition,
                        exitNotes + " время", position);

                    if (positionNumber == 0)
                        firstPositionEntryPrice = position.EntryPrice;
                    return;
                }

                stopPrice = GetStopPrice(notes);
                notes = exitNotes;
                security.CloseAtStop(barNumber + 1, stopPrice, signalNameForClosePosition, notes, position);

                if (positionNumber == 0)
                    firstPositionEntryPrice = position.EntryPrice;
            }
        }

        private int CountDecimalPlaces(double value)
        {
            string strValue = value.ToString();
            var currentCulture = CultureInfo.CurrentCulture;            
            char decimalSeparator = currentCulture.NumberFormat.NumberDecimalSeparator[0];

            int pointIndex = strValue.IndexOf(decimalSeparator);

            if (pointIndex != -1)            
                return strValue.Length - pointIndex - 1;            

            return 0;
        }

        private int GetQtyUnits()
        {
            var units = 0;
            for (var i = limitOpenedPositions; i > 0; i--)
            {                
                var notes = " Вход №" + i;
                if (IsPositionOpen(notes))
                {
                    units = i;
                    break;
                }
            }
            return units;
        }

        public void SetParameters(SystemParameters systemParameters)
        {
            slowDonchian = (int)systemParameters.GetValue("slowDonchian");
            fastDonchian = (int)systemParameters.GetValue("fastDonchian");
            kAtrForStopLoss = (double)systemParameters.GetValue("kAtrForStopLoss");
            kAtrForOpenPosition = (double)systemParameters.GetValue("kAtrForOpenPosition");
            atrPeriod = (int)systemParameters.GetValue("atrPeriod");
            limitOpenedPositions = (int)systemParameters.GetValue("limitOpenedPositions");

            //Появились позже: без них система работает как раньше — держит позицию
            //до стопа, канальный выход включён.
            maxBarsInPosition = systemParameters.TryGetValue("maxBarsInPosition",
                out object maxBars) ? (int)maxBars : 0;
            useChannelExit = !systemParameters.TryGetValue("useChannelExit",
                out object channelExit) || (int)channelExit == 1;

            //Переключатель появился позже самой длительности: если его не передали,
            //признаком служит ненулевая длительность, как было раньше.
            useTimeExit = systemParameters.TryGetValue("useTimeExit", out object timeExit)
                ? (int)timeExit == 1
                : maxBarsInPosition > 0;

            //Нулевая длительность при включённом выходе закрывала бы позицию на том
            //же баре, на котором она открылась.
            if (useTimeExit && maxBarsInPosition < 1)
                maxBarsInPosition = 1;

            var pSide = (int)systemParameters.GetValue("positionSide");

            if (pSide == 0)
                positionSide = PositionSide.Long;
            else if (pSide == 1)
                positionSide = PositionSide.Short;
            else
                positionSide = PositionSide.Null;

            parametersCombination = string.Format("slowDonchian: {0}; fastDonchian: {1}; " +
                "kAtr: {2}; atrPeriod: {3}; useTimeExit: {4}; maxBarsInPosition: {5}; " +
                "useChannelExit: {6}", slowDonchian, fastDonchian, kAtrForStopLoss,
                atrPeriod, useTimeExit, maxBarsInPosition, useChannelExit);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
        }
        
        public override void CalculateIndicators()
        {
            nonTradingPeriod = Math.Max(slowDonchian, atrPeriod);
            highest = converter.GetHighest(converter.GetHighPrices(security).ToList(), slowDonchian).ToList();
            lowest = converter.GetLowest(converter.GetLowPrices(security).ToList(), fastDonchian).ToList();
            var candles = ConvertBarsForUsingInTsLabIndicators();
            atr = Series.AverageTrueRange(candles, atrPeriod);        
        }

        private IReadOnlyList<IDataBar> ConvertBarsForUsingInTsLabIndicators()
        {
            var candles = new ReadAndAddList<IDataBar>();
            foreach (var bar in security.Bars)            
                candles.Add(bar);
            return candles;
        }

        public void Paint(Context context)
        {
            if (context.IsOptimization)
                return;           
            
            var pane = context.CreatePane("Первая панель", 50, true);
            var colorTSlab1 = new Color(SystemColor.Blue.ToArgb());
            var colorTSlab2 = new Color(SystemColor.Green.ToArgb());
            var colorTSlab3 = new Color(SystemColor.Red.ToArgb());

            pane.AddList(security.ToString(), security, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);
            pane.AddList("Highest", highest.ToList(), ListStyles.LINE, colorTSlab2, LineStyles.SOLID, PaneSides.RIGHT);
            pane.AddList("Lowest", lowest.ToList(), ListStyles.LINE, colorTSlab3, LineStyles.SOLID, PaneSides.RIGHT);

            pane = context.CreatePane("Вторая панель", 50, true);            
            pane.AddList(security.ToString(), security, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);
            pane.AddList("Highest", highest.ToList(), ListStyles.LINE, colorTSlab2, LineStyles.SOLID, PaneSides.RIGHT);
            pane.AddList("Lowest", lowest.ToList(), ListStyles.LINE, colorTSlab3, LineStyles.SOLID, PaneSides.RIGHT);
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

        public void Initialize(IContext ctx)
        {
            Ctx = ctx;
        }                        

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void CheckPositionCloseCase(Position position, string signalNameForClosePosition, out bool isPositionClosing)
        {
            throw new NotImplementedException();
        }
    }
}