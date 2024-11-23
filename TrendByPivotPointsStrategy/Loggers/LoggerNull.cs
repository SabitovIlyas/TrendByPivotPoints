namespace TradingSystems
{
    public class LoggerNull : Logger
    {     
        public override void Log(string text)
        {
        }

        public override void Log(string text, params object[] args)
        {
        }
    }
}
