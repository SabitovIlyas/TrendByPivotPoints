using System.Collections.Generic;
using System.Linq;
using TradingSystems;
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

        public override void CheckPositionCloseCase(int barNumber)
        {
            throw new System.NotImplementedException();
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
                //Остановился здесь. Не могу понять, как быть с этим. Скорее всего, мне придётся всё-таки
                //реализовывать класс IContext.
                if (Ctx.Runtime.LastRecalcReasons.Any(x => x.Name == EventKind.PositionOpening.ToString()))
                    Log("Внеочередной пересчёт по открытию позиции! Надо выставлять стоп-лосс!");

                var position = GetOpenedPosition(notes);
                Log("{0} позиция открыта.", converter.Long);
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

        protected override double GetStopPrice(string notes = "")
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

        public override void Initialize()
        {
            parametersCombination = string.Format("SMA: {0}", sma);
            base.Initialize();
        }            
    }
}