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
    }
}
