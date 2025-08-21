using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public class ContextLab : Context
    {
        public bool IsLastBarClosed => throw new System.NotImplementedException();

        public bool IsRealTimeTrading { get { return false; } }

        public bool IsOptimization => throw new System.NotImplementedException();

        public Pane CreateGraphPane(string name, string title)
        {
            throw new System.NotImplementedException();
        }

        public IGraphPane CreatePane(string title, double sizePct, bool hideLegend, bool addToTop = false)
        {
            throw new System.NotImplementedException();
        }

        public Context GetClone()
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<RecalcReason> LastRecalcReasons()
        {
            return new ReadAndAddList<RecalcReason>();
        }

        GraphPane Context.CreatePane(string title, double sizePct, bool hideLegend, bool addToTop)
        {
            throw new System.NotImplementedException();
        }
    }
}
