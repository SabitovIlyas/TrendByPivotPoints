using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script.Handlers;

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
            List<PointValue> points, int dimension, int[] radiusNeighbour, double barrier, bool isCheckedPass)
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

            IComparer<Combination> comparer = CombinationsDescendingComparer.Create();
            combinationsPassedBarrier.Sort(comparer);

            //var indexes = new List<int>();
            //for (var i = 0; i < combinationsPassedBarrier.Count; i++)
            //    if (combinationsPassedBarrier[i].GetAverageValue() == max)
            //        indexes.Add(i);

            //var result = Math.Round(max, 2) + ": ";

            //for(var ind = 0; ind < indexes.Count;ind++)
            //{
            //    var coords = matirxCreator.GetCoords(combinationsPassedBarrier[indexes[ind]]);
            //    for (var i = 0; i < coords.Length; i++)
            //    {
            //        if (i == 0)
            //            result += "(";
            //        if (i < coords.Length - 1)
            //            result = result + coords[i] + "; ";
            //        else
            //            result += coords[i] + ")";
            //    }

            //    if (ind < indexes.Count - 1)              
            //        result += "; ";                
            //}



            var result = string.Empty;

            for (int i = 0;i< combinationsPassedBarrier.Count;i++)
            {
                result += Math.Round(combinationsPassedBarrier[i].GetAverageValue(), 2) + ": ";

                var coords = matirxCreator.GetCoords(combinationsPassedBarrier[i]);
                for (var j = 0; j < coords.Length; j++)
                {
                    if (j == 0)
                        result += "(";
                    if (j < coords.Length - 1)
                        result = result + coords[j] + "; ";
                    else
                        result += coords[j] + ")";
                }

                if (i < combinationsPassedBarrier.Count - 1)
                    result += "; ";
            }
            return result;
        }
    }

    public class CombinationsDescendingComparer : IComparer<Combination>
    {
        public static CombinationsDescendingComparer Create()
        {
            return new CombinationsDescendingComparer();
        }

        private CombinationsDescendingComparer() { }

        public int Compare(Combination x, Combination y)
        {
            var comparer = CombinationsAscendingComparer.Create();
            return -comparer.Compare(x, y);
        }
    }

    public class CombinationsAscendingComparer : IComparer<Combination>
    {
        public static CombinationsAscendingComparer Create()
        {
            return new CombinationsAscendingComparer();
        }

        private CombinationsAscendingComparer() { }
        
        public int Compare(Combination x, Combination y)
        {
            try
            {
                var combination1 = x as Combination;
                var combination2 = y as Combination;

                if (combination1.GetAverageValue() > combination2.GetAverageValue())
                    return 1;
                else if (combination1.GetAverageValue() < combination2.GetAverageValue())
                    return -1;

                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}