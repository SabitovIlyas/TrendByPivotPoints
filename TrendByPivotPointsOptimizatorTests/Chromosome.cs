namespace TrendByPivotPointsOptimizator.Tests
{
    public class Chromosome
    {
        public int FastPeriod { get; set; }
        public int SlowPeriod { get; set; }
        public double Fitness { get; set; }

        public Chromosome(int fast, int slow)
        {
            FastPeriod = fast;
            SlowPeriod = slow;
        }
    }
}