using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class RealLogger:Logger
    {
        IContext context;
        public RealLogger(IContext context)
        {
            this.context = context;
        }

        public void Log(string text)
        {
            context.Log(text);
        }
    }
}
