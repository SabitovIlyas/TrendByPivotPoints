using System.Runtime.Remoting.Contexts;

namespace TradingSystems
{
    public interface Context
    {
        bool IsLastBarClosed { get; }
        Pane CreateGraphPane(string name, string title);
    }
}
