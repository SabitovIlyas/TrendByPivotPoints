using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class LoggerSystem:Logger
    {
        IContext context;
        bool switchOn = true;
        public LoggerSystem(IContext context)
        {
            this.context = context;
        }

        public void Log(string text)
        {
            if (switchOn)
                context.Log(text);
        }

        public void SwitchOff()
        {
            switchOn = false;
        }

        public void SwitchOn()
        {
            switchOn = true;
        }
    }
}
