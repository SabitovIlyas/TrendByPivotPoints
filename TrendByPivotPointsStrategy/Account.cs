using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TradingSystems
{
    public interface Account
    {
        double InitDeposit { get; }
        double Equity { get; }

        double GObying { get; }

        double GOselling { get; }

        double Rate { get; set; }

        ISecurity Security { get; }
        void Update(int barNumber);
        void Initialize(List<Security> securities);

        Logger Logger { get; set; }
        double FreeBalance { get; }
    }
}
