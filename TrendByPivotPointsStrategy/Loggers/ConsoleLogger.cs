using System;

namespace TradingSystems
{
    public class ConsoleLogger : Logger
    {
        public void LockCurrentStatus()
        {
        }

        public override void Log(string text)
        {
            if (switchOn)
                Console.WriteLine(text);
        }

        public override void Log(string text, params object[] args)
        {
            if (switchOn)
            {
                var log = string.Format(text, args);
                Console.WriteLine(text);
            }
        }
    }
}