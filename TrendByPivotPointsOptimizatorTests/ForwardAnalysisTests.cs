using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPointsOptimizator;
using System;
using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsOptimizatorTests
{
    [TestClass()]
    public class ForwardAnalysisTests
    {
        private Security CreateTestSecurity(DateTime startDate, int days)
        {
            var bars = new List<Bar>();
            for (int i = 0; i < days; i++)
            {
                bars.Add(new Bar
                {
                    Date = startDate.AddDays(i),
                    Close = 100 + i
                });
            }
            return new SecurityLab(Currency.RUB, shares: 1, bars, new LoggerNull(),
                commissionRate: 0);
        }

        private double SimpleFitnessFunction(List<Bar> bars)
        {
            // Если список баров пустой, возвращаем 0
            if (bars == null || bars.Count < 2)
                return 0;

            // Считаем доходности между соседними барами
            double sumReturns = 0;
            int count = 0;
            for (int i = 1; i < bars.Count; i++)
            {
                double prevClose = bars[i - 1].Close;
                double currClose = bars[i].Close;
                double ret = (currClose / prevClose) - 1;
                sumReturns += ret;
                count++;
            }

            // Возвращаем среднюю доходность в процентах
            return (sumReturns / count) * 100;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullSecurity_ThrowsArgumentNullException()
        {
            var analysis = new ForwardAnalysis(security: null, 30, 180, 10, 30);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_NegativeForwardPeriod_ThrowsArgumentException()
        {
            var security = CreateTestSecurity(DateTime.Now, 1000);
            var analysis = new ForwardAnalysis(security, -30, 180, 10, 30);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_NegativeBackwardPeriod_ThrowsArgumentException()
        {
            var security = CreateTestSecurity(DateTime.Now, 1000);
            var analysis = new ForwardAnalysis(security, 30, -180, 10, 30);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_NegativePeriodsCount_ThrowsArgumentException()
        {
            var security = CreateTestSecurity(DateTime.Now, 1000);
            var analysis = new ForwardAnalysis(security, 30, 180, -10, 30);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Constructor_EmptyBars_ThrowsInvalidOperationException()
        {
            var security = new SecurityLab(Currency.RUB, shares: 1, new List<Bar>(),
                new LoggerNull(), commissionRate: 0);
            var analysis = new ForwardAnalysis(security, 30, 180, 10, 30);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Constructor_InsufficientData_ThrowsInvalidOperationException()
        {
            var security = CreateTestSecurity(DateTime.Now, 100); // Меньше, чем нужно (180 + 30*10 = 480 дней)
            var analysis = new ForwardAnalysis(security, 30, 180, 10, 30);
        }

        [TestMethod]
        public void PerformAnalysis_ValidParameters_ReturnsCorrectNumberOfResults()
        {
            var security = CreateTestSecurity(DateTime.Now.AddDays(-1000), 1000);
            var analysis = new ForwardAnalysis(security, 30, 180, 5, 30);
            var results = analysis.PerformAnalysis(SimpleFitnessFunction);
            Assert.AreEqual(5, results.Count);
        }

        [TestMethod]
        public void PerformAnalysis_DateRanges_CorrectlyAligned()
        {
            var security = CreateTestSecurity(DateTime.Now.AddDays(-1000 + 1), 1000); //чтобы попал текущий день
            var analysis = new ForwardAnalysis(security, forwardPeriodDays: 30,
                backwardPeriodDays: 180, forwardPeriodsCount: 2, shiftWindowDays: 30);

            var results = analysis.PerformAnalysis(SimpleFitnessFunction);
            var firstResult = results[0];
            var secondResult = results[1];

            // Проверяем, что форвардный период начинается сразу после бэктеста
            Assert.AreEqual(firstResult.BackwardEnd.AddDays(1), firstResult.ForwardStart);
            // Проверяем длительность периодов
            Assert.AreEqual(30, (firstResult.ForwardEnd - firstResult.ForwardStart).Days + 1);
            Assert.AreEqual(180, (firstResult.BackwardEnd - firstResult.BackwardStart).Days + 1);
            // Проверяем смещение между периодами
            Assert.AreEqual(firstResult.ForwardStart.AddDays(-30), secondResult.ForwardStart);
        }

        [TestMethod]
        public void IsStrategyViable_EmptyResults_ReturnsFalse()
        {
            //Я здесь
            var security = CreateTestSecurity(DateTime.Now, 1000);
            var analysis = new ForwardAnalysis(security, forwardPeriodDays: 30,
                backwardPeriodDays: 180, forwardPeriodsCount: 10, shiftWindowDays: 30);

            var result = analysis.IsStrategyViable(new List<ForwardAnalysisResult>());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsStrategyViable_HighCorrelation_ReturnsTrue()
        {
            var security = CreateTestSecurity(DateTime.Now, 1000);
            var analysis = new ForwardAnalysis(security, forwardPeriodDays: 30,
                backwardPeriodDays: 180, forwardPeriodsCount: 3, shiftWindowDays: 30);

            var results = new List<ForwardAnalysisResult>
        {
            new ForwardAnalysisResult { BackwardFitness = 1.0, ForwardFitness = 1.1 },
            new ForwardAnalysisResult { BackwardFitness = 2.0, ForwardFitness = 2.1 },
            new ForwardAnalysisResult { BackwardFitness = 3.0, ForwardFitness = 3.1 }
        };

            var isViable = analysis.IsStrategyViable(results, 0.9);

            Assert.IsTrue(isViable);
        }

        [TestMethod]
        public void IsStrategyViable_LowCorrelation_ReturnsFalse()
        {
            var security = CreateTestSecurity(DateTime.Now, 1000);
            var analysis = new ForwardAnalysis(security, forwardPeriodDays: 30,
                backwardPeriodDays: 180, forwardPeriodsCount: 3, shiftWindowDays: 30);

            var results = new List<ForwardAnalysisResult>
        {
            new ForwardAnalysisResult { BackwardFitness = 1.0, ForwardFitness = 3.0 },
            new ForwardAnalysisResult { BackwardFitness = 2.0, ForwardFitness = 1.0 },
            new ForwardAnalysisResult { BackwardFitness = 3.0, ForwardFitness = 2.0 }
        };

            var isViable = analysis.IsStrategyViable(results, 0.9);

            Assert.IsFalse(isViable);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PerformAnalysis_InsufficientHistoricalData_ThrowsInvalidOperationException()
        {
            var security = CreateTestSecurity(DateTime.Now.AddDays(-200), 200);
            var analysis = new ForwardAnalysis(security, 30, 180, 10, 30);

            var results = analysis.PerformAnalysis(SimpleFitnessFunction);
        }
    }
}