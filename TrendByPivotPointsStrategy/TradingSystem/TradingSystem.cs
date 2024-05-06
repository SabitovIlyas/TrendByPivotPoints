using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public abstract class TradingSystem
    {
        protected List<Security> securities { get; set; }

        protected Logger logger;
        protected PositionSide positionSide;
        protected SystemParameters systemParameters;
        protected int lastBarNumber;
        protected Security security;
        protected ContractsManager contractsManager;
        protected Indicators indicators;
        protected int barNumber;
        protected string tradingSystemDescription;
        protected int limitOpenedPositions = 1;

        public TradingSystem(List<Security> securities, ContractsManager contractsManager, Indicators indicators, Logger logger)
        {
            if (securities.Count == 0)            
                throw new ArgumentOutOfRangeException(nameof(securities));

            this.securities = securities;
            security = securities.First();
            lastBarNumber = security.GetBarsCountReal() - 1;            
            this.contractsManager = contractsManager;
            this.indicators = indicators;
            this.logger = logger;
        }

        public abstract void CalculateIndicators(int barNumber);
        public abstract void CheckPositionCloseCase(int barNumber);
        public abstract void CheckPositionOpenLongCase(double lastPrice, int barNumber);
        public abstract void CheckPositionOpenShortCase(double lastPrice, int barNumber);
        public abstract bool CheckShortPositionCloseCase(IPosition se, int barNumber);
        public abstract bool HasOpenPosition();
        public abstract void Paint(Context context);               
        public virtual void Update(int barNumber)
        {
            try
            {
                this.barNumber = barNumber;

                if (security.IsRealTimeActualBar(barNumber))
                    logger.SwitchOn();
                else
                    logger.SwitchOff();

                CheckPositionOpenLongCase();
            }

            catch (Exception e)
            {
                Log("Исключение в методе Update(): " + e.ToString());
            }
        }

        public void CheckPositionOpenLongCase()
        {
            for (var i = 0; i < limitOpenedPositions; i++)
                CheckPositionOpenLongCase(i);
        }

        protected virtual void CheckPositionOpenLongCase(int positionNumber)
        {
            //остановился здесь
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

        protected void Log(string text)
        {
            logger.Log("{0}: {1}", tradingSystemDescription, text);
        }

        protected void Log(string text, params object[] args)
        {
            text = string.Format(text, args);
            Log(text);
        }
        public abstract void Initialize(IContext ctx);       
    }
}