using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class ForwardSummaryTests
    {
        private ForwardAnalysisResult CreateSegment(double profit, double profitPrcnt,
            int winners, int losers, double averageWin, double averageLoss,
            double drawDown = 10)
        {
            return new ForwardAnalysisResult()
            {
                ForwardProfit = profit,
                ForwardProfitPrcnt = profitPrcnt,
                ForwardMaxDrawDown = drawDown,
                ForwardDealsStatistics = new DealsStatistics()
                {
                    DealsCount = winners + losers,
                    WinningDealsCount = winners,
                    LosingDealsCount = losers,
                    AverageWin = averageWin,
                    AverageLoss = averageLoss,
                },
            };
        }

        [TestMethod()]
        public void Calculate_SumsDealsAcrossSegments()
        {
            var summary = ForwardSummary.Calculate(new List<ForwardAnalysisResult>()
            {
                CreateSegment(100, 10, winners: 2, losers: 3,
                    averageWin: 200, averageLoss: 100),
                CreateSegment(-50, -5, winners: 1, losers: 4,
                    averageWin: 150, averageLoss: 50),
            });

            Assert.AreEqual(2, summary.SegmentsCount);
            Assert.AreEqual(10, summary.DealsCount);
            Assert.AreEqual(3, summary.WinningDealsCount);
            Assert.AreEqual(7, summary.LosingDealsCount);
            Assert.AreEqual(30, summary.WinRatePrcnt);
        }

        [TestMethod()]
        public void Calculate_WeightsProfitFactorByDealsNotBySegments()
        {
            //Отрезок с двумя сделками не должен весить столько же, сколько отрезок
            //с двадцатью: средние разворачиваются обратно в суммы.
            var summary = ForwardSummary.Calculate(new List<ForwardAnalysisResult>()
            {
                //Выигрыши 2 × 200 = 400, проигрыши 3 × 100 = 300.
                CreateSegment(100, 10, winners: 2, losers: 3,
                    averageWin: 200, averageLoss: 100),
                //Выигрыши 1 × 150 = 150, проигрыши 4 × 50 = 200.
                CreateSegment(-50, -5, winners: 1, losers: 4,
                    averageWin: 150, averageLoss: 50),
            });

            //(400 + 150) / (300 + 200) = 1,1
            Assert.AreEqual(1.1, summary.ProfitFactor);
        }

        [TestMethod()]
        public void Calculate_CountsPositiveAndNegativeSegments()
        {
            var summary = ForwardSummary.Calculate(new List<ForwardAnalysisResult>()
            {
                CreateSegment(100, 10, 2, 3, 200, 100),
                CreateSegment(-50, -5, 1, 4, 150, 50),
                CreateSegment(-30, -3, 1, 5, 100, 50),
                CreateSegment(-20, -2, 1, 3, 100, 50),
                CreateSegment(400, 40, 3, 2, 200, 50),
            });

            Assert.AreEqual(2, summary.PositiveSegments);
            Assert.AreEqual(3, summary.NegativeSegments);

            //Три убыточных отрезка подряд в середине.
            Assert.AreEqual(3, summary.MaxLosingStreak);
        }

        [TestMethod()]
        public void Calculate_SumsProfitAndFindsExtremes()
        {
            var summary = ForwardSummary.Calculate(new List<ForwardAnalysisResult>()
            {
                CreateSegment(100, 10, 2, 3, 200, 100, drawDown: 12),
                CreateSegment(-50, -5, 1, 4, 150, 50, drawDown: 25),
                CreateSegment(400, 40, 3, 2, 200, 50, drawDown: 8),
            });

            Assert.AreEqual(450, summary.TotalProfit);
            Assert.AreEqual(45, summary.TotalProfitPrcnt);
            Assert.AreEqual(40, summary.BestSegmentProfitPrcnt);
            Assert.AreEqual(-5, summary.WorstSegmentProfitPrcnt);
            Assert.AreEqual(25, summary.WorstSegmentDrawDownPrcnt);
        }

        [TestMethod()]
        public void Calculate_SkipsMainBacktestRow()
        {
            //Итог главного прогона попадает в тот же список, но форвардной части
            //у него нет — в агрегат он входить не должен.
            var summary = ForwardSummary.Calculate(new List<ForwardAnalysisResult>()
            {
                CreateSegment(100, 10, 2, 3, 200, 100),
                new ForwardAnalysisResult() { BackwardProfit = 999999 },
            });

            Assert.AreEqual(1, summary.SegmentsCount);
            Assert.AreEqual(100, summary.TotalProfit);
        }

        [TestMethod()]
        public void CopyWindowMetrics_CarriesDealsStatisticsIntoReportRow()
        {
            //Из-за пропуска этого переноса итог по форвардным отрезкам выходил
            //нулевым: статистика оставалась на хромосоме и до отчёта не доезжала.
            var from = CreateSegment(100, 10, winners: 2, losers: 3,
                averageWin: 200, averageLoss: 100, drawDown: 18);
            from.BackwardMaxDrawDown = 12;
            from.BackwardRecoveryFactor = 5.5;
            from.ForwardRecoveryFactor = 2.1;
            from.BackwardDealsStatistics = new DealsStatistics() { DealsCount = 40 };

            var to = new ForwardAnalysisResult();
            OptimizatorGeneticAlgorithmStarter.CopyWindowMetrics(from, to);

            Assert.IsNotNull(to.ForwardDealsStatistics);
            Assert.AreEqual(5, to.ForwardDealsStatistics.DealsCount);
            Assert.AreEqual(40, to.BackwardDealsStatistics.DealsCount);
            Assert.AreEqual(18, to.ForwardMaxDrawDown);
            Assert.AreEqual(12, to.BackwardMaxDrawDown);
            Assert.AreEqual(2.1, to.ForwardRecoveryFactor);
            Assert.AreEqual(5.5, to.BackwardRecoveryFactor);
        }

        [TestMethod()]
        public void Calculate_WorksOnRowsBuiltByCopyWindowMetrics()
        {
            //Сквозная проверка: строка отчёта, собранная так же, как в прогоне,
            //должна попадать в агрегат, а не отсеиваться.
            var row = new ForwardAnalysisResult() { ForwardProfit = 100 };
            OptimizatorGeneticAlgorithmStarter.CopyWindowMetrics(
                CreateSegment(100, 10, 2, 3, 200, 100), row);

            var summary = ForwardSummary.Calculate(
                new List<ForwardAnalysisResult>() { row });

            Assert.AreEqual(1, summary.SegmentsCount);
            Assert.AreEqual(5, summary.DealsCount);
        }

        [TestMethod()]
        public void Calculate_HandlesEmptyInput()
        {
            var summary = ForwardSummary.Calculate(new List<ForwardAnalysisResult>());

            Assert.AreEqual(0, summary.SegmentsCount);
            Assert.AreEqual(0, summary.DealsCount);
            Assert.AreEqual(0, summary.ProfitFactor);
            Assert.AreEqual(0, summary.MaxLosingStreak);
        }

        [TestMethod()]
        public void Calculate_ReturnsInfinityWhenNoLosses()
        {
            var summary = ForwardSummary.Calculate(new List<ForwardAnalysisResult>()
            {
                CreateSegment(100, 10, winners: 2, losers: 0,
                    averageWin: 200, averageLoss: 0),
            });

            Assert.IsTrue(double.IsPositiveInfinity(summary.ProfitFactor));
        }
    }
}
