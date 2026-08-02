using System.Globalization;
using TradingSystems;

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

        /// <summary>Показатели по сделкам — чтобы они не терялись при попадании
        /// в кэш и при продолжении прогона с чек-поинта.</summary>
        public DealsStatistics DealsStatistics { get; set; } = new DealsStatistics();

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
                DealsStatistics = chromosome.DealsStatistics,
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
            chromosome.DealsStatistics = DealsStatistics;
        }

        /// <summary>Строка для файла чек-поинта: числа через точку с запятой.</summary>
        public string ToLine()
        {
            var culture = CultureInfo.InvariantCulture;
            var s = DealsStatistics ?? new DealsStatistics();

            return string.Join(";", new[]
            {
                FitnessValue.ToString("R", culture),
                Profit.ToString("R", culture),
                ProfitPrcnt.ToString("R", culture),
                RecoveryFactor.ToString("R", culture),
                MaxDrawDown.ToString("R", culture),
                DealsCount.ToString(culture),
                s.WinningDealsCount.ToString(culture),
                s.LosingDealsCount.ToString(culture),
                s.WinRatePrcnt.ToString("R", culture),
                s.AverageWin.ToString("R", culture),
                s.AverageLoss.ToString("R", culture),
                s.PayoffRatio.ToString("R", culture),
                s.ProfitFactor.ToString("R", culture),
                s.ExpectedPayoff.ToString("R", culture),
                s.LargestWin.ToString("R", culture),
                s.LargestLoss.ToString("R", culture),
                s.MaxConsecutiveLosses.ToString(culture),
                s.MaxConsecutiveWins.ToString(culture),
                s.AverageBarsInDeal.ToString("R", culture),
            });
        }

        public static ChromosomeMetrics Parse(string line)
        {
            var culture = CultureInfo.InvariantCulture;
            var parts = line.Split(';');
            if (parts.Length < 6)
                throw new System.FormatException(
                    "Ожидалось не меньше шести чисел с результатом хромосомы: " + line);

            var metrics = new ChromosomeMetrics()
            {
                FitnessValue = double.Parse(parts[0], culture),
                Profit = double.Parse(parts[1], culture),
                ProfitPrcnt = double.Parse(parts[2], culture),
                RecoveryFactor = double.Parse(parts[3], culture),
                MaxDrawDown = double.Parse(parts[4], culture),
                DealsCount = int.Parse(parts[5], culture),
            };

            //Чек-поинт, снятый до появления показателей по сделкам, тоже читаем:
            //прерванный прогон должен продолжаться после обновления оптимизатора.
            if (parts.Length < 19)
            {
                metrics.DealsStatistics = new DealsStatistics()
                {
                    DealsCount = metrics.DealsCount,
                };
                return metrics;
            }

            metrics.DealsStatistics = new DealsStatistics()
            {
                DealsCount = metrics.DealsCount,
                WinningDealsCount = int.Parse(parts[6], culture),
                LosingDealsCount = int.Parse(parts[7], culture),
                WinRatePrcnt = double.Parse(parts[8], culture),
                AverageWin = double.Parse(parts[9], culture),
                AverageLoss = double.Parse(parts[10], culture),
                PayoffRatio = double.Parse(parts[11], culture),
                ProfitFactor = double.Parse(parts[12], culture),
                ExpectedPayoff = double.Parse(parts[13], culture),
                LargestWin = double.Parse(parts[14], culture),
                LargestLoss = double.Parse(parts[15], culture),
                MaxConsecutiveLosses = int.Parse(parts[16], culture),
                MaxConsecutiveWins = int.Parse(parts[17], culture),
                AverageBarsInDeal = double.Parse(parts[18], culture),
            };

            return metrics;
        }
    }
}
