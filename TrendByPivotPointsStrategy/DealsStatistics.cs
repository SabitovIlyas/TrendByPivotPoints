using System.Collections.Generic;

namespace TradingSystems
{
    /// <summary>
    /// Показатели эффективности, посчитанные по сделкам. Считаются по метасделкам:
    /// вход с последующими доливками и выход считаются одной сделкой, а не тремя.
    /// Показатели по счёту (просадка, фактор восстановления) живут в AccountLab —
    /// они считаются по кривой капитала, а не по сделкам.
    /// </summary>
    public class DealsStatistics
    {
        public int DealsCount { get; set; }
        public int WinningDealsCount { get; set; }
        public int LosingDealsCount { get; set; }

        /// <summary>Доля выигрышных сделок, %.</summary>
        public double WinRatePrcnt { get; set; }

        /// <summary>Средний выигрыш по прибыльным сделкам.</summary>
        public double AverageWin { get; set; }

        /// <summary>Средний проигрыш по убыточным сделкам — положительное число.</summary>
        public double AverageLoss { get; set; }

        /// <summary>Средний выигрыш к среднему проигрышу.</summary>
        public double PayoffRatio { get; set; }

        /// <summary>Сумма выигрышей к сумме проигрышей.</summary>
        public double ProfitFactor { get; set; }

        /// <summary>Средний результат сделки — математическое ожидание.</summary>
        public double ExpectedPayoff { get; set; }

        public double LargestWin { get; set; }

        /// <summary>Самый большой проигрыш — положительное число.</summary>
        public double LargestLoss { get; set; }

        /// <summary>Самая длинная череда убыточных сделок подряд.</summary>
        public int MaxConsecutiveLosses { get; set; }

        /// <summary>Самая длинная череда выигрышных сделок подряд.</summary>
        public int MaxConsecutiveWins { get; set; }

        /// <summary>Средняя длительность сделки в барах.</summary>
        public double AverageBarsInDeal { get; set; }

        /// <summary>
        /// Считает показатели по списку метасделок. Сделка с нулевым результатом
        /// не считается ни выигрышной, ни убыточной, но череду прерывает.
        /// </summary>
        public static DealsStatistics Calculate(List<Position> metaDeals)
        {
            var statistics = new DealsStatistics();
            if (metaDeals == null || metaDeals.Count == 0)
                return statistics;

            var sumWin = 0d;
            var sumLoss = 0d;
            var sumBars = 0d;
            var dealsWithBars = 0;
            var consecutiveLosses = 0;
            var consecutiveWins = 0;

            foreach (var deal in metaDeals)
            {
                var profit = deal.GetProfit();

                if (profit > 0)
                {
                    statistics.WinningDealsCount++;
                    sumWin += profit;
                    if (profit > statistics.LargestWin)
                        statistics.LargestWin = profit;

                    consecutiveWins++;
                    consecutiveLosses = 0;
                }
                else if (profit < 0)
                {
                    statistics.LosingDealsCount++;
                    sumLoss += -profit;
                    if (-profit > statistics.LargestLoss)
                        statistics.LargestLoss = -profit;

                    consecutiveLosses++;
                    consecutiveWins = 0;
                }
                else
                {
                    consecutiveWins = 0;
                    consecutiveLosses = 0;
                }

                if (consecutiveLosses > statistics.MaxConsecutiveLosses)
                    statistics.MaxConsecutiveLosses = consecutiveLosses;
                if (consecutiveWins > statistics.MaxConsecutiveWins)
                    statistics.MaxConsecutiveWins = consecutiveWins;

                //У незакрытой сделки номер бара закрытия остаётся int.MaxValue —
                //её длительность в среднее не берём. Проверять разность на знак
                //нельзя: int.MaxValue минус номер бара входа положителен, и такая
                //сделка утаскивала среднее к 429 496 610 баров.
                if (deal.BarNumberClosePosition != int.MaxValue)
                {
                    var bars = deal.BarNumberClosePosition - deal.BarNumberOpenPosition;
                    if (bars >= 0)
                    {
                        sumBars += bars;
                        dealsWithBars++;
                    }
                }
            }

            statistics.DealsCount = metaDeals.Count;
            statistics.WinRatePrcnt = Round(
                statistics.WinningDealsCount * 100d / statistics.DealsCount);
            statistics.ExpectedPayoff = Round((sumWin - sumLoss) / statistics.DealsCount);

            statistics.AverageWin = statistics.WinningDealsCount > 0
                ? Round(sumWin / statistics.WinningDealsCount) : 0;
            statistics.AverageLoss = statistics.LosingDealsCount > 0
                ? Round(sumLoss / statistics.LosingDealsCount) : 0;

            //Без убыточных сделок отношения не определены: показываем бесконечность,
            //чтобы не выдавать это за конечное число.
            statistics.PayoffRatio = statistics.AverageLoss > 0
                ? Round(statistics.AverageWin / statistics.AverageLoss)
                : (statistics.AverageWin > 0 ? double.PositiveInfinity : 0);
            statistics.ProfitFactor = sumLoss > 0
                ? Round(sumWin / sumLoss)
                : (sumWin > 0 ? double.PositiveInfinity : 0);

            statistics.AverageBarsInDeal = dealsWithBars > 0
                ? Round(sumBars / dealsWithBars) : 0;

            return statistics;
        }

        private static double Round(double value)
        {
            return System.Math.Round(value, 2);
        }
    }
}
