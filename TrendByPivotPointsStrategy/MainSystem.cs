using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public interface MainSystem
    {
        void Initialize(ISecurity[] securities, IContext ctx);
        void Paint(IContext ctx, ISecurity sec);
        void Run();
        void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide, double rateUSD, double positionSide, double comission, double riskValuePrcnt);
    }
}