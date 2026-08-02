using System.Globalization;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Результат прогона одной комбинации генов — только числа, без баров,
    /// стартера и сделок. Кэш оптимизатора хранит именно это: одна и та же
    /// комбинация генов даёт один и тот же результат, а держать ради него
    /// в памяти всю историю прогона незачем.
    /// </summary>
    public class ChromosomeMetrics
    {
        public double FitnessValue { get; set; }
        public double Profit { get; set; }
        public double ProfitPrcnt { get; set; }
        public double RecoveryFactor { get; set; }
        public double MaxDrawDown { get; set; }
        public int DealsCount { get; set; }

        public static ChromosomeMetrics From(ChromosomeUniversal chromosome)
        {
            return new ChromosomeMetrics()
            {
                FitnessValue = chromosome.FitnessValue,
                Profit = chromosome.Profit,
                ProfitPrcnt = chromosome.ProfitPrcnt,
                RecoveryFactor = chromosome.RecoveryFactor,
                MaxDrawDown = chromosome.MaxDrawDown,
                DealsCount = chromosome.DealsCount,
            };
        }

        public void ApplyTo(ChromosomeUniversal chromosome)
        {
            chromosome.FitnessValue = FitnessValue;
            chromosome.Profit = Profit;
            chromosome.ProfitPrcnt = ProfitPrcnt;
            chromosome.RecoveryFactor = RecoveryFactor;
            chromosome.MaxDrawDown = MaxDrawDown;
            chromosome.DealsCount = DealsCount;
        }

        /// <summary>Строка для файла чек-поинта: шесть чисел через точку с запятой.</summary>
        public string ToLine()
        {
            var culture = CultureInfo.InvariantCulture;
            return string.Join(";", new[]
            {
                FitnessValue.ToString("R", culture),
                Profit.ToString("R", culture),
                ProfitPrcnt.ToString("R", culture),
                RecoveryFactor.ToString("R", culture),
                MaxDrawDown.ToString("R", culture),
                DealsCount.ToString(culture),
            });
        }

        public static ChromosomeMetrics Parse(string line)
        {
            var culture = CultureInfo.InvariantCulture;
            var parts = line.Split(';');
            if (parts.Length < 6)
                throw new System.FormatException(
                    "Ожидалось шесть чисел с результатом хромосомы: " + line);

            return new ChromosomeMetrics()
            {
                FitnessValue = double.Parse(parts[0], culture),
                Profit = double.Parse(parts[1], culture),
                ProfitPrcnt = double.Parse(parts[2], culture),
                RecoveryFactor = double.Parse(parts[3], culture),
                MaxDrawDown = double.Parse(parts[4], culture),
                DealsCount = int.Parse(parts[5], culture),
            };
        }
    }
}
