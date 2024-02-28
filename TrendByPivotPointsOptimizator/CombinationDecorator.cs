using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class CombinationDecorator
    {
        public Combination Combination { get; private set; }
        public int Id { get; set; }
        public double Value { get {  return Combination.Value; } }


        private PointValue point;  

        public static CombinationDecorator Create(Combination combination, PointValue point)
        {            
            return new CombinationDecorator(combination, point);
        }

        private CombinationDecorator(Combination combination, PointValue point)
        {
            Combination = combination;
            this.point = point;
        }

        public int GetCoord(int index)
        {
            return point.GetCoord(index);
        }

        public void AddNearCombinations(List<CombinationDecorator> combinations)
        {
            foreach (var combination in combinations)            
                Combination.AddNearCombination(combination.Combination);            
        }
    }
}