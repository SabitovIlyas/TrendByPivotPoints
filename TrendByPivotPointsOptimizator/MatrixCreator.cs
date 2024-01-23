using System;
using System.Collections.Generic;

namespace TrendByPivotPointsOptimizator
{
    public class MatrixCreator
    {
        List<CombinationDecorator> combinations = new List<CombinationDecorator>();
        List<PointValue> points;
        private int dimension;
        private int[] radiusNeighbour;

        public static MatrixCreator Create(List<PointValue> points, int dimension, int[] radiusNeighbour)
        {
            return new MatrixCreator(points, dimension, radiusNeighbour);
        }

        private MatrixCreator(List<PointValue> points, int dimension, int[] radiusNeighbour)
        {
            this.points = points;
            this.dimension = dimension;
            this.radiusNeighbour = radiusNeighbour;
        }

        public List<Combination> CreateMatrix()
        {
            foreach (var point in points)
            {
                var combination = Combination.Create(point.Value);
                var combinationDecorator = CombinationDecorator.Create(combination, point);
                combinations.Add(combinationDecorator);
            }

            foreach (var combination in combinations)
            {
                var neighbourList = new List<CombinationDecorator>();
                var coords = new int[dimension];
                for (var z = 0; z < dimension; z++)
                {
                    coords[z] = combination.GetCoord(z);
                }

                var lowerBand = new int[dimension];
                var upperBand = new int[dimension];

                for (var z = 0; z < dimension; z++)
                {
                    lowerBand[z] = combination.GetCoord(z) - radiusNeighbour[z];
                    upperBand[z] = combination.GetCoord(z) + radiusNeighbour[z];
                }

                FillNeighbourList(combination, neighbourList, lowerBand, upperBand);
                combination.AddNearCombinations(neighbourList);                              
            }

            var result = new List<Combination>();
            foreach (var combinationDecorator in combinations)
                result.Add(combinationDecorator.Combination);
            return result;
        }

        public List<Combination> CreateMatrixPercent()
        {
            foreach (var point in points)
            {
                var combination = Combination.Create(point.Value);
                var combinationDecorator = CombinationDecorator.Create(combination, point);
                combinations.Add(combinationDecorator);
            }

            foreach (var combination in combinations)
            {
                var neighbourList = new List<CombinationDecorator>();
                var coords = new int[dimension];
                for (var z = 0; z < dimension; z++)
                {
                    coords[z] = combination.GetCoord(z);
                }

                var lowerBand = new int[dimension];
                var upperBand = new int[dimension];

                for (var z = 0; z < dimension; z++)
                {
                    lowerBand[z] = (int)(combination.GetCoord(z) - Math.Round(combination.GetCoord(z) * (float)radiusNeighbour[z] / 100, MidpointRounding.AwayFromZero));
                    upperBand[z] = (int)(combination.GetCoord(z) + Math.Round(combination.GetCoord(z) * (float)radiusNeighbour[z] / 100, MidpointRounding.AwayFromZero));
                }

                FillNeighbourList(combination, neighbourList, lowerBand, upperBand);
                combination.AddNearCombinations(neighbourList);
            }

            var result = new List<Combination>();
            foreach (var combinationDecorator in combinations)
                result.Add(combinationDecorator.Combination);
            return result;
        }       

        public int[] GetCoords(Combination combination)
        {
            var result = new int[dimension];
            var cmb = combinations.Find(x => x.Combination == combination);

            for (var i = 0; i < dimension; i++)
                result[i] = cmb.GetCoord(i);
            return result;
        }

        private void FillNeighbourList(CombinationDecorator combination, List<CombinationDecorator> neighbourList, int[] lowerBand, int[] upperBand)
        {
            foreach (var potentialNeighbour in combinations)
            {
                var passed = true;
                for (var z = 0; z < dimension; z++)
                {
                    if (potentialNeighbour.GetCoord(z) < lowerBand[z] || potentialNeighbour.GetCoord(z) > upperBand[z])
                    {
                        passed = false;
                        break;
                    }
                }

                if (passed && potentialNeighbour != combination)
                    neighbourList.Add(potentialNeighbour);
            }
        }
    }
}