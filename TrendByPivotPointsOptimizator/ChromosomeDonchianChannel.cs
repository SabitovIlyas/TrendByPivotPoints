using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator
{
    public class ChromosomeDonchianChannel
    {
        public bool FitnessPassed { get; set; }
        public double FitnessValue { get; set; }
        public Ticker Ticker { get; }
        public Interval TimeFrame { get; }
        public PositionSide Side { get; }
        public int FastDonchian { get; }
        public int SlowDonchian { get; }
        public int AtrPeriod { get; }
        public int LimitOpenedPositions { get; }
        public double KAtrForOpenPosition { get; }
        public double KAtrForStopLoss { get; }

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