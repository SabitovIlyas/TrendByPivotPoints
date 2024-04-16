using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public abstract class TradingSystem
    {
        IContext Ctx { get; set; }

        protected Logger logger;
        protected PositionSide positionSide;
        protected SystemParameters systemParameters;
        protected List<Security> securities { get; set; }               

        public abstract void CalculateIndicators();
        public abstract void CheckPositionCloseCase(int barNumber);
        public abstract void CheckPositionOpenLongCase(double lastPrice, int barNumber);
        public abstract void CheckPositionOpenShortCase(double lastPrice, int barNumber);
        public abstract bool CheckShortPositionCloseCase(IPosition se, int barNumber);
        public abstract bool HasOpenPosition();
        public abstract void Paint(Context context);
        public abstract void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide);
        public abstract void SetParameters(SystemParameters systemParameters);
        public abstract void Update(int barNumber);
        public abstract void Initialize(IContext ctx);
    }
}