using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPointsStrategy
{
    public interface Logger
    {
        void Log(string text);
        void Log(string text, params object[] args);
        void SwitchOn();
        void SwitchOff();
        void LockCurrentStatus();
        void UnlockCurrentStatus();
    }
}
