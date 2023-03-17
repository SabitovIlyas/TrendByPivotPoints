﻿using System;
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
}