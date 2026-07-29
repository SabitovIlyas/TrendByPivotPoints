using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Универсальная хромосома: набор генов «имя параметра — значение».
    /// Не зависит от конкретной стратегии — состав генов задаёт StrategyDefinition.
    /// </summary>
    public class ChromosomeUniversal : IOptimizableChromosome
    {
        public bool FitnessPassed { get { return !double.IsNegativeInfinity(FitnessValue); } }
        public double FitnessValue { get; set; } = double.NaN;
        public double Profit { get; set; } = double.NaN;
        public double ProfitPrcnt { get; set; } = double.NaN;
        public double RecoveryFactor { get; set; } = double.NaN;
        public double MaxDrawDown { get; set; } = double.NaN;
        public int DealsCount { get; set; } = 0;
        public Ticker Ticker { get; set; }
        public Interval TimeFrame { get; set; }
        public PositionSide Side { get; set; }
        public Dictionary<string, double> Genes { get; }
        public string Name { get; private set; } = string.Empty;
        public FitnessUniversal Fitness { get; set; }

        public List<ForwardAnalysisResult> ForwardAnalysisResults { get; set; } =
            new List<ForwardAnalysisResult>();

        public ChromosomeUniversal(Ticker ticker, Interval timeFrame, PositionSide side,
            Dictionary<string, double> genes)
        {
            Ticker = ticker;
            ResetBarsToInitBars();

            TimeFrame = timeFrame;
            Side = side;
            Genes = genes;

            UpdateName();
        }

        public void ResetBarsToInitBars()
        {
            Ticker.ResetBarsToInitBars();
        }

        public void SetBackwardBarsAsTickerBars()
        {
            if (ForwardAnalysisResults.Any() && ForwardAnalysisResults.First().BackwardBars != null)
                Ticker.Bars = ForwardAnalysisResults.Last().BackwardBars;
        }

        public void SetForwardBarsAsTickerBars()
        {
            if (ForwardAnalysisResults.Any() && ForwardAnalysisResults.First().ForwardBars != null)
                Ticker.Bars = ForwardAnalysisResults.Last().ForwardBars;
        }

        public void UpdateName()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Ticker: {0}; TimeFrame: {1}; Side: {2};", Ticker.Name,
                TimeFrame, Side);

            //Гены — в отсортированном порядке, чтобы имя (ключ кэша) было детерминированным.
            foreach (var gene in Genes.OrderBy(g => g.Key))
                builder.AppendFormat(" {0}: {1};", gene.Key, gene.Value);

            Name = builder.ToString();
        }
    }
}
