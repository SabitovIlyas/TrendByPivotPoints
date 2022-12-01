using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class ContextTSLab: Context
    {
        private IContext context;

        public static ContextTSLab Create(IContext context)
        {
            return new ContextTSLab(context);
        }      

        private ContextTSLab(IContext context)
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
