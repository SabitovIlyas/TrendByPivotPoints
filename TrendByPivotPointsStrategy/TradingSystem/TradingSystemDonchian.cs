using System;
using System.Collections.Generic;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.Helpers;
using TSLab.Script.Handlers;
using TSLab.DataSource;
using System.Linq;

namespace TradingSystems
{
    public class TradingSystemDonchian : TradingSystem
    {        
        public IContext Ctx { get; set; }
        public Logger Logger { get; set; } = new NullLogger();
        public PositionSide PositionSide { get { return positionSide; } }

        private ContractsManager localMoneyManager;
        private ISecurity sec;
        private ISecurity secCompressed;
        private Security security;
        private IList<double> atr;

        private PositionSide positionSide;
        private Converter convertable;

        private string signalNameForOpenPosition = "";
        private string signalNameForClosePosition = "";

        private string name = "TradingSystemDonchian";
        private string parametersCombination;
        private double fixedAtr;

        private IList<double> highest;
        private IList<double> lowest;

        private int slowDonchian;
        private int fastDonchian;
        private int atrPeriod;
        private double kAtrForStopLoss;
        private double kAtrForOpenPosition = 0.5;
        private double openPositionPrice;

        public TradingSystemDonchian(List<Security> securities, ContractsManager contractsManager, TradingSystems.Indicators indicators, Logger logger) :
            base(securities, contractsManager, indicators, logger)
        {   
            var securityTSLab = security as SecurityTSLab;
            sec = securityTSLab.security;            
            secCompressed = sec.CompressTo(Interval.D1);            
        }                               

        private void BuyIfGreater(double price, int contracts, string notes)
        {
            if (positionSide == PositionSide.Long)
                sec.Positions.BuyIfGreater(barNumber + 1, contracts, price, signalNameForOpenPosition + notes);
            if (positionSide == PositionSide.Short)
                sec.Positions.SellIfLess(barNumber + 1, contracts, price, signalNameForOpenPosition + notes);
        }

        protected override double GetStopPrice(string notes = "")
        {
            double stopPriceAtr;
            if (IsPositionOpen(notes))
            {
                var position = GetOpenedPosition(notes);
                stopPriceAtr = convertable.Minus(position.EntryPrice, kAtrForStopLoss * fixedAtr);
            }
            else
                stopPriceAtr = convertable.Minus(highest[barNumber], kAtrForStopLoss * fixedAtr);
            var stopPriceDonchian = lowest[barNumber];
            return convertable.Maximum(stopPriceAtr, stopPriceDonchian);
        }        

        protected override void CheckPositionOpenLongCase(int positionNumber)
        {
            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, convertable.Long);
            double stopPrice;

            var notes = " Вход №" + (positionNumber + 1);

            if (!IsPositionOpen(notes))
            {
                Log("{0} позиция не открыта.", convertable.Long);

                if (positionNumber == 0)
                    fixedAtr = atr[barNumber];

                Log("Фиксированный ATR = {0}", fixedAtr);

                Log("Вычисляем стоп-цену...");
                stopPrice = GetStopPrice(notes);

                Log("Определяем количество контрактов...");
                var contracts = localMoneyManager.GetQntContracts(highest[barNumber], stopPrice, positionSide);

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
                    openPositionPrice = highest[barNumber];

                var price = convertable.Plus(openPositionPrice, positionNumber * fixedAtr * kAtrForOpenPosition);
                Log("Рассчитаем цену для открытия позиции, исходя из следующих данных: {0} {1} {2} * {3} * {4} = {5}", openPositionPrice, convertable.SymbolPlus, positionNumber, fixedAtr, kAtrForOpenPosition, price);

                BuyIfGreater(price, contracts, notes);

                Log("Отправляем ордер.", convertable.Long);
            }

            else
            {
                if (Ctx.Runtime.LastRecalcReasons.Any(x => x.Name == EventKind.PositionOpening.ToString()))                
                    Log("Внеочередной пересчёт по открытию позиции! Надо выставлять стоп-лосс!");

                var position = GetOpenedPosition(notes);
                Log("{0} позиция открыта.", convertable.Long);
                stopPrice = GetStopPrice(notes);
                notes = " Выход №" + (positionNumber + 1);
                position.CloseAtStop(barNumber + 1, stopPrice, signalNameForClosePosition + notes);
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
            slowDonchian = systemParameters.GetInt("slowDonchian");
            fastDonchian = systemParameters.GetInt("fastDonchian");
            kAtrForStopLoss = systemParameters.GetDouble("kAtr");
            atrPeriod = systemParameters.GetInt("atrPeriod");
            limitOpenedPositions = systemParameters.GetInt("limitOpenedPositions");

            parametersCombination = string.Format("slowDonchian: {0}; fastDonchian: {1}; kAtr: {2}; atrPeriod: {3}", slowDonchian, fastDonchian, kAtrForStopLoss, atrPeriod);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
        }

        public override void CalculateIndicators()
        {
            highest = convertable.GetHighest(convertable.GetHighPrices(secCompressed), slowDonchian);
            lowest = convertable.GetLowest(convertable.GetLowPrices(secCompressed), fastDonchian);
            atr = Series.AverageTrueRange(secCompressed.Bars, atrPeriod);

            highest = secCompressed.Decompress(highest);
            lowest = secCompressed.Decompress(lowest);
            atr = secCompressed.Decompress(atr);           
        }

        public void Paint(Context context)
        {
            if (Ctx.IsOptimization)
                return;           
            
            var pane = Ctx.CreatePane("Первая панель", 50, true);
            var colorTSlab1 = new Color(SystemColor.Blue.ToArgb());
            var colorTSlab2 = new Color(SystemColor.Green.ToArgb());
            var colorTSlab3 = new Color(SystemColor.Red.ToArgb());

            pane.AddList(secCompressed.ToString(), secCompressed, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);
            pane.AddList("Highest", highest, ListStyles.LINE, colorTSlab2, LineStyles.SOLID, PaneSides.RIGHT);
            pane.AddList("Lowest", lowest, ListStyles.LINE, colorTSlab3, LineStyles.SOLID, PaneSides.RIGHT);

            pane = Ctx.CreatePane("Вторая панель", 50, true);            
            pane.AddList(sec.ToString(), sec, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);
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

        public override void CheckPositionCloseCase(int barNumber)
        {
            throw new NotImplementedException();
        }                

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}