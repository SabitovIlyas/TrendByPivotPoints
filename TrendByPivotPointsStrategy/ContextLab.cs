using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public class ContextLab : Context
    {
        public bool IsLastBarClosed => throw new System.NotImplementedException();

        public bool IsRealTimeTrading { get { return false; } }

        public Pane CreateGraphPane(string name, string title)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<RecalcReason> LastRecalcReasons()
        {
            return new ReadAndAddList<RecalcReason>();
        }
    }
}
