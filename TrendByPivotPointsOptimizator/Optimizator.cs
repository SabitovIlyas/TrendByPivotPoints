using System;
using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class Optimizator
    {
        public static Optimizator Create()
        {
            return new Optimizator();
        }

        private Optimizator() { }

        public string GetOptimalParameters(
            List<PointValue> points, int dimension, int radiusNeighbour, double barrier, bool isCheckedPass)
        {
            var matirxCreator = MatrixCreator.Create(points, dimension, radiusNeighbour);
            var matrix = matirxCreator.CreateMatrix();


            var combinationsPassedBarrier = new List<Combination>();
            foreach (var combination in matrix)
                if (isCheckedPass)
                {
                    if (combination.IsCombinationPassedTestWhenTheyAreAllGreaterOrEqualThenValue(barrier))
                        combinationsPassedBarrier.Add(combination);
                }
                else
                    combinationsPassedBarrier.Add(combination);

            var max = 0d;
            foreach (var element in combinationsPassedBarrier)
            {
                var averageValue = element.GetAverageValue();
                if (averageValue > max)                
                    max = element.GetAverageValue();                               
            }

            var indexes = new List<int>();
            for (var i = 0; i < combinationsPassedBarrier.Count; i++)
                if (combinationsPassedBarrier[i].GetAverageValue() == max)
                    indexes.Add(i);

            var result = Math.Round(max, 2) + ": ";

            for(var ind = 0; ind < indexes.Count;ind++)
            {
                var coords = matirxCreator.GetCoords(combinationsPassedBarrier[indexes[ind]]);
                for (var i = 0; i < coords.Length; i++)
                {
                    if (i == 0)
                        result += "(";
                    if (i < coords.Length - 1)
                        result = result + coords[i] + "; ";
                    else
                        result += coords[i] + ")";
                }

                if (ind < indexes.Count - 1)              
                    result += "; ";                
            }
            return result;
        }
    }
}