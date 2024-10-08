using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public interface Context
    {
        bool IsLastBarClosed { get; }
        bool IsRealTimeTrading { get; }
        Pane CreateGraphPane(string name, string title);
        IReadOnlyList <RecalcReason> LastRecalcReasons();
    }
}
