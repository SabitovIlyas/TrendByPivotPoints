using System;

namespace TrendByPivotPointsOptimizator
{
    public class RandomProvider : IRandomProvider
    {
        private Random random = new Random();
        public double NextDouble() => random.NextDouble();
        public int Next(int maxValue) => random.Next(maxValue);
        public int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);       
    }
}