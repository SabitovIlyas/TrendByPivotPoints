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
    public class OptimizatorTests
    {
        [TestMethod()]
        public void GetOptimalParametersTestDimension1()
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

            var exptected = "4,67: (6); 4: (4); 4: (5); 3,67: (7); 3,5: (8)";
            var optimizator = Optimizator.Create();
            var actual =
                optimizator.GetOptimalParameters(points, dimension: 1, radiusNeighbour: new int[1] {1},
                barrier: 2, isCheckedPass: true);
            Assert.AreEqual(exptected, actual);
        }

        [TestMethod()]
        public void GetOptimalParametersTestDimension2()
        {
            var points = new List<PointValue>
            {
                PointValue.Create(4.76, new int[2] { 21,79 }),
                PointValue.Create(3.95, new int[2] { 21,78 }),
                PointValue.Create(3.66, new int[2] { 21,77 }),
                PointValue.Create(2.23, new int[2] { 22,79 }),
                PointValue.Create(6.39, new int[2] { 22, 78}),
                PointValue.Create(3.67, new int[2] { 22, 77 }),
                PointValue.Create(-0.32, new int[2] { 23, 79 }),
                PointValue.Create(4.9, new int[2] { 23, 78 }),
                PointValue.Create(4.08, new int[2] { 23, 77 })
            };

            var exptected = "4,76: (23; 77); 4,44: (22; 77); 4,42: (21; 77); 4,33: (21; 79); 4,11: (21; 78)";
            var optimizator = Optimizator.Create();
            var actual =
                optimizator.GetOptimalParameters(points, dimension: 2, radiusNeighbour: new int[2] { 1,1 },
                barrier: 1, isCheckedPass: true);
            Assert.AreEqual(exptected, actual);
        }

        [TestMethod()]
        public void GetOptimalParametersPercent()
        {
            var points = new List<PointValue>
            {
                PointValue.Create(4.76, new int[2] { 21, 11 }),
                PointValue.Create(3.95, new int[2] { 21, 10 }),
                PointValue.Create(3.66, new int[2] { 21, 9 }),
                PointValue.Create(2.23, new int[2] { 22, 11 }),
                PointValue.Create(6.39, new int[2] { 22, 10}),
                PointValue.Create(3.67, new int[2] { 22, 9 }),
                PointValue.Create(-0.32, new int[2] { 23, 11 }),
                PointValue.Create(4.9, new int[2] { 23, 10 }),
                PointValue.Create(4.08, new int[2] { 23, 9 })
            };

            var exptected = "4,44: (21; 9); 4,44: (22; 9); 4,44: (23; 9)";
            var optimizator = Optimizator.Create();
            var actual =
                optimizator.GetOptimalParametersPercentDeprecated(points, dimension: 2, radiusNeighbourInPercent: new int[2] { 10, 10 },
                barrier: 1, isCheckedPass: true);
            Assert.AreEqual(exptected, actual);
        }

        //[TestMethod()]
        //public void SortTest()
        //{
        //    ma

        //    var points = new List<PointValue>
        //    {
        //        PointValue.Create(4.76, new int[2] { 21, 11 }),
        //        PointValue.Create(3.95, new int[2] { 21, 10 }),
        //        PointValue.Create(3.66, new int[2] { 21, 9 }),
        //        PointValue.Create(2.23, new int[2] { 22, 11 }),
        //        PointValue.Create(6.39, new int[2] { 22, 10}),
        //        PointValue.Create(3.67, new int[2] { 22, 9 }),
        //        PointValue.Create(-0.32, new int[2] { 23, 11 }),
        //        PointValue.Create(4.9, new int[2] { 23, 10 }),
        //        PointValue.Create(4.08, new int[2] { 23, 9 })
        //    };

        //    var exptected = "4,44: (21; 9); 4,44: (22; 9); 4,44: (23; 9)";
        //    var optimizator = Optimizator.Create();
        //    var actual =
        //        optimizator.GetOptimalParametersPercent(points, dimension: 2, radiusNeighbourInPercent: new int[2] { 10, 10 },
        //        barrier: 1, isCheckedPass: true);
        //    Assert.AreEqual(exptected, actual);
        //}
    }
}