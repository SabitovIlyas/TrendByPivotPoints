using System;
using System.Collections.Generic;
using TSLab.Script;
using SystemColor = System.Drawing.Color;
using TSLab.Script.Helpers;
using TSLab.Script.Handlers;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class TradingSystemDonchian : ITradingSystem
    {
        public IContext Ctx { get; set; }
        public Logger Logger { get; set; } = new NullLogger();
        public PositionSide PositionSide { get { return positionSide; } }

        private LocalMoneyManager localMoneyManager;
        private ISecurity sec;
        private Security security;
        private IList<double> atr;

        private PositionSide positionSide;
        private Account account;
        private int barNumber;
        private Converter convertable;

        private string signalNameForOpenPosition = "";
        private string signalNameForClosePosition = "";

        private string tradingSystemDescription;
        private string name = "TradingSystemDonchian";
        private string parametersCombination;
        private double fixedAtr;

        private IPosition position;
        private IList<double> highest;
        private IList<double> lowest;

        private int slowDonchian;
        private int fastDonchian;
        private int atrPeriod;
        private double kAtr;

        public TradingSystemDonchian(LocalMoneyManager localMoneyManager, Account account, Security security, PositionSide positionSide)
        {
            this.localMoneyManager = localMoneyManager;
            this.account = account;
            var securityTSLab = security as SecurityTSlab;
            sec = securityTSLab.security;
            this.security = security;            
            this.positionSide = positionSide;
        }

        public void Update(int barNumber)
        {
            try
            {
                if (barNumber == 0)
                    return;

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

        private bool IsPositionOpen()
        {
            var position = sec.Positions.GetLastActiveForSignal(signalNameForOpenPosition, barNumber);
            return position != null;
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

        private void BuyIfGreater(int contracts)
        {
            if (positionSide == PositionSide.Long)
                sec.Positions.BuyIfGreater(barNumber + 1, contracts, highest[barNumber], signalNameForOpenPosition);
            if (positionSide == PositionSide.Short)
                sec.Positions.SellIfLess(barNumber + 1, contracts, highest[barNumber], signalNameForOpenPosition);
        }

        private double GetStopPrice()
        {
            double stopPriceAtr;
            if (IsPositionOpen())
                stopPriceAtr = convertable.Minus(position.EntryPrice, kAtr * fixedAtr);
            else
                stopPriceAtr = convertable.Minus(highest[barNumber], kAtr * fixedAtr);
            var stopPriceDonchian = lowest[barNumber];
            return convertable.Maximum(stopPriceAtr, stopPriceDonchian);
        }

        public void CheckPositionOpenLongCase()
        {
            position = sec.Positions.GetLastActiveForSignal(signalNameForOpenPosition, barNumber);
            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, convertable.Long);
            double stopPrice;
            if (!IsPositionOpen())
            {
                Log("{0} позиция не открыта.", convertable.Long);

                fixedAtr = atr[barNumber];

                Log("Вычисляем стоп-цену...");
                stopPrice = GetStopPrice();

                Log("Определяем количество контрактов...");
                var contracts = localMoneyManager.GetQntContracts(highest[barNumber], stopPrice, positionSide);

                Log("Торгуем в лаборатории или в режиме реального времени?");
                if (security.IsRealTimeTrading)
                {
                    contracts = 1;
                    Log("Торгуем в режиме реального времени, поэтому количество контрактов установим в количестве {0}", contracts);
                }
                else
                {
                    //contracts = 1;
                    Log("Торгуем в лаборатории.");
                }

                BuyIfGreater(contracts);

                Log("Открываем {0} позицию! Отправляем ордер.", convertable.Long);
            }

            else
            {
                Log("{0} позиция открыта.", convertable.Long);
                stopPrice = GetStopPrice();
                position.CloseAtStop(barNumber + 1, stopPrice, signalNameForClosePosition);
            }
        }                

        public void SetParameters(SystemParameters systemParameters)
        {
            slowDonchian = systemParameters.GetInt("slowDonchian");
            fastDonchian = systemParameters.GetInt("fastDonchian");
            kAtr = systemParameters.GetDouble("kAtr");
            atrPeriod = systemParameters.GetInt("atrPeriod");            

            parametersCombination = string.Format("slowDonchian: {0}; fastDonchian: {1}; kAtr: {2}; atrPeriod: {3}", slowDonchian, fastDonchian, kAtr, atrPeriod);
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

            highest = convertable.GetHighest(convertable.GetHighPrices(sec), slowDonchian);
            lowest = convertable.GetLowest(convertable.GetLowPrices(sec), fastDonchian);                        
            atr = Series.AverageTrueRange(sec.Bars, atrPeriod);                        
        }

        public void Paint(Context context)
        {
            if (Ctx.IsOptimization)
                return;

            var contextTSLab = context as ContextTSLab;
            var name = string.Format("{0} {1} {2}", sec.ToString(), positionSide, sec.Interval);
            var pane = contextTSLab.context.CreateGraphPane(name: name, title: name);
            var colorTSlab1 = new Color(SystemColor.Blue.ToArgb());
            var colorTSlab2 = new Color(SystemColor.Green.ToArgb());
            var colorTSlab3 = new Color(SystemColor.Red.ToArgb());
            
            var securityTSLab = (SecurityTSlab)security;
            pane.AddList(sec.ToString(), securityTSLab.security, CandleStyles.BAR_CANDLE, colorTSlab1, PaneSides.RIGHT);
            
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