using System.Collections.Generic;
using System.Dynamic;
using System.Xml;

namespace TrendByPivotPointsOptimizator
{
    public class Combination
    {
        public double Value { get; private set; }
        private List<Combination> nearCombinations = new List<Combination>();
        public static Combination Create(double value)
        {
            return new Combination(value);
        }
        private Combination(double value)
        {
            Value = value;
        }

        public bool IsCombinationPassedTestWhenTheyAreAllGreaterOrEqualThenValue(double barrier)
        {
            if (Value < barrier)
                return false;
            foreach (var combination in nearCombinations)            
                if (combination.Value < barrier)
                    return false;            

            return true;
        }

        public double GetAverageValue()
        {
            var sum = Value;            

            foreach (var combination in nearCombinations)
                sum+= combination.Value;
            return sum / (nearCombinations.Count + 1);
        }

        public void AddNearCombination(Combination combination)
        {
            nearCombinations.Add(combination);
        }
    }
}