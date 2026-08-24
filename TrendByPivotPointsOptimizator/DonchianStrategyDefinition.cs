using System.Collections.Generic;
using TradingSystems;
using TrendByPivotPointsStarter;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Описание стратегии прорыва канала Дончиана для универсального оптимизатора.
    /// Диапазоны параметров совпадают с зашитыми в GeneticAlgorithmDonchianChannel.
    /// </summary>
    public class DonchianStrategyDefinition : StrategyDefinition
    {
        public override string Name => "Donchian";

        private readonly List<ParameterDescriptor> parameters = new List<ParameterDescriptor>()
        {
            new ParameterDescriptor("fastDonchian", 10, 100),
            new ParameterDescriptor("slowDonchian", 10, 200),
            new ParameterDescriptor("atrPeriod", 2, 24),
            new ParameterDescriptor("limitOpenedPositions", 1, 4),
            new ParameterDescriptor("kAtrForOpenPosition", 0.5, 3.0, step: 0.5, isInteger: false),
            new ParameterDescriptor("kAtrForStopLoss", 0.5, 3.0, step: 0.5, isInteger: false),
            //Выход по времени: 0 — не закрывать по времени. Верхняя граница взята
            //с запасом к замеренному диапазону преимущества (100–400 баров).
            new ParameterDescriptor("maxBarsInPosition", 0, 800, step: 10),
            //Канальный выход тянется за ценой и закрывает позицию раньше, чем
            //преимущество успевает проявиться; выключенный оставляет только стоп
            //по ATR, отвечающий за риск.
            new ParameterDescriptor("useChannelExit", 0, 1, isCategorical: true),
        };

        public override List<ParameterDescriptor> Parameters => parameters;

        public override void Repair(Dictionary<string, double> genes)
        {
            //Быстрый канал не может быть длиннее медленного.
            if (genes["slowDonchian"] < genes["fastDonchian"])
            {
                var temp = genes["fastDonchian"];
                genes["fastDonchian"] = genes["slowDonchian"];
                genes["slowDonchian"] = temp;
            }
        }

        public override Starter CreateStarter(Context context, List<Security> securities,
            Logger logger)
        {
            return new StarterDonchianTradingSystemLab(context, securities, logger);
        }
    }
}
