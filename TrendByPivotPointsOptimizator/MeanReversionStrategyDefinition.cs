using System.Collections.Generic;
using TradingSystems;
using TrendByPivotPointsStarter;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Описание стратегии возврата к среднему (RSI + SMA-фильтр + ATR-стоп) для
    /// универсального оптимизатора. Робот торгует только одну сторону; пороги RSI
    /// сторон независимы, поэтому диапазоны поиска зависят от стороны прогона:
    /// лонг — вход [5;50], выход [50;95]; шорт — вход [50;95], выход [5;50].
    /// </summary>
    public class MeanReversionStrategyDefinition : StrategyDefinition
    {
        public override string Name => "MeanReversion";

        private readonly List<ParameterDescriptor> parameters;

        public MeanReversionStrategyDefinition(PositionSide side)
        {
            var rsiEntryLevel = side == PositionSide.Short
                ? new ParameterDescriptor("rsiEntryLevel", 50, 95)  //вход при RSI выше уровня
                : new ParameterDescriptor("rsiEntryLevel", 5, 50);  //вход при RSI ниже уровня

            var rsiExitLevel = side == PositionSide.Short
                ? new ParameterDescriptor("rsiExitLevel", 5, 50)    //выход при пересечении сверху вниз
                : new ParameterDescriptor("rsiExitLevel", 50, 95);  //выход при пересечении снизу вверх

            parameters = new List<ParameterDescriptor>()
            {
                new ParameterDescriptor("maPeriod", 50, 250),
                new ParameterDescriptor("rsiEntryPeriod", 5, 50),
                new ParameterDescriptor("rsiExitPeriod", 5, 50),
                new ParameterDescriptor("atrPeriod", 5, 50),
                rsiEntryLevel,
                rsiExitLevel,
                new ParameterDescriptor("atrMultiplier", 0.5, 3.0, step: 0.5, isInteger: false),
                new ParameterDescriptor("useTrailingStop", 0, 1),
            };
        }

        public override List<ParameterDescriptor> Parameters => parameters;

        public override Starter CreateStarter(Context context, List<Security> securities,
            Logger logger)
        {
            return new StarterMeanReversionTradingSystemLab(context, securities, logger);
        }
    }
}
