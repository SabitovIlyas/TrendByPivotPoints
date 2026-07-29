using System.Collections.Generic;
using TradingSystems;
using TrendByPivotPointsStarter;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Описание стратегии возврата к среднему (RSI + SMA-фильтр + ATR-стоп) для
    /// универсального оптимизатора. Стратегия торгует одну сторону, поэтому пороги
    /// RSI общие: для шорта они применяются к зеркальному RSI' = 100 − RSI.
    /// </summary>
    public class MeanReversionStrategyDefinition : StrategyDefinition
    {
        public override string Name => "MeanReversion";

        private readonly List<ParameterDescriptor> parameters = new List<ParameterDescriptor>()
        {
            new ParameterDescriptor("maPeriod", 50, 250),
            new ParameterDescriptor("rsiEntryPeriod", 5, 50),
            new ParameterDescriptor("rsiExitPeriod", 5, 50),
            new ParameterDescriptor("atrPeriod", 5, 50),
            new ParameterDescriptor("rsiEntryLevel", 5, 50),
            new ParameterDescriptor("rsiExitLevel", 50, 95),
            new ParameterDescriptor("atrMultiplier", 0.5, 3.0, step: 0.5, isInteger: false),
            new ParameterDescriptor("useTrailingStop", 0, 1),
        };

        public override List<ParameterDescriptor> Parameters => parameters;

        public override Starter CreateStarter(Context context, List<Security> securities,
            Logger logger)
        {
            return new StarterMeanReversionTradingSystemLab(context, securities, logger);
        }
    }
}
