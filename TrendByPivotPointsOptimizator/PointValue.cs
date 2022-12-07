using System;

namespace TrendByPivotPointsOptimizator
{
    public class PointValue
    {
        public double Value { get; private set; }

        int dimension = 0;
        int[] coords;

        public static PointValue Create(double value, int[]coords)
        {
            return new PointValue(value, coords);
        }
        private PointValue(double value, int[] coords)
        {
            dimension = coords.Length;            
            this.coords = coords;
            Value = value;
        }

        public int GetCoord(int index)
        {
            if (index < dimension)
                return coords[index];

            throw new Exception("Запрашиваемая координата больше измерения матрицы");
        }
    }
}