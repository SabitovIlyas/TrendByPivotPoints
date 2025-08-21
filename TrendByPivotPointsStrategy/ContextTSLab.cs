using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public class ContextTSLab: Context
    {
        private IContext context;
        public bool IsLastBarClosed { get { return context.IsLastBarClosed; } }     
        public bool IsRealTimeTrading { get; private set; }
        public bool IsOptimization { get { return context.IsOptimization; } }

        public ContextTSLab(IContext context, ISecurity securityFirst)
        {
            this.context = context;
            IsRealTimeTrading = !IsLaboratory(securityFirst);
        }

        private bool IsLaboratory(ISecurity security)
        {
            var realTimeSecurity = security as ISecurityRt;
            return realTimeSecurity == null;
        }

        public Pane CreateGraphPane(string name, string title)
        {
            var pane = context.CreateGraphPane(name, title);
            return new PaneTSLab(pane);
        }

        public IReadOnlyList<RecalcReason> LastRecalcReasons()
        {
            return context.Runtime.LastRecalcReasons;
        }

        public IGraphPane CreatePane1(string title, double sizePct, bool hideLegend, bool addToTop = false)
        {
            return context.CreatePane(title, sizePct, hideLegend, addToTop);
        }

        public GraphPane CreatePane(string title, double sizePct, bool hideLegend, bool addToTop)
        {
            var graphPane = context.CreatePane(title, sizePct, hideLegend, addToTop);
            return new GraphPaneTsLab(graphPane);                       
        }

        public Context GetClone()
        {
            throw new System.NotImplementedException();
        }
    }
}
