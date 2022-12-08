using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPointsOptimizator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class MatrixCreatorTests
    {

        [TestMethod()]
        public void CreateMatrixTest()
        {
            var points = new List<PointValue>
            {
                PointValue.Create(2, new int[1] { 1 }),
                PointValue.Create(1, new int[1] { 2 }),
                PointValue.Create(4, new int[1] { 3 }),
                PointValue.Create(3, new int[1] { 4 }),
                PointValue.Create(5, new int[1] { 5 }),
                PointValue.Create(4, new int[1] { 6 }),
                PointValue.Create(5, new int[1] { 7 }),
                PointValue.Create(2, new int[1] { 8 })
            };

            var matirxCreator = MatrixCreator.Create(points, dimension: 1, new int[1] { 1 });
            var matrix = matirxCreator.CreateMatrix();

            Assert.IsNotNull(matrix);
            foreach (var element in matrix)
            {
                Assert.AreNotEqual(0, element.GetAverageValue());
            }
        }
    }
}