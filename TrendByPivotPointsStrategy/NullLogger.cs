namespace TrendByPivotPointsStrategy
{
    public class NullLogger : Logger
    {
        public void LockCurrentStatus()
        {
        }

        public void Log(string text) { }

        public void Log(string text, params object[] args)
        {            
        }

        public void SwitchOff()
        {
        }

        public void SwitchOn()
        {
        }

        public void UnlockCurrentStatus()
        {
        }
    }
}
