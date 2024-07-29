using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class TsLabLogger : Logger
    {
        IContext context;       

        public TsLabLogger(IContext context)
        {
            this.context = context;
        }               

        public override void Log(string text)
        {
            if (switchOn)
                context.Log(text);
        }

        public override void Log(string text, params object[] args)
        {
            if (switchOn)
            {
                var log = string.Format(text, args);
                context.Log(log);
            }
        }        
    }
}