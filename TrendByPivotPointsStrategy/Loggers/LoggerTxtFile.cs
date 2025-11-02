using System.IO;

namespace TradingSystems
{
    public class LoggerTxtFile : Logger
    {
        private string fileName = string.Empty;
        public LoggerTxtFile(string filename)
        {
            this.fileName = filename;  
        }

        public void LockCurrentStatus()
        {
        }

        public override void Log(string text)
        {
            if (switchOn)
            {
                using (StreamWriter writer = new StreamWriter(fileName, append: true))
                {
                    writer.WriteLine($"{text}");
                }
            }
        }

        public override void Log(string text, params object[] args)
        {
            if (switchOn)
            {
                using (StreamWriter writer = new StreamWriter(fileName, append: true))
                {
                    var log = string.Format(text, args);
                    writer.WriteLine(log);
                }
            }
        }
    }
}