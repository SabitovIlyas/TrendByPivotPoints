using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStarter
{
    public class SampleMainSystem : MainSystem
    {
        public override void Initialize(ISecurity[] securities, IContext ctx)
        {

        }

        public override void Paint()
        {
            throw new System.NotImplementedException();
        }

        public override void Run()
        {

        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            SetBaseParameters(systemParameters);
            var specialParameter = systemParameters.GetInt("specialParameter");
        }
    }
}