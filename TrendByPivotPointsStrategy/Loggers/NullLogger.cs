namespace TradingSystems
{
    public class NullLogger : Logger
    {     
        public override void Log(string text)
        {
        }

        public override void Log(string text, params object[] args)
        {
        }
    }
}
