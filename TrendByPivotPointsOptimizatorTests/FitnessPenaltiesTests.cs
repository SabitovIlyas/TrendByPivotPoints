using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class FitnessPenaltiesTests
    {
        private FitnessUniversal CreateFitness(double maxDrawDownPrcnt = 0,
            double minWinRatePrcnt = 0, double penaltyPower = 1)
        {
            //Штрафы считаются по числам, прогон стратегии для этого не нужен.
            return new FitnessUniversal(null, null, null, null)
            {
                MaxDrawDownPrcnt = maxDrawDownPrcnt,
                MinWinRatePrcnt = minWinRatePrcnt,
                PenaltyPower = penaltyPower,
            };
        }

        [TestMethod()]
        public void Penalties_DoNothingWhenTurnedOff()
        {
            var fitness = CreateFitness();

            Assert.AreEqual(10, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 90,
                winRatePrcnt: 5));
        }

        [TestMethod()]
        public void Penalties_DoNothingWithinThresholds()
        {
            var fitness = CreateFitness(maxDrawDownPrcnt: 20, minWinRatePrcnt: 50);

            Assert.AreEqual(10, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 15,
                winRatePrcnt: 60));
            //Ровно на пороге штрафа ещё нет.
            Assert.AreEqual(10, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 20,
                winRatePrcnt: 50));
        }

        [TestMethod()]
        public void Penalties_ScaleDownDeepDrawdown()
        {
            var fitness = CreateFitness(maxDrawDownPrcnt: 20);

            //Просадка вдвое глубже порога — оценка вдвое меньше.
            Assert.AreEqual(5, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 40,
                winRatePrcnt: 60));

            //Просадка 89% при пороге 20% — оценка падает почти до нуля.
            Assert.AreEqual(2.25, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 89,
                winRatePrcnt: 60));
        }

        [TestMethod()]
        public void Penalties_ScaleDownLowWinRate()
        {
            var fitness = CreateFitness(minWinRatePrcnt: 50);

            //Выигрышных вдвое меньше порога — оценка вдвое меньше.
            Assert.AreEqual(5, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 10,
                winRatePrcnt: 25));

            //Как в реальном прогоне: 6% выигрышных при пороге 50%.
            Assert.AreEqual(1.2, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 10,
                winRatePrcnt: 6));
        }

        [TestMethod()]
        public void Penalties_MultiplyWithEachOther()
        {
            var fitness = CreateFitness(maxDrawDownPrcnt: 20, minWinRatePrcnt: 50);

            //Просадка вдвое глубже и выигрышных вдвое меньше — оценка вчетверо ниже.
            Assert.AreEqual(2.5, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 40,
                winRatePrcnt: 25));
        }

        [TestMethod()]
        public void Penalties_AreHarsherWithHigherPower()
        {
            var soft = CreateFitness(maxDrawDownPrcnt: 20, penaltyPower: 1);
            var hard = CreateFitness(maxDrawDownPrcnt: 20, penaltyPower: 2);

            Assert.AreEqual(5, soft.ApplyPenalties(10, 40, 60));
            Assert.AreEqual(2.5, hard.ApplyPenalties(10, 40, 60));
        }

        [TestMethod()]
        public void Penalties_MakeLosingStrategiesWorseNotBetter()
        {
            //Умножение отрицательной оценки на долю меньше единицы приблизило бы её
            //к нулю, то есть штраф улучшал бы плохую стратегию.
            var fitness = CreateFitness(maxDrawDownPrcnt: 20);

            var penalized = fitness.ApplyPenalties(-10, maxDrawDownPrcnt: 40,
                winRatePrcnt: 60);

            Assert.AreEqual(-20, penalized);
        }

        [TestMethod()]
        public void Penalties_KeepRejectedChromosomesRejected()
        {
            var fitness = CreateFitness(maxDrawDownPrcnt: 20, minWinRatePrcnt: 50);

            Assert.IsTrue(double.IsNegativeInfinity(
                fitness.ApplyPenalties(double.NegativeInfinity, 40, 25)));
        }

        [TestMethod()]
        public void Penalties_SurviveZeroWinRate()
        {
            //Ни одной выигрышной сделки: деления на ноль быть не должно.
            var fitness = CreateFitness(minWinRatePrcnt: 50);

            Assert.AreEqual(0, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 10,
                winRatePrcnt: 0));
            Assert.IsFalse(double.IsNaN(fitness.ApplyPenalties(-10,
                maxDrawDownPrcnt: 10, winRatePrcnt: 0)));
        }
    }
}
