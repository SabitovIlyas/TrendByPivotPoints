using System;
using System.Collections.Generic;
using TradingSystems;
using TrendByPivotPointsStarter;
using TSLab.DataSource;
using Security = TradingSystems.Security;

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

        /// <summary>
        /// Делит риск между уровнями пирамиды. Каждый уровень считает контракты
        /// сам и рискует полной долей, поэтому без деления четыре уровня дают до
        /// четырёхкратного риска на сделку. Раньше это гасил общий стоп по границе
        /// канала: он тянется за ценой и снижает риск ранних уровней. С выключенным
        /// канальным выходом у каждого уровня остаётся свой неподвижный стоп, и
        /// риски складываются полностью.
        ///
        /// Деление точно соответствует лимиту при выключенном канале и оставляет
        /// запас при включённом — там суммарный риск падает ещё быстрее.
        /// </summary>
        public override SystemParameters CreateSystemParameters(
            Dictionary<string, double> genes, Ticker ticker, PositionSide side,
            Interval timeFrame, Settings settings)
        {
            var parameters = base.CreateSystemParameters(genes, ticker, side,
                timeFrame, settings);

            var levels = (int)Math.Round(genes["limitOpenedPositions"]);
            if (levels > 1)
                parameters.SetValue("riskValuePrcnt", settings.RiskValuePrcnt / levels);

            return parameters;
        }

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
