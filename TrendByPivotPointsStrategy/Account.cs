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
        Currency Currency { get; }
        //double Rate { get; set; }
        void Update(int barNumber);
        void Initialize(List<Security> securities);
        Logger Logger { get; set; }
        double FreeBalance { get; }
    }
}
