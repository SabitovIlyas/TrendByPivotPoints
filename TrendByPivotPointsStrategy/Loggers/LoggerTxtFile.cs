using System;
using System.IO;

namespace TradingSystems
{
    /// <summary>
    /// Журнал в текстовый файл. Файл открывается один раз и держится открытым:
    /// открывать и закрывать его на каждой строке слишком дорого — прогон
    /// оптимизатора пишет десятки тысяч строк. Каждая строка сразу сбрасывается
    /// на диск, поэтому после аварийного завершения журнал не обрывается.
    /// </summary>
    public class LoggerTxtFile : Logger, IDisposable
    {
        private readonly object sync = new object();
        private readonly StreamWriter writer;

        public LoggerTxtFile(string filename)
        {
            //FileShare.ReadWrite — чтобы журнал можно было читать, пока идёт прогон.
            var stream = new FileStream(filename, FileMode.Append, FileAccess.Write,
                FileShare.ReadWrite);
            writer = new StreamWriter(stream) { AutoFlush = true };
        }

        public void LockCurrentStatus()
        {
        }

        public override void Log(string text)
        {
            if (!switchOn)
                return;

            //Писать могут несколько потоков сразу — строки не должны перемешиваться.
            lock (sync)
                writer.WriteLine(text);
        }

        public override void Log(string text, params object[] args)
        {
            if (!switchOn)
                return;

            var log = string.Format(text, args);
            lock (sync)
                writer.WriteLine(log);
        }

        public void Dispose()
        {
            lock (sync)
                writer.Dispose();
        }
    }
}
