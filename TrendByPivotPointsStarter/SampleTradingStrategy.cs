using System;
using System.Collections.Generic;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using Security = TradingSystems.Security;
using Converter = TradingSystems.Converter;
using Account = TradingSystems.Account;
using TSLab.Script.Helpers;
using TSLab.Script.Handlers;
using TSLab.DataSource;
using TradingSystems;

namespace TrendByPivotPointsStarter
{
    public class SampleTradingStrategy : TradingStrategy
    {
        public IContext Ctx { get; set; }
        public Logger Logger { get; set; } = new NullLogger();
        public PositionSide PositionSide { get { return positionSide; } }

        private LocalMoneyManager localMoneyManager;
        private ISecurity sec;
        private ISecurity secCompressed;
        private Security security;
        private IList<double> atr;

        private PositionSide positionSide;
        private int barNumber;
        private Converter convertable;

        private string signalNameForOpenPosition = "";
        private string signalNameForClosePosition = "";

        private string tradingSystemDescription;
        private string name = "TradingSystemDonchian";
        private string parametersCombination;
        private double fixedAtr;

        private IList<double> highest;
        private IList<double> lowest;

        private int slowDonchian;
        private int fastDonchian;
        private int atrPeriod;
        private double kAtrForStopLoss;
        private int limitOpenedPositions = 10;
        private double kAtrForOpenPosition = 0.5;
        private double openPositionPrice;

        public SampleTradingStrategy(LocalMoneyManager localMoneyManager, Account account, Security security, PositionSide positionSide)
        {
            this.localMoneyManager = localMoneyManager;
            var securityTSLab = security as TSLabSecurity;
            sec = securityTSLab.security;
            this.security = security;
            secCompressed = sec.CompressTo(Interval.D1);
            this.positionSide = positionSide;
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

        private bool IsPositionOpen(string notes = "")
        {
            var position = sec.Positions.GetLastActiveForSignal(signalNameForOpenPosition + notes, barNumber);
            return position != null;
        }

        private IPosition GetOpenedPosition(string notes)
        {
            var position = sec.Positions.GetLastActiveForSignal(signalNameForOpenPosition + notes, barNumber);
            return position;
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

        private void BuyIfGreater(double price, int contracts, string notes)
        {
            if (positionSide == PositionSide.Long)
                sec.Positions.BuyIfGreater(barNumber + 1, contracts, price, signalNameForOpenPosition + notes);
            if (positionSide == PositionSide.Short)
                sec.Positions.SellIfLess(barNumber + 1, contracts, price, signalNameForOpenPosition + notes);
        }

        private double GetStopPrice(string notes = "")
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

        public void CheckPositionOpenLongCase()
        {
            for (var i = 0; i < limitOpenedPositions; i++)
                CheckPositionOpenLongCase(i);
        }

        private void CheckPositionOpenLongCase(int positionNumber)
        {
            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, convertable.Long);
            double stopPrice;

            var notes = " Вход №" + (positionNumber + 1);

            if (!IsPositionOpen(notes))
            {
                Log("{0} позиция не открыта.", convertable.Long);

                if (positionNumber == 0)
                    fixedAtr = atr[barNumber];

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

                BuyIfGreater(convertable.Plus(openPositionPrice, positionNumber * fixedAtr * kAtrForOpenPosition), contracts, notes);

                Log("Отправляем ордер.", convertable.Long);
            }

            else
            {
                var position = GetOpenedPosition(notes);
                Log("{0} позиция открыта.", convertable.Long);
                stopPrice = GetStopPrice(notes);
                notes = " Выход №" + (positionNumber + 1);
                position.CloseAtStop(barNumber + 1, stopPrice, signalNameForClosePosition + notes);
            }
        }

        public void SetParameters(SystemParameters systemParameters)
        {
            slowDonchian = systemParameters.GetInt("slowDonchian");
            fastDonchian = systemParameters.GetInt("fastDonchian");
            kAtrForStopLoss = systemParameters.GetDouble("kAtr");
            atrPeriod = systemParameters.GetInt("atrPeriod");
            limitOpenedPositions = systemParameters.GetInt("limitOpenedPositions");
            kAtrForOpenPosition = kAtrForStopLoss;

            parametersCombination = string.Format("slowDonchian: {0}; fastDonchian: {1}; kAtr: {2}; atrPeriod: {3}", slowDonchian, fastDonchian, kAtrForStopLoss, atrPeriod);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
        }

        public void CalculateIndicators()
        {
            switch (positionSide)
            {
                case PositionSide.Long:
                    {
                        signalNameForOpenPosition = "LE";
                        signalNameForClosePosition = "LXS";
                        convertable = new Converter(isConverted: false);
                        break;
                    }
                case PositionSide.Short:
                    {
                        signalNameForOpenPosition = "SE";
                        signalNameForClosePosition = "SXS";
                        convertable = new Converter(isConverted: true);
                        break;
                    }
            }

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

        public bool HasOpenPosition()
        {
            IPosition position = null;
            if (positionSide == PositionSide.Long)
                position = sec.Positions.GetLastActiveForSignal("LE");
            else if (positionSide == PositionSide.Short)
                position = sec.Positions.GetLastActiveForSignal("SE");
            return position != null;
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

        public void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide)
        {
            throw new NotImplementedException();
        }
    }
}
