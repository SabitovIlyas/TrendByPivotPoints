using System;
using System.Collections.Generic;
using TradingSystems;

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
        public List<Bar> BackwardBars { get; set; }
        public List<Bar> ForwardBars { get; set; }
        public double BackwardProfit { get; set; }
        public double ForwardProfit { get; set; }
        public double BackwardProfitPrcnt { get; set; }
        public double ForwardProfitPrcnt { get; set; }
    }
}
