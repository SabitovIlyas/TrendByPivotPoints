using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class LoggerSystem : Logger
    {
        IContext context;
        bool switchOn = true;
        bool locked = false;

        //public static LoggerSystem ()
        public LoggerSystem(IContext context)
        {
            this.context = context;
        }

        public void LockCurrentStatus()
        {
            locked = true;
        }

        public void Log(string text)
        {
            if (switchOn)
                context.Log(text);
        }

        public void Log(string text, params object[] args)
        {            
            if (switchOn)
            {
                var log = string.Format(text, args);
                context.Log(log); 
            }
        }

        public void SwitchOff()
        {
            if (!locked)
                switchOn = false;
        }

        public void SwitchOn()
        {
            if (!locked)
                switchOn = true;
        }

        public void UnlockCurrentStatus()
        {
            locked = false;
        }
    }
}
