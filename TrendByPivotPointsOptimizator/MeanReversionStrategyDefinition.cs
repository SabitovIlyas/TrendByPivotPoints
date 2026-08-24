using System.Collections.Generic;
using TradingSystems;
using TrendByPivotPointsStarter;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Описание стратегии возврата к среднему (RSI + SMA-фильтр + ATR-стоп) для
    /// универсального оптимизатора. Робот торгует только одну сторону.
    ///
    /// Оба порога RSI ищутся в общем диапазоне [5;95] независимо от стороны, а их
    /// взаимный порядок наводит Repair. Раньше порядок задавался непересекающимися
    /// диапазонами (лонг — вход [5;50], выход [50;95]), но это запрещало вход по
    /// высокому порогу, который на замерах оказался единственным, обходящим простое
    /// удержание позиции по фильтру SMA.
    /// </summary>
    public class MeanReversionStrategyDefinition : StrategyDefinition
    {
        public override string Name => "MeanReversion";

        private readonly PositionSide side;
        private readonly List<ParameterDescriptor> parameters;

        public MeanReversionStrategyDefinition(PositionSide side)
        {
            this.side = side;

            parameters = new List<ParameterDescriptor>()
            {
                new ParameterDescriptor("maPeriod", 50, 250),
                new ParameterDescriptor("rsiEntryPeriod", 5, 50),
                new ParameterDescriptor("rsiExitPeriod", 5, 50),
                new ParameterDescriptor("atrPeriod", 5, 50),
                new ParameterDescriptor("rsiEntryLevel", 5, 95),
                new ParameterDescriptor("rsiExitLevel", 5, 95),
                new ParameterDescriptor("atrMultiplier", 0.5, 3.0, step: 0.5, isInteger: false),
                //Переключатели режима: в окрестности не сдвигаются — иначе соседом
                //удачной комбинации оказалась бы принципиально другая стратегия.
                new ParameterDescriptor("useTrailingStop", 0, 1, isCategorical: true),
                new ParameterDescriptor("rsiEntryMode", 0, 2, isCategorical: true),
                new ParameterDescriptor("rsiExitMode", 0, 3, isCategorical: true),
            };
        }

        /// <summary>
        /// Наводит порядок в паре «вход — выход», иначе условие выхода оказывается
        /// выполненным уже в момент входа, пересечение сработать не может, и позиция
        /// пропускает первую возможность закрыться. На замерах по золоту это
        /// случалось на 80–100% входов, причём каждое из двух правил закрывает свой
        /// отказ и по отдельности они недостаточны:
        ///
        /// периоды — выходной RSI быстрее входного даёт до 98% клинчей даже при
        /// правильном порядке уровней;
        /// уровни — при равных периодах порог выхода «позади» порога входа даёт
        /// ровно 100% клинчей, потому что ряд RSI тогда буквально один и тот же.
        ///
        /// Обмен значениями безопасен: у обоих периодов общий диапазон [5;50], у
        /// обоих уровней — [5;95].
        /// </summary>
        public override void Repair(Dictionary<string, double> genes)
        {
            if (genes["rsiExitPeriod"] < genes["rsiEntryPeriod"])
                Swap(genes, "rsiEntryPeriod", "rsiExitPeriod");

            var entryLevel = genes["rsiEntryLevel"];
            var exitLevel = genes["rsiExitLevel"];

            //Лонг закрывается выше входа, шорт — ниже.
            var isOrdered = side == PositionSide.Short
                ? exitLevel < entryLevel
                : exitLevel > entryLevel;

            if (!isOrdered)
                Swap(genes, "rsiEntryLevel", "rsiExitLevel");

            //После обмена пороги могли совпасть — тогда обмен ничего не исправил.
            if (genes["rsiEntryLevel"] == genes["rsiExitLevel"])
                Separate(genes);
        }

        /// <summary>Разводит совпавшие пороги на один пункт, не выходя за [5;95].</summary>
        private void Separate(Dictionary<string, double> genes)
        {
            var step = side == PositionSide.Short ? -1 : 1;
            var exitLevel = genes["rsiExitLevel"] + step;

            if (exitLevel >= 5 && exitLevel <= 95)
                genes["rsiExitLevel"] = exitLevel;
            else
                genes["rsiEntryLevel"] -= step;
        }

        private static void Swap(Dictionary<string, double> genes, string first, string second)
        {
            var temp = genes[first];
            genes[first] = genes[second];
            genes[second] = temp;
        }

        public override List<ParameterDescriptor> Parameters => parameters;

        public override Starter CreateStarter(Context context, List<Security> securities,
            Logger logger)
        {
            return new StarterMeanReversionTradingSystemLab(context, securities, logger);
        }
    }
}
