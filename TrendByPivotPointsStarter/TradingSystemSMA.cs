using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStarter
{
    public class TradingSystemSMA : TradingSystem
    {
        public PositionSide PositionSide { get; set; }
        public int SMAperiod { get; set; }

        private List<double> sma;

        public TradingSystemSMA(List<Security> securities, ContractsManager contractsManager, TradingSystems.Indicators indicators, Logger logger) :
            base(securities, contractsManager, indicators, logger)
        {
            name = "TradingSystemSMA";
        }    

        public override void CalculateIndicators()
        {            
            var bars = security.GetBars(lastBarNumber);
            var closes = (from bar in bars
                         select bar.Close).ToList();

            sma = indicators.SMA(closes, SMAperiod);
        }        

        public override void CheckPositionCloseCase(Position position, string signalNameForClosePosition, out bool isPositionClosing) //Реализовать класс Position
        {
            if (converter.IsLessOrEqual(currentPrice, sma[barNumber]))
            {
                //Остановился здесь
                position.CloseAtMarket(barNumber + 1, signalNameForClosePosition);
                isPositionClosing = true;
            }
            else
                isPositionClosing = false;
        }

        protected override void CheckPositionOpenLongCase(int positionNumber)
        {
            Log("бар № {0}. Открыта ли {1} позиция?", barNumber, converter.Long);
            double stopPrice;
            var notes = " Вход №" + (positionNumber + 1);

            if (!IsPositionOpen(notes))
            {
                Log("{0} позиция не открыта.", converter.Long);
                Log("Вычисляем стоп-цену...");
                stopPrice = GetStopPrice(notes);
                entryPricePlanned = sma[barNumber];

                Log("Определяем количество контрактов...");
                var contracts = contractsManager.GetQntContracts(entryPricePlanned, stopPrice, positionSide);
                BuyIfGreater(contracts, notes);
                Log("Отправляем ордер.", converter.Long);
            }

            else
            {

                var position = GetOpenedPosition(notes);
                Log("{0} позиция открыта.", converter.Long);                
                CheckPositionCloseCase(position, out bool isPositionClosing);
                    
                if (isPositionClosing)
                    return;
                
                stopPrice = GetStopPrice(notes);
                notes = " Выход №" + (positionNumber + 1);
                position.CloseAtStop(barNumber + 1, stopPrice, signalNameForClosePosition + notes);
            }
        }

        private void BuyIfGreater(int contracts, string notes)
        {            
            security.BuyIfGreater(barNumber + 1, contracts, entryPricePlanned, signalNameForOpenPosition + notes, 
                converter.IsConverted);            
        }                                 

        public override void Initialize()
        {
            parametersCombination = string.Format("SMA: {0}", sma);
            base.Initialize();
        }

        protected override double GetStopPrice(string notes = "")
        {
            double stopPrice;
            double risk = 0.01;
            if (IsPositionOpen(notes))
            {
                var position = GetOpenedPosition(notes);
                stopPrice = converter.Minus(position.EntryPrice, position.EntryPrice * risk);
            }
            else
                stopPrice = converter.Minus(entryPricePlanned, entryPricePlanned * risk);
            return stopPrice;
        }       
    }
}