using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public interface IStopLoss
    {
        IContext ctx { get; set; }
        bool IsStopLossUpdateWhenBarIsClosedOnly { get; set; }
        Logger Logger { get; set; }
    }
}