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
        public TradingSystemSMA(List<Security> securities, SystemParameters systemParameters, ContractsManager contractsManager, TradingSystems.Indicators indicators, Logger logger):
            base(securities, systemParameters, contractsManager, indicators, logger)
        {
            base.SetParameters();
        }               

        public override void CalculateIndicators()
        {
            var bars = security.GetBars(security.BarNumber);
            var closes = (from bar in bars
                         select bar.Close).ToList();

            indicators.SMA(closes, period: 10);
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

        public override void SetParameters()
        {
            throw new System.NotImplementedException();
        }

        public override void Update(int barNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}