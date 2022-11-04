using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public interface ITradingSystem
    {
        Logger Logger { get; set; }
        PositionSide PositionSide { get; }

        IContext Ctx { get; set; }

        void CalculateIndicators();
        void CheckPositionCloseCase(int barNumber);
        void CheckPositionOpenLongCase(double lastPrice, int barNumber);
        void CheckPositionOpenShortCase(double lastPrice, int barNumber);
        bool CheckShortPositionCloseCase(IPosition se, int barNumber);
        bool HasOpenPosition();
        void Paint(Context context);
        void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide);
        void SetParameters(SystemParameters systemParameters);
        void Update(int barNumber);
        void Initialize(IContext ctx);
    }
}