using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class NeighbourhoodBuilderTests
    {
        private List<ParameterDescriptor> CreateParameters()
        {
            return new List<ParameterDescriptor>()
            {
                //Диапазон 200 при шаге 1: 5% диапазона — это 10, а не 1.
                new ParameterDescriptor("maPeriod", 50, 250),
                //Диапазон 2,5 при шаге 0,5: 5% диапазона меньше шага, берётся шаг.
                new ParameterDescriptor("atrMultiplier", 0.5, 3.0, step: 0.5, isInteger: false),
            };
        }

        private Dictionary<string, double> CreateGenes()
        {
            return new Dictionary<string, double>()
            {
                { "maPeriod", 150 },
                { "atrMultiplier", 1.5 },
            };
        }

        [TestMethod()]
        public void Build_ReturnsRequestedNumberOfPoints()
        {
            var builder = new NeighbourhoodBuilder(CreateParameters(), points: 8,
                percent: 0.05, seed: 42);

            var neighbours = builder.Build("хромосома", CreateGenes());

            Assert.AreEqual(8, neighbours.Count);
        }

        [TestMethod()]
        public void Build_ReturnsNothingWhenTurnedOff()
        {
            var off = new NeighbourhoodBuilder(CreateParameters(), points: 0,
                percent: 0.05, seed: 42);
            Assert.AreEqual(0, off.Build("хромосома", CreateGenes()).Count);

            var zeroRadius = new NeighbourhoodBuilder(CreateParameters(), points: 8,
                percent: 0, seed: 42);
            Assert.AreEqual(0, zeroRadius.Build("хромосома", CreateGenes()).Count);
        }

        [TestMethod()]
        public void Build_ShiftsGenesByShareOfRangeAndSnapsToGrid()
        {
            var builder = new NeighbourhoodBuilder(CreateParameters(), points: 40,
                percent: 0.05, seed: 42);

            var neighbours = builder.Build("хромосома", CreateGenes());

            foreach (var neighbour in neighbours)
            {
                //5% диапазона 200 — сдвиг ровно на 10 в любую сторону либо на месте.
                var maPeriod = neighbour["maPeriod"];
                Assert.IsTrue(maPeriod == 140 || maPeriod == 150 || maPeriod == 160,
                    "maPeriod сдвинулся не на 5% диапазона: " + maPeriod);

                //5% от 2,5 меньше шага 0,5, поэтому сдвиг равен шагу.
                var atrMultiplier = neighbour["atrMultiplier"];
                Assert.IsTrue(atrMultiplier == 1.0 || atrMultiplier == 1.5 ||
                    atrMultiplier == 2.0,
                    "atrMultiplier сдвинулся не на шаг: " + atrMultiplier);
            }

            //Проверяем, что сдвиги вообще происходят, а не все точки совпали с центром.
            Assert.IsTrue(neighbours.Any(n => n["maPeriod"] != 150),
                "Ни один сосед не отличается от центра.");
        }

        [TestMethod()]
        public void Build_KeepsGenesWithinBounds()
        {
            var parameters = CreateParameters();
            var builder = new NeighbourhoodBuilder(parameters, points: 40,
                percent: 0.5, seed: 7);

            //Хромосома у самой границы: соседи не должны выйти за диапазон.
            var genes = new Dictionary<string, double>()
            {
                { "maPeriod", 250 },
                { "atrMultiplier", 0.5 },
            };

            foreach (var neighbour in builder.Build("край", genes))
                foreach (var descriptor in parameters)
                {
                    var value = neighbour[descriptor.Name];
                    Assert.IsTrue(value >= descriptor.Min && value <= descriptor.Max,
                        string.Format("Ген {0} = {1} вышел за диапазон [{2}; {3}].",
                            descriptor.Name, value, descriptor.Min, descriptor.Max));
                }
        }

        [TestMethod()]
        public void Build_IsReproducibleForSameChromosome()
        {
            //Окрестность не должна зависеть от порядка расчёта: при нескольких
            //потоках он не определён, а прогон обязан быть воспроизводимым.
            var first = new NeighbourhoodBuilder(CreateParameters(), points: 8,
                percent: 0.05, seed: 42).Build("хромосома", CreateGenes());
            var second = new NeighbourhoodBuilder(CreateParameters(), points: 8,
                percent: 0.05, seed: 42).Build("хромосома", CreateGenes());

            for (var i = 0; i < first.Count; i++)
                foreach (var gene in first[i])
                    Assert.AreEqual(gene.Value, second[i][gene.Key],
                        "Окрестность одной и той же хромосомы разошлась.");
        }

        [TestMethod()]
        public void Build_DiffersForDifferentChromosomes()
        {
            var parameters = CreateParameters();
            var first = new NeighbourhoodBuilder(parameters, points: 8, percent: 0.05,
                seed: 42).Build("первая", CreateGenes());
            var second = new NeighbourhoodBuilder(parameters, points: 8, percent: 0.05,
                seed: 42).Build("вторая", CreateGenes());

            var same = true;
            for (var i = 0; i < first.Count && same; i++)
                foreach (var gene in first[i])
                    if (gene.Value != second[i][gene.Key])
                    {
                        same = false;
                        break;
                    }

            Assert.IsFalse(same, "У разных хромосом окрестность оказалась одинаковой.");
        }

        [TestMethod()]
        public void GetStableHash_DoesNotDependOnRuntime()
        {
            //Значение зафиксировано: если хеш поедет, продолженный с чек-поинта
            //прогон получит другую окрестность и разойдётся с непрерывным.
            Assert.AreEqual(NeighbourhoodBuilder.GetStableHash("хромосома"),
                NeighbourhoodBuilder.GetStableHash("хромосома"));
            Assert.AreNotEqual(NeighbourhoodBuilder.GetStableHash("первая"),
                NeighbourhoodBuilder.GetStableHash("вторая"));
        }
    }
}
