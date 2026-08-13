using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class DealsStatisticsTests
    {
        /// <summary>Метасделка с заданным результатом и длительностью.</summary>
        private class DealStub : Position
        {
            private readonly double profit;

            public DealStub(double profit, int barOpen = 0, int barClose = 0)
            {
                this.profit = profit;
                BarNumberOpenPosition = barOpen;
                BarNumberClosePosition = barClose;
            }

            public int BarNumberOpenPosition { get; }
            public int BarNumberClosePosition { get; }
            public double EntryPrice => 0;
            public double ExitPrice => 0;
            public string SignalNameForOpenPosition => string.Empty;
            public string SignalNameForClosePosition => string.Empty;
            public PositionSide PositionSide => PositionSide.Long;
            public int Contracts => 1;

            public double GetProfit() => profit;
            public double GetProfit(int barNumber) => profit;

            public void CloseAtStop(int barNumber, double stopPrice, string signal) { }
            public void CloseAtMarket(int barNumber, double price, string signal) { }
            public void CloseAtMarket(int barNumber, string signal) { }
        }

        private List<Position> Deals(params double[] profits)
        {
            var deals = new List<Position>();
            foreach (var profit in profits)
                deals.Add(new DealStub(profit));
            return deals;
        }

        [TestMethod()]
        public void Calculate_CountsWinsAndLosses()
        {
            var statistics = DealsStatistics.Calculate(Deals(100, -50, 200, -30, -20));

            Assert.AreEqual(5, statistics.DealsCount);
            Assert.AreEqual(2, statistics.WinningDealsCount);
            Assert.AreEqual(3, statistics.LosingDealsCount);
            Assert.AreEqual(40, statistics.WinRatePrcnt);
        }

        [TestMethod()]
        public void Calculate_AveragesWinAndLoss()
        {
            var statistics = DealsStatistics.Calculate(Deals(100, -50, 200, -30, -20));

            Assert.AreEqual(150, statistics.AverageWin);          //(100 + 200) / 2
            Assert.AreEqual(33.33, statistics.AverageLoss);        //(50 + 30 + 20) / 3
            Assert.AreEqual(4.5, statistics.PayoffRatio);          //150 / 33.33
            Assert.AreEqual(3, statistics.ProfitFactor);           //300 / 100
            Assert.AreEqual(40, statistics.ExpectedPayoff);        //(300 - 100) / 5
            Assert.AreEqual(200, statistics.LargestWin);
            Assert.AreEqual(50, statistics.LargestLoss);
        }

        [TestMethod()]
        public void Calculate_FindsLongestLosingStreak()
        {
            //Череда убытков: 3 подряд в середине.
            var statistics = DealsStatistics.Calculate(
                Deals(100, -10, -20, -30, 50, -40, -50, 10));

            Assert.AreEqual(3, statistics.MaxConsecutiveLosses);
            Assert.AreEqual(1, statistics.MaxConsecutiveWins);
        }

        [TestMethod()]
        public void Calculate_FindsLongestWinningStreak()
        {
            var statistics = DealsStatistics.Calculate(Deals(10, 20, 30, -5, 40, 50));

            Assert.AreEqual(3, statistics.MaxConsecutiveWins);
            Assert.AreEqual(1, statistics.MaxConsecutiveLosses);
        }

        [TestMethod()]
        public void Calculate_BreaksStreakOnZeroDeal()
        {
            //Сделка в ноль не выигрышная и не убыточная, но череду прерывает.
            var statistics = DealsStatistics.Calculate(Deals(-10, -20, 0, -30));

            Assert.AreEqual(2, statistics.MaxConsecutiveLosses);
            Assert.AreEqual(3, statistics.LosingDealsCount);
            Assert.AreEqual(0, statistics.WinningDealsCount);
            Assert.AreEqual(4, statistics.DealsCount);
        }

        [TestMethod()]
        public void Calculate_ReturnsInfinityWhenThereAreNoLosses()
        {
            var statistics = DealsStatistics.Calculate(Deals(10, 20, 30));

            Assert.AreEqual(100, statistics.WinRatePrcnt);
            Assert.IsTrue(double.IsPositiveInfinity(statistics.ProfitFactor),
                "Без убыточных сделок профит-фактор не определён.");
            Assert.IsTrue(double.IsPositiveInfinity(statistics.PayoffRatio));
        }

        [TestMethod()]
        public void Calculate_HandlesEmptyDeals()
        {
            var statistics = DealsStatistics.Calculate(new List<Position>());

            Assert.AreEqual(0, statistics.DealsCount);
            Assert.AreEqual(0, statistics.WinRatePrcnt);
            Assert.AreEqual(0, statistics.ProfitFactor);
            Assert.AreEqual(0, statistics.MaxConsecutiveLosses);
        }

        [TestMethod()]
        public void Calculate_AveragesDealDurationInBars()
        {
            var deals = new List<Position>()
            {
                new DealStub(100, barOpen: 10, barClose: 20),   //10 баров
                new DealStub(-50, barOpen: 30, barClose: 34),   //4 бара
            };

            var statistics = DealsStatistics.Calculate(deals);

            Assert.AreEqual(7, statistics.AverageBarsInDeal);
        }

        [TestMethod()]
        public void Calculate_SkipsDurationOfDealStillOpenAtWindowEnd()
        {
            //У незакрытой позиции номер бара закрытия остаётся int.MaxValue.
            //Раньше он попадал в среднее и давал 429 496 610 баров на сделку.
            var deals = new List<Position>()
            {
                new DealStub(100, barOpen: 10, barClose: 20),   //10 баров
                new DealStub(-50, barOpen: 30, barClose: 34),   //4 бара
                new DealStub(70, barOpen: 40, barClose: int.MaxValue),
            };

            var statistics = DealsStatistics.Calculate(deals);

            Assert.AreEqual(7, statistics.AverageBarsInDeal);
            //Сама сделка из остальных показателей не выпадает: её результат
            //переоценён по последнему закрытию окна и в статистике учтён.
            Assert.AreEqual(3, statistics.DealsCount);
            Assert.AreEqual(2, statistics.WinningDealsCount);
        }
    }
}
