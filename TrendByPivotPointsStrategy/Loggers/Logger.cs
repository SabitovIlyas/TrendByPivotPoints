namespace TradingSystems
{
    public abstract class Logger
    {        
        protected bool switchOn = true;
        bool locked = false;        

        public void LockCurrentStatus()
        {
            locked = true;
        }

        public abstract void Log(string text);

        //public void Log(string text) //убрать реализацию
        //{
        //    if (switchOn)
        //        context.Log(text);
        //}

        public abstract void Log(string text, params object[] args);

        //public void Log(string text, params object[] args) //убрать реализацию
        //{
        //    if (switchOn)
        //    {
        //        var log = string.Format(text, args);
        //        context.Log(log);
        //    }
        //}

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
