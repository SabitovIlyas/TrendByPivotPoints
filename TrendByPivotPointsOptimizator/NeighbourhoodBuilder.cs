using System;
using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Строит окрестность хромосомы — точки рядом с ней в пространстве параметров.
    /// Нужна, чтобы оценивать не одиночную точку, а плато вокруг неё: набор
    /// параметров, который разваливается от сдвига на шаг, торговать нельзя.
    ///
    /// Сосед — это сдвиг каждого гена на долю его диапазона в случайную сторону.
    /// Доля диапазона, а не шаг сетки: у maPeriod шаг 1 при диапазоне 200 — это
    /// 0,5% и проверка ни о чём, а у atrMultiplier шаг 0,5 при диапазоне 2,5 —
    /// это уже 20%. Сдвиг приклеивается к сетке параметра, чтобы соседи попадали
    /// в те же узлы, что и хромосомы, и переиспользовались через кэш.
    /// </summary>
    public class NeighbourhoodBuilder
    {
        private readonly List<ParameterDescriptor> parameters;
        private readonly int points;
        private readonly double percent;
        private readonly int seed;

        public NeighbourhoodBuilder(List<ParameterDescriptor> parameters, int points,
            double percent, int seed)
        {
            this.parameters = parameters;
            this.points = points;
            this.percent = percent;
            this.seed = seed;
        }

        /// <summary>
        /// Соседи хромосомы. Случайность берётся из имени хромосомы и сида прогона,
        /// а не из общего генератора: иначе окрестность зависела бы от порядка
        /// расчёта, а он при нескольких потоках не определён.
        /// </summary>
        public List<Dictionary<string, double>> Build(string chromosomeName,
            Dictionary<string, double> genes)
        {
            var neighbours = new List<Dictionary<string, double>>();
            if (points <= 0 || percent <= 0)
                return neighbours;

            var random = new Random(GetStableHash(chromosomeName) ^ seed);

            for (var i = 0; i < points; i++)
            {
                var neighbour = new Dictionary<string, double>(genes);

                foreach (var descriptor in parameters)
                {
                    if (!neighbour.ContainsKey(descriptor.Name))
                        continue;

                    //Переключатели режима оставляем как есть: у них нет «чуть-чуть
                    //в сторону», а смена режима — это не сосед, а другая стратегия.
                    if (descriptor.IsCategorical)
                        continue;

                    //Сдвиг не меньше шага: иначе на узких диапазонах сосед
                    //совпадёт с центром и проверять будет нечего.
                    var offset = Math.Max(descriptor.Step, descriptor.Range * percent);
                    var direction = random.Next(3) - 1;      //-1, 0 или +1

                    neighbour[descriptor.Name] = descriptor.Snap(
                        neighbour[descriptor.Name] + direction * offset);
                }

                neighbours.Add(neighbour);
            }

            return neighbours;
        }

        /// <summary>
        /// Хеш строки, одинаковый от запуска к запуску (FNV-1a). Встроенный
        /// GetHashCode на это не годится: он не обязан совпадать между процессами,
        /// и продолженный с чек-поинта прогон получил бы другую окрестность.
        /// </summary>
        public static int GetStableHash(string text)
        {
            unchecked
            {
                var hash = (int)2166136261;
                foreach (var c in text)
                    hash = (hash ^ c) * 16777619;
                return hash;
            }
        }
    }
}
