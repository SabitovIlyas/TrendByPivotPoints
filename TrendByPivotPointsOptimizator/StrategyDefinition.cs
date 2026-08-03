using System;
using System.Collections.Generic;
using TradingSystems;
using TSLab.DataSource;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Описание стратегии для универсального оптимизатора: имя, набор оптимизируемых
    /// параметров, фабрика стартера и починка недопустимых комбинаций генов.
    /// </summary>
    public abstract class StrategyDefinition
    {
        public abstract string Name { get; }
        public abstract List<ParameterDescriptor> Parameters { get; }

        public abstract Starter CreateStarter(Context context, List<Security> securities,
            Logger logger);

        /// <summary>
        /// Починка недопустимых комбинаций генов (например, fast больше slow).
        /// По умолчанию ничего не делает.
        /// </summary>
        public virtual void Repair(Dictionary<string, double> genes) { }

        /// <summary>
        /// Переопределяет диапазоны поиска параметров значениями из файла настроек
        /// (строки «Range:имя:мин:макс:шаг»). Незнакомые имена игнорируются.
        /// </summary>
        public void ApplyRangeOverrides(Settings settings)
        {
            if (settings.ParameterRanges == null || settings.ParameterRanges.Count == 0)
                return;

            var parameters = Parameters;
            for (var i = 0; i < parameters.Count; i++)
            {
                if (settings.ParameterRanges.TryGetValue(parameters[i].Name,
                    out ParameterRange range))
                {
                    parameters[i] = new ParameterDescriptor(parameters[i].Name,
                        range.Min, range.Max, range.Step, parameters[i].IsInteger,
                        parameters[i].IsCategorical);
                }
            }
        }

        /// <summary>
        /// Собирает SystemParameters для стартера из генов хромосомы и общих настроек.
        /// </summary>
        public virtual SystemParameters CreateSystemParameters(Dictionary<string, double> genes,
            Ticker ticker, PositionSide side, Interval timeFrame, Settings settings)
        {
            var parameters = new SystemParameters();

            foreach (var descriptor in Parameters)
            {
                var value = genes[descriptor.Name];
                if (descriptor.IsInteger)
                    parameters.Add(descriptor.Name, (int)Math.Round(value));
                else
                    parameters.Add(descriptor.Name, value);
            }

            if (ticker.IsUSD)
                parameters.Add("isUSD", 1);
            else
                parameters.Add("isUSD", 0);
            parameters.Add("rateUSD", ticker.RateUSD);

            parameters.Add("positionSide", side);
            parameters.Add("timeFrame", timeFrame);
            parameters.Add("shares", ticker.Shares);

            parameters.Add("equity", settings.Equity);
            parameters.Add("riskValuePrcnt", settings.RiskValuePrcnt);
            parameters.Add("contracts", 0);

            return parameters;
        }
    }
}
