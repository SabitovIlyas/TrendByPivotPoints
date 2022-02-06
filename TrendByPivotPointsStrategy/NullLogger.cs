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
            throw new System.NotImplementedException();
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
