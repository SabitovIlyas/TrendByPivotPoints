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
            //Остановился здесь. Проследить за этим параметром security.BarNumber
            var bars = security.GetBars(security.BarNumber);
            var closes = (from bar in bars
                         select bar.Close).ToList();

            sma = indicators.SMA(closes, SMAperiod);
        }

        public override void Update(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public override void CheckPositionCloseCase(int barNumber)
        {
            throw new System.NotImplementedException();
        }        

        public override void CheckPositionOpenLongCase(double lastPrice, int barNumber)
        {
            throw new System.NotImplementedException();
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
    }
}