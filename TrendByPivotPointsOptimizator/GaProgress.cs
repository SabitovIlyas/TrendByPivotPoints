using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Состояние генетического алгоритма на границе поколений: всё, что нужно,
    /// чтобы продолжить прогон с этого места.
    /// </summary>
    public class GaProgress
    {
        /// <summary>Сколько поколений уже завершено.</summary>
        public int Generation { get; set; }

        public double BestFitnessEver { get; set; }
        public int GenerationsWithoutImprovement { get; set; }
        public List<ChromosomeUniversal> Population { get; set; }
    }
}
