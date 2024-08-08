using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class ContextTSLab: Context
    {
        private IContext context;

        public bool IsLastBarClosed { get { return context.IsLastBarClosed; } }     

        public ContextTSLab(IContext context)
        {
            this.context = context;
        }

        public Pane CreateGraphPane(string name, string title)
        {
            var pane = context.CreateGraphPane(name, title);
            return new PaneTSLab(pane);
        }
    }
}
