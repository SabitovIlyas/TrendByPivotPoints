using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Общий контракт хромосомы для форвард-анализа: у хромосомы есть тикер с барами
    /// и список результатов форвард-анализа.
    /// </summary>
    public interface IOptimizableChromosome
    {
        Ticker Ticker { get; }
        List<ForwardAnalysisResult> ForwardAnalysisResults { get; }
    }
}
