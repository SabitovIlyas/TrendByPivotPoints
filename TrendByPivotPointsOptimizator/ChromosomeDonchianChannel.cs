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
        public int DealsCount { get; set; }
        public Ticker Ticker { get; set; }
        public Interval TimeFrame { get; set; }
        public PositionSide Side { get; set; }
        public int FastDonchian { get; set; }
        public int SlowDonchian { get; set; }
        public int AtrPeriod { get; set; }
        public int LimitOpenedPositions { get; set; }
        public double KAtrForOpenPosition { get; set; }
        public double KAtrForStopLoss { get; set; }
        //public bool IsUSD { get; set; }
        //public double RateUSD { get; set; }
        public string Name { get; private set; } = string.Empty;
        public FitnessDonchianChannel FitnessDonchianChannel { get; set; }

        public List<ForwardAnalysisResult> ForwardAnalysisResults { get; set; } = new List<ForwardAnalysisResult>();
        public ChromosomeDonchianChannel(Ticker ticker, Interval timeFrame, PositionSide side, 
            int fastDonchian, int slowDonchian, int atrPeriod, int limitOpenedPositions, 
            double kAtrForOpenPosition, double kAtrForStopLoss)//, bool isUSD, double rateUSD)
        {
            Ticker = ticker;            
            ResetBarsToInitBars();            
            
            TimeFrame = timeFrame;
            Side = side;
            FastDonchian = fastDonchian;
            SlowDonchian = slowDonchian;
            AtrPeriod = atrPeriod;
            LimitOpenedPositions = limitOpenedPositions;
            KAtrForOpenPosition = kAtrForOpenPosition;
            KAtrForStopLoss = kAtrForStopLoss;
            //IsUSD = isUSD;
            //RateUSD = rateUSD;
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
            Name = string.Format(
                "{0}: {1}; " +
                "{2}: {3}; " +
                "{4}: {5}; " +
                "{6}: {7}; " +
                "{8}: {9}; " +
                "{10}: {11}; " +
                "{12}: {13}; " +
                "{14}: {15}; " +
                "{16}: {17};", nameof(Ticker.Name), Ticker.Name, nameof(TimeFrame), TimeFrame, nameof(Side), Side,
                nameof(FastDonchian), FastDonchian, nameof(SlowDonchian), SlowDonchian, nameof(AtrPeriod),
                AtrPeriod, nameof(LimitOpenedPositions), LimitOpenedPositions, nameof(KAtrForOpenPosition),
                KAtrForOpenPosition, nameof(KAtrForStopLoss), KAtrForStopLoss);
        }
    }
}