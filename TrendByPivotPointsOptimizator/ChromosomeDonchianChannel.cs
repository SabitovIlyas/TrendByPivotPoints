using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator
{
    public class ChromosomeDonchianChannel
    {
        public bool FitnessPassed { get; set; }
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
    }

    public class FitnessDonchianChannel
    {
        public bool IsPassed()
        {
            return false;
        }

        public double Value()
        {
            return 0;
        }
    }
}