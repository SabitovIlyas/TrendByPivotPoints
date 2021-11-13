using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public interface MainSystem
    {
        Logger Logger { get; set; }
        void Initialize(ISecurity[] securities, IContext ctx);
        void Paint(IContext ctx, ISecurity sec);
        void Run();
        void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide, double rateUSD, double positionSide, double comission, double riskValuePrcnt, int securityNumber, int instrumentsGroup, int shares = 1);
    }
}