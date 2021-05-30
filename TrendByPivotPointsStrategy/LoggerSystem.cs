using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class LoggerSystem:Logger
    {
        IContext context;
        public LoggerSystem(IContext context)
        {
            this.context = context;
        }

        public void Log(string text)
        {
            context.Log(text);
        }
    }
}
