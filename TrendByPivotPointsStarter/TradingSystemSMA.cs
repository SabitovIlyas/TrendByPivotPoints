using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;

namespace TrendByPivotPointsStarter
{

    public class TradingSystemSMA : TradingSystem
    {
        public PositionSide PositionSide { get; set; }
        public int SMAperiod { get; set; }

        private List<double> sma;

        public TradingSystemSMA(List<Security> securities, ContractsManager contractsManager, TradingSystems.Indicators indicators, Logger logger):
            base(securities, contractsManager, indicators, logger)
        {            
        }               

        public override void CalculateIndicators()
        {            
            var bars = security.GetBars(lastBarNumber);
            var closes = (from bar in bars
                         select bar.Close).ToList();

            sma = indicators.SMA(closes, SMAperiod);
        }        

        public override void CheckPositionCloseCase(int barNumber)
        {
            throw new System.NotImplementedException();
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

        public override void CheckPositionOpenShortCase(double lastPrice, int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public override bool CheckShortPositionCloseCase(IPosition se, int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public override bool HasOpenPosition()
        {
            throw new System.NotImplementedException();
        }        

        public override void Initialize(IContext ctx)
        {
            throw new System.NotImplementedException();
        }

        public override void Paint(Context context)
        {
            throw new System.NotImplementedException();
        }        

        protected override void CheckPositionOpenShortCase(int positionNumber)
        {
            throw new System.NotImplementedException();
        }

        protected override void CheckPositionOpenLongCase(int positionNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}