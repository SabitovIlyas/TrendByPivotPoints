using System;
using System.Collections.Generic;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.Helpers;
using TSLab.Script.Handlers;
using TSLab.DataSource;
using System.Linq;
using TSLab.Script.Optimization;

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

        private IList<double> highest;
        private IList<double> lowest;

        private int slowDonchian;
        private int fastDonchian;
        private int atrPeriod;
        private double kAtrForStopLoss;
        private double kAtrForOpenPosition = 0.5;
        private double openPositionPrice;
        private double firstPositionEntryPrice;

        public TradingSystemDonchian(List<Security> securities, 
            ContractsManager contractsManager, Indicators indicators, Context context,
            Logger logger) :
            base(securities, contractsManager, indicators, context, logger)
        {            
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
                stopPriceAtr = converter.Minus(position.EntryPrice, kAtrForStopLoss * fixedAtr);                
            }
            else
                stopPriceAtr = converter.Minus(highest[barNumber], kAtrForStopLoss * fixedAtr);
            var stopPriceDonchian = lowest[barNumber];
            if (kAtrForStopLoss > 0)
                return converter.Maximum(stopPriceAtr, stopPriceDonchian);
            return stopPriceDonchian;
        }        

        protected override void CheckPositionOpenLongCase(int positionNumber)
        {
            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, converter.Long);
            double stopPrice;

            var notes = " Вход №" + (positionNumber + 1);

            if (!IsPositionOpen(notes))
            {
                Log("{0} позиция не открыта.", converter.Long);

                if (positionNumber == 0)
                    fixedAtr = atr[barNumber];

                Log("Фиксированный ATR = {0}", fixedAtr);

                Log("Вычисляем стоп-цену...");
                stopPrice = GetStopPrice(notes);

                Log("Определяем количество контрактов...");
                var contracts = contractsManager.GetQntContracts(security, highest[barNumber], stopPrice, positionSide);

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
                    openPositionPrice = highest[barNumber];
                    firstPositionEntryPrice = 0;
                }
                else
                {
                    if (firstPositionEntryPrice != 0)
                        openPositionPrice = firstPositionEntryPrice;
                }

                var price = converter.Plus(openPositionPrice, positionNumber * fixedAtr * kAtrForOpenPosition);
                Log("Рассчитаем цену для открытия позиции, исходя из следующих данных: {0} {1} {2} * {3} * {4} = {5}", openPositionPrice, converter.SymbolPlus, positionNumber, fixedAtr, kAtrForOpenPosition, price);

                BuyIfGreater(price, contracts, notes);

                Log("Отправляем ордер.", converter.Long);
            }

            else
            {
                var reasons = context.LastRecalcReasons();
                if (reasons.Any(x => x.Name == EventKind.PositionOpening.ToString()))                
                    Log("Внеочередной пересчёт по открытию позиции! Надо выставлять стоп-лосс!");

                var position = GetOpenedPosition(notes);
                Log("{0} позиция открыта.", converter.Long);
                stopPrice = GetStopPrice(notes);
                notes = " Выход №" + (positionNumber + 1);
                security.CloseAtStop(barNumber + 1, stopPrice, signalNameForClosePosition, notes, position);
                
                if (positionNumber == 0)
                    firstPositionEntryPrice = position.EntryPrice;
            }
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
            kAtrForStopLoss = (double)systemParameters.GetValue("kAtr");            
            atrPeriod = (int)systemParameters.GetValue("atrPeriod");           
            limitOpenedPositions = (int)systemParameters.GetValue("limitOpenedPositions");
            var pSide = (int)systemParameters.GetValue("positionSide");

            if (pSide == 0)
                positionSide = PositionSide.Long;
            else if (pSide == 1)
                positionSide = PositionSide.Short;
            else
                positionSide = PositionSide.Null;

            parametersCombination = string.Format("slowDonchian: {0}; fastDonchian: {1}; kAtr: {2}; atrPeriod: {3}", slowDonchian, fastDonchian, kAtrForStopLoss, atrPeriod);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
        }
        
        public override void CalculateIndicators()
        {
            nonTradingPeriod = Math.Max(slowDonchian, atrPeriod);
            highest = converter.GetHighest(converter.GetHighPrices(security), slowDonchian);
            lowest = converter.GetLowest(converter.GetLowPrices(security), fastDonchian);
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
            pane.AddList("Highest", highest, ListStyles.LINE, colorTSlab2, LineStyles.SOLID, PaneSides.RIGHT);
            pane.AddList("Lowest", lowest, ListStyles.LINE, colorTSlab3, LineStyles.SOLID, PaneSides.RIGHT);

            pane = context.CreatePane("Вторая панель", 50, true);            
            pane.AddList(security.ToString(), security, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);
            pane.AddList("Highest", highest, ListStyles.LINE, colorTSlab2, LineStyles.SOLID, PaneSides.RIGHT);
            pane.AddList("Lowest", lowest, ListStyles.LINE, colorTSlab3, LineStyles.SOLID, PaneSides.RIGHT);
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