using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class TsLabContext : Context
    {
        IContext context;

        public bool IsLastBarClosed { get { return context.IsLastBarClosed; } }

        public TsLabContext(IContext context)
        {
            this.context = context;            
        }

        public Pane CreateGraphPane(string name, string title)
        {
            throw new System.NotImplementedException();
        }
    }
}
