using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator
{
    public class ChromosomeDonchianChannel
    {
        public bool FitnessPassed { get { return !double.IsNegativeInfinity(FitnessValue); } }
        public double FitnessValue { get; set; }
        public Ticker Ticker { get; set; }
        public Interval TimeFrame { get; set; }
        public PositionSide Side { get; set; }
        public int FastDonchian { get; set; }
        public int SlowDonchian { get; set; }
        public int AtrPeriod { get; set; }
        public int LimitOpenedPositions { get; set; }
        public double KAtrForOpenPosition { get; set; }
        public double KAtrForStopLoss { get; set; }

        public List<ForwardAnalysisResult> ForwardAnalysisResults { get; set; } = new List<ForwardAnalysisResult>();
        public ChromosomeDonchianChannel(Ticker ticker, Interval timeFrame, PositionSide side, int fastDonchian, int slowDonchian, int atrPeriod, int limitOpenedPositions, double kAtrForOpenPosition, double kAtrForStopLoss)
        {
            Ticker = ticker;
            TimeFrame = timeFrame;
            Side = side;
            FastDonchian = fastDonchian;
            SlowDonchian = slowDonchian;
            AtrPeriod = atrPeriod;
            LimitOpenedPositions = limitOpenedPositions;
            KAtrForOpenPosition = kAtrForOpenPosition;
            KAtrForStopLoss = kAtrForStopLoss;
        }

        public void SetBackwardBarsAsTickerBars()
        {
            if (ForwardAnalysisResults.Any() && ForwardAnalysisResults.First().BackwardBars != null)
                Ticker.Bars = ForwardAnalysisResults.First().BackwardBars;
        }

        public void SetForwardBarsAsTickerBars()
        {
            if (ForwardAnalysisResults.Any() && ForwardAnalysisResults.First().ForwardBars != null)
                Ticker.Bars = ForwardAnalysisResults.First().ForwardBars;
        }
    }
}