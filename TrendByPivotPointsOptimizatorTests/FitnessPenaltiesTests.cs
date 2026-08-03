using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class FitnessPenaltiesTests
    {
        //Достаточно сделок, чтобы штраф за малую выборку не вмешивался.
        private const int EnoughDeals = 1000;

        private FitnessUniversal CreateFitness(double maxDrawDownPrcnt = 0,
            double minWinRatePrcnt = 0, double penaltyPower = 1, int minDealsCount = 0)
        {
            //Штрафы считаются по числам, прогон стратегии для этого не нужен.
            return new FitnessUniversal(null, null, null, null)
            {
                MaxDrawDownPrcnt = maxDrawDownPrcnt,
                MinWinRatePrcnt = minWinRatePrcnt,
                PenaltyPower = penaltyPower,
                MinDealsCount = minDealsCount,
            };
        }

        [TestMethod()]
        public void Penalties_DoNothingWhenTurnedOff()
        {
            var fitness = CreateFitness();

            Assert.AreEqual(10, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 90,
                winRatePrcnt: 5, dealsCount: 3));
        }

        [TestMethod()]
        public void Penalties_DoNothingWithinThresholds()
        {
            var fitness = CreateFitness(maxDrawDownPrcnt: 20, minWinRatePrcnt: 50,
                minDealsCount: 30);

            Assert.AreEqual(10, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 15,
                winRatePrcnt: 60, dealsCount: 100));

            //Ровно на пороге штрафа ещё нет.
            Assert.AreEqual(10, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 20,
                winRatePrcnt: 50, dealsCount: 30));
        }

        [TestMethod()]
        public void Penalties_ScaleDownDeepDrawdown()
        {
            var fitness = CreateFitness(maxDrawDownPrcnt: 20);

            //Просадка вдвое глубже порога — оценка вдвое меньше.
            Assert.AreEqual(5, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 40,
                winRatePrcnt: 60, dealsCount: EnoughDeals));

            //Просадка 89% при пороге 20% — оценка падает почти до нуля.
            Assert.AreEqual(2.25, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 89,
                winRatePrcnt: 60, dealsCount: EnoughDeals));
        }

        [TestMethod()]
        public void Penalties_ScaleDownLowWinRate()
        {
            var fitness = CreateFitness(minWinRatePrcnt: 50);

            //Выигрышных вдвое меньше порога — оценка вдвое меньше.
            Assert.AreEqual(5, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 10,
                winRatePrcnt: 25, dealsCount: EnoughDeals));

            //Как в реальном прогоне: 6% выигрышных при пороге 50%.
            Assert.AreEqual(1.2, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 10,
                winRatePrcnt: 6, dealsCount: EnoughDeals));
        }

        [TestMethod()]
        public void Penalties_ScaleDownSmallSample()
        {
            var fitness = CreateFitness(minDealsCount: 30);

            //Ровно те случаи из прогона, которые давали лучшие оценки на бэктесте
            //и полный провал на форварде: три-пять сделок за четыре года.
            Assert.AreEqual(1, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 10,
                winRatePrcnt: 60, dealsCount: 3));
            Assert.AreEqual(4, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 10,
                winRatePrcnt: 60, dealsCount: 12));
        }

        [TestMethod()]
        public void Penalties_HaveNoCliffAtDealsThreshold()
        {
            //Главное свойство мягкого штрафа: у порога нет обрыва, через который
            //выгодно перепрыгнуть, размыв фильтр входа ради лишних сделок.
            var fitness = CreateFitness(minDealsCount: 30);

            var justBelow = fitness.ApplyPenalties(10, 10, 60, dealsCount: 29);
            var atThreshold = fitness.ApplyPenalties(10, 10, 60, dealsCount: 30);

            Assert.AreEqual(9.67, justBelow);
            Assert.AreEqual(10, atThreshold);
            Assert.IsTrue(atThreshold - justBelow < 0.5,
                "У порога не должно быть скачка оценки.");
        }

        [TestMethod()]
        public void Penalties_DoNotRewardDealsAboveThreshold()
        {
            //Выше порога за сделки не доплачивают — иначе появился бы стимул
            //торговать ради самой торговли.
            var fitness = CreateFitness(minDealsCount: 30);

            Assert.AreEqual(fitness.ApplyPenalties(10, 10, 60, dealsCount: 30),
                fitness.ApplyPenalties(10, 10, 60, dealsCount: 3000));
        }

        [TestMethod()]
        public void Penalties_MultiplyWithEachOther()
        {
            var fitness = CreateFitness(maxDrawDownPrcnt: 20, minWinRatePrcnt: 50,
                minDealsCount: 30);

            //Просадка вдвое глубже, выигрышных вдвое меньше, сделок вдвое меньше —
            //оценка в восемь раз ниже.
            Assert.AreEqual(1.25, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 40,
                winRatePrcnt: 25, dealsCount: 15));
        }

        [TestMethod()]
        public void Penalties_AreHarsherWithHigherPower()
        {
            var soft = CreateFitness(maxDrawDownPrcnt: 20, penaltyPower: 1);
            var hard = CreateFitness(maxDrawDownPrcnt: 20, penaltyPower: 2);

            Assert.AreEqual(5, soft.ApplyPenalties(10, 40, 60, EnoughDeals));
            Assert.AreEqual(2.5, hard.ApplyPenalties(10, 40, 60, EnoughDeals));
        }

        [TestMethod()]
        public void Penalties_MakeLosingStrategiesWorseNotBetter()
        {
            //Умножение отрицательной оценки на долю меньше единицы приблизило бы её
            //к нулю, то есть штраф улучшал бы плохую стратегию.
            var fitness = CreateFitness(maxDrawDownPrcnt: 20);

            var penalized = fitness.ApplyPenalties(-10, maxDrawDownPrcnt: 40,
                winRatePrcnt: 60, dealsCount: EnoughDeals);

            Assert.AreEqual(-20, penalized);
        }

        [TestMethod()]
        public void Penalties_KeepRejectedChromosomesRejected()
        {
            var fitness = CreateFitness(maxDrawDownPrcnt: 20, minWinRatePrcnt: 50);

            Assert.IsTrue(double.IsNegativeInfinity(
                fitness.ApplyPenalties(double.NegativeInfinity, 40, 25, EnoughDeals)));
        }

        [TestMethod()]
        public void Penalties_SurviveZeroValues()
        {
            //Ни одной выигрышной сделки и ни одной сделки вообще: деления на ноль
            //быть не должно.
            var fitness = CreateFitness(minWinRatePrcnt: 50, minDealsCount: 30);

            Assert.AreEqual(0, fitness.ApplyPenalties(10, maxDrawDownPrcnt: 10,
                winRatePrcnt: 0, dealsCount: 0));
            Assert.IsFalse(double.IsNaN(fitness.ApplyPenalties(-10,
                maxDrawDownPrcnt: 10, winRatePrcnt: 0, dealsCount: 0)));
        }
    }
}
