using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public interface Context
    {
        bool IsLastBarClosed { get; }
        bool IsRealTimeTrading { get; }
        bool IsOptimization { get; }
        Pane CreateGraphPane(string name, string title);
        IReadOnlyList <RecalcReason> LastRecalcReasons();
        GraphPane CreatePane(string title, double sizePct, bool hideLegend, bool addToTop = false);
    }
}