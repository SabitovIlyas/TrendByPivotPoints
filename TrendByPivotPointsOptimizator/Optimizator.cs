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

        public string GetOptimalParametersPercent(List<PointValue> points, int dimension, int[] radiusNeighbourInPercent, double barrier, bool isCheckedPass)
        {
            var matirxCreator = MatrixCreator.Create(points, dimension, radiusNeighbourInPercent);
            var matrix = matirxCreator.CreateMatrixPercent();

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

            for (int i = 0; i < combinationsPassedBarrier.Count; i++)
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

        public string GetOptimalParametersPercent1(List<PointValue> points, int dimension, int[] radiusNeighbourInPercent, double barrier, bool isCheckedPass)
        {
            var matrixCreator = MatrixCreator.Create(points, dimension, radiusNeighbourInPercent);
            matrixCreator.CreateMatrixPercent();
            var combinations = matrixCreator.Combinations;
            SetIdToCombination(matrixCreator);
            SortCombinationsById(matrixCreator);

            var result = string.Empty;

            for (int i = 0; i < combinations.Count; i++)
            {
                var id = "";
                var coords = matrixCreator.GetCoords(combinations[i].Combination);
                for (var j = 0; j < coords.Length; j++)                
                    result += coords[j] + ";";

                result += matrixCreator.Combinations[i].Id + ";";
                result += combinations[i].Value + ";";
                if (combinations[i].Combination.IsCombinationPassedTestWhenTheyAreAllGreaterOrEqualThenValue(barrier))
                    result += Math.Round(combinations[i].Combination.GetAverageValue(),2) + ";\n";
                else
                    result += ";\n";                
            }
            return result;
        }

        private void SetIdToCombination(MatrixCreator matrixCreator)
        {
            var combinations = matrixCreator.Combinations;
            for (int i = 0; i < combinations.Count; i++)
            {
                var id = string.Empty;
                var coords = matrixCreator.GetCoords(combinations[i].Combination);

                for (var j = 0; j < coords.Length; j++)
                {
                    if (j < coords.Length - 1)
                        id += coords[j] + "000";
                    else
                        id += coords[j];
                }

                matrixCreator.Combinations[i].Id = int.Parse(id);
            }                        
        }

        private void SortCombinationsById(MatrixCreator matrixCreator)
        {
            var combinations = matrixCreator.Combinations;
            var comparer = IdCombinationDecoratorsAscendingComparer.Create();
            combinations.Sort(comparer);
        }       
    }
}