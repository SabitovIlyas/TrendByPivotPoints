using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public abstract class MainSystem
    {
        public Logger Logger { get { return logger; } set { logger = value; } }

        protected ContextTSLab context;
        protected List<ITradingSystemPivotPointsEMA> tradingSystems;
        protected int securityNumber;
        protected int instrumentsGroup;

        protected int leftLocalSide;
        protected int rightLocalSide;
        protected double pivotPointBreakDownSide;
        protected int EmaPeriodSide;

        protected double rateUSD;
        protected int positionSide;
        protected double comission;
        protected double riskValuePrcnt;
        
        protected int shares;        
        protected Logger logger = new NullLogger();
        protected Account account;
        protected SystemParameters systemParameters;

        public abstract void Initialize(ISecurity[] securities, IContext ctx);
        public void Paint()
        {
            var tradingSystem = tradingSystems[securityNumber];
            tradingSystem.Paint(context);
        }
        public abstract void Run();
        public void SetParameters(SystemParameters systemParameters)
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
        }
    }
}