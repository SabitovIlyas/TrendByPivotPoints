using System;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Описание одного оптимизируемого параметра стратегии: имя, границы, шаг сетки.
    /// </summary>
    public class ParameterDescriptor
    {
        public string Name { get; }
        public double Min { get; }
        public double Max { get; }
        public double Step { get; }
        public bool IsInteger { get; }

        public double Range => Max - Min;
        public int StepsCount => (int)Math.Round((Max - Min) / Step);

        public ParameterDescriptor(string name, double min, double max, double step = 1,
            bool isInteger = true)
        {
            if (min > max)
                throw new ArgumentException("Минимум больше максимума: " + name);
            if (step <= 0)
                throw new ArgumentException("Шаг должен быть положительным: " + name);

            Name = name;
            Min = min;
            Max = max;
            Step = step;
            IsInteger = isInteger;
        }

        /// <summary>Случайный узел сетки параметра.</summary>
        public double RandomValue(IRandomProvider randomProvider)
        {
            return Snap(Min + randomProvider.Next(StepsCount + 1) * Step);
        }

        /// <summary>Прищёлкивание значения к сетке и границам параметра.</summary>
        public double Snap(double value)
        {
            var clamped = Math.Max(Min, Math.Min(Max, value));
            var steps = Math.Round((clamped - Min) / Step);
            var snapped = Min + steps * Step;
            snapped = Math.Max(Min, Math.Min(Max, snapped));

            if (IsInteger)
                return Math.Round(snapped);
            return Math.Round(snapped, 6);
        }
    }
}
