using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public abstract class PivotPointsMainSystem : MainSystem
    {

        protected int instrumentsGroup;
        protected int leftLocalSide;
        protected int rightLocalSide;
        protected double pivotPointBreakDownSide;
        protected int EmaPeriodSide;

        public override void Paint()
        {
            var tradingSystem = tradingSystems[securityNumber];
            tradingSystem.Paint(context);
        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            this.systemParameters = systemParameters;

            leftLocalSide = systemParameters.GetInt("leftLocalSide");
            rightLocalSide = systemParameters.GetInt("rightLocalSide");
            pivotPointBreakDownSide = systemParameters.GetDouble("pivotPointBreakDownSide");
            EmaPeriodSide = systemParameters.GetInt("emaPeriodSide");
            rateUSD = systemParameters.GetDouble("rateUSD");
            positionSide = systemParameters.GetInt("positionSide");
            comission = systemParameters.GetDouble("comission"); ;
            riskValuePrcnt = systemParameters.GetDouble("riskValuePrcnt"); ;
            securityNumber = systemParameters.GetInt("securityNumber");
            instrumentsGroup = systemParameters.GetInt("instrumentsGroup");
            shares = systemParameters.GetInt("shares");
            isUSD = systemParameters.GetInt("isUSD");
        }
    }
}