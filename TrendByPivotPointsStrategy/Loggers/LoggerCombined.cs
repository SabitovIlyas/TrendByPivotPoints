using System.Collections.Generic;

namespace TradingSystems
{
    /// <summary>
    /// Пишет сообщение сразу в несколько журналов — например, и в консоль,
    /// и в файл, чтобы после прогона осталось, что почитать.
    /// </summary>
    public class LoggerCombined : Logger
    {
        private readonly List<Logger> loggers;

        public LoggerCombined(params Logger[] loggers)
        {
            this.loggers = new List<Logger>(loggers);
        }

        public override void Log(string text)
        {
            if (!switchOn)
                return;

            foreach (var logger in loggers)
                logger.Log(text);
        }

        public override void Log(string text, params object[] args)
        {
            if (!switchOn)
                return;

            foreach (var logger in loggers)
                logger.Log(text, args);
        }
    }
}
