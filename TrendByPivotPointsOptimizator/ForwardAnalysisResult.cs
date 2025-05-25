using System;

namespace TrendByPivotPointsOptimizator
{
    public class ForwardAnalysisResult
    {
        public double BackwardFitness { get; set; }
        public double ForwardFitness { get; set; }
        public DateTime BackwardStart { get; set; }
        public DateTime BackwardEnd { get; set; }
        public DateTime ForwardStart { get; set; }
        public DateTime ForwardEnd { get; set; }
    }
}
