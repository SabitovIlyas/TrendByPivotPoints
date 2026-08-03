using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class TournamentSelectionTests
    {
        private ChromosomeUniversal CreateChromosome(double fitness)
        {
            var bars = new List<Bar>()
            {
                Bar.Create(new System.DateTime(2026, 1, 5), 1, 1, 1, 1, 1, "TEST", "60", 0),
            };
            var ticker = new Ticker("TEST", Currency.RUB, 1, bars, new LoggerNull(),
                commissionRate: 0, isUSD: false, rateUSD: 1);

            var chromosome = new ChromosomeUniversal(ticker,
                new Interval(60, DataIntervals.MINUTE), PositionSide.Long,
                new Dictionary<string, double>() { { "maPeriod", 100 } });
            chromosome.FitnessValue = fitness;
            return chromosome;
        }

        [TestMethod()]
        public void IsBetter_PrefersHigherFitness()
        {
            Assert.IsTrue(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(5), CreateChromosome(3)));
            Assert.IsFalse(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(3), CreateChromosome(5)));
            Assert.IsFalse(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(5), CreateChromosome(5)));
        }

        [TestMethod()]
        public void IsBetter_ReplacesNotCountedChromosome()
        {
            //Главная правка: раньше «не число», вытянутое первым, выигрывало турнир
            //у любой посчитанной хромосомы, потому что сравнение с ним всегда ложно.
            Assert.IsTrue(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(0.1), CreateChromosome(double.NaN)));

            //Даже отбракованная хромосома лучше непосчитанной.
            Assert.IsTrue(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(double.NegativeInfinity),
                CreateChromosome(double.NaN)));
        }

        [TestMethod()]
        public void IsBetter_NeverPrefersNotCountedChromosome()
        {
            Assert.IsFalse(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(double.NaN), CreateChromosome(0.1)));
            Assert.IsFalse(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(double.NaN),
                CreateChromosome(double.NegativeInfinity)));
            Assert.IsFalse(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(double.NaN), CreateChromosome(double.NaN)));
        }

        [TestMethod()]
        public void IsBetter_PrefersAnythingOverRejected()
        {
            Assert.IsTrue(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(-5), CreateChromosome(double.NegativeInfinity)));
            Assert.IsFalse(GeneticAlgorithmUniversal.IsBetter(
                CreateChromosome(double.NegativeInfinity), CreateChromosome(-5)));
        }

        [TestMethod()]
        public void RejectIfNotFinite_TurnsUndefinedIntoRejection()
        {
            //Без сделок фактор восстановления — это 0/0, при нулевой просадке
            //с прибылью — бесконечность. Оценивать в обоих случаях нечего.
            Assert.IsTrue(double.IsNegativeInfinity(
                FitnessUniversal.RejectIfNotFinite(double.NaN)));
            Assert.IsTrue(double.IsNegativeInfinity(
                FitnessUniversal.RejectIfNotFinite(double.PositiveInfinity)));
        }

        [TestMethod()]
        public void RejectIfNotFinite_KeepsNormalValues()
        {
            Assert.AreEqual(3.35, FitnessUniversal.RejectIfNotFinite(3.35));
            Assert.AreEqual(-2, FitnessUniversal.RejectIfNotFinite(-2));
            Assert.AreEqual(0, FitnessUniversal.RejectIfNotFinite(0));
            Assert.IsTrue(double.IsNegativeInfinity(
                FitnessUniversal.RejectIfNotFinite(double.NegativeInfinity)));
        }
    }
}
