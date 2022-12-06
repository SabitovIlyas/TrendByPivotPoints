using TSLab.Script.Handlers;
using TSLab.Script.Optimization;
using TSLab.Script;

namespace TrendByPivotPointsStrategy.TSLabScripts
{
    public class ScriptScalperStrategy : IExternalScript
    {
        public OptimProperty rateUSD = new OptimProperty(61, 1, 200, 1);
        public OptimProperty positionSide = new OptimProperty(0, 1, 2, 1);  //0 -- для лонга, 1 -- для шорта, 2 -- для null
        public OptimProperty comission = new OptimProperty(0.565, 0.001, 100, 0.001);
        public OptimProperty mode = new OptimProperty(1, 0, 1, 1);          //0 -- для тестов в реальном времени, 1 -- для оптимизации и 2 -- для торговли
        public OptimProperty isPaint = new OptimProperty(0, 0, 1, 1);
        public OptimProperty isLoggerOn = new OptimProperty(0, 0, 1, 1);
        public OptimProperty shares = new OptimProperty(1, 0, 1, 1);
        public OptimProperty isUSD = new OptimProperty(0, 0, 1, 1);
        public OptimProperty period = new OptimProperty(14, 5, 15, 1);
        public OptimProperty rsiBand = new OptimProperty(30, 25, 35, 1);

        public void Execute(IContext context, ISecurity security)
        {
            context.Log("123!!!");
        }
    }
}