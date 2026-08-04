using System;
using System.Collections.Generic;
using System.Linq;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Итог по всем форвардным отрезкам вместе. Отдельный отрезок оценивать
    /// бессмысленно: при одной-двух сделках в месяц большинство отрезков у
    /// трендовой системы убыточны по построению, а зарабатывает она редкими
    /// крупными сделками. Смысл имеет только агрегат по всем отрезкам сразу.
    ///
    /// Чего здесь нет: сквозной просадки по склеенной кривой капитала. Для неё
    /// нужна поштучная кривая, а здесь только итоги отрезков — худшая просадка
    /// отрезка её занижает. Прибыль тоже складывается без реинвестирования:
    /// каждый форвардный прогон стартует с одного и того же капитала.
    /// </summary>
    public class ForwardSummary
    {
        public int SegmentsCount { get; set; }
        public int PositiveSegments { get; set; }
        public int NegativeSegments { get; set; }

        /// <summary>Самая длинная череда убыточных отрезков подряд — сколько
        /// придётся терпеть, прежде чем система себя оправдает.</summary>
        public int MaxLosingStreak { get; set; }

        public int DealsCount { get; set; }
        public int WinningDealsCount { get; set; }
        public int LosingDealsCount { get; set; }
        public double WinRatePrcnt { get; set; }

        /// <summary>Профит-фактор по всем сделкам всех отрезков вместе.</summary>
        public double ProfitFactor { get; set; }

        public double TotalProfit { get; set; }
        public double TotalProfitPrcnt { get; set; }
        public double BestSegmentProfitPrcnt { get; set; }
        public double WorstSegmentProfitPrcnt { get; set; }

        /// <summary>Худшая просадка среди отрезков. Сквозную не заменяет.</summary>
        public double WorstSegmentDrawDownPrcnt { get; set; }

        public static ForwardSummary Calculate(IEnumerable<ForwardAnalysisResult> results)
        {
            var summary = new ForwardSummary();

            //Итог главного прогона форвардной части не имеет — его пропускаем.
            var segments = results == null
                ? new List<ForwardAnalysisResult>()
                : results.Where(r => r.ForwardDealsStatistics != null).ToList();

            if (segments.Count == 0)
                return summary;

            var sumWin = 0d;
            var sumLoss = 0d;
            var losingStreak = 0;

            foreach (var segment in segments)
            {
                var statistics = segment.ForwardDealsStatistics;

                summary.DealsCount += statistics.DealsCount;
                summary.WinningDealsCount += statistics.WinningDealsCount;
                summary.LosingDealsCount += statistics.LosingDealsCount;

                //Средние по отрезку разворачиваем обратно в суммы, иначе отрезки
                //с разным числом сделок вошли бы в профит-фактор с равным весом.
                sumWin += statistics.AverageWin * statistics.WinningDealsCount;
                sumLoss += statistics.AverageLoss * statistics.LosingDealsCount;

                summary.TotalProfit += segment.ForwardProfit;
                summary.TotalProfitPrcnt += segment.ForwardProfitPrcnt;

                if (segment.ForwardProfit > 0)
                {
                    summary.PositiveSegments++;
                    losingStreak = 0;
                }
                else if (segment.ForwardProfit < 0)
                {
                    summary.NegativeSegments++;
                    losingStreak++;
                    if (losingStreak > summary.MaxLosingStreak)
                        summary.MaxLosingStreak = losingStreak;
                }
                else
                    losingStreak = 0;

                if (segment.ForwardMaxDrawDown > summary.WorstSegmentDrawDownPrcnt)
                    summary.WorstSegmentDrawDownPrcnt = segment.ForwardMaxDrawDown;
            }

            summary.SegmentsCount = segments.Count;
            summary.BestSegmentProfitPrcnt = segments.Max(s => s.ForwardProfitPrcnt);
            summary.WorstSegmentProfitPrcnt = segments.Min(s => s.ForwardProfitPrcnt);

            summary.WinRatePrcnt = summary.DealsCount > 0
                ? Math.Round(summary.WinningDealsCount * 100d / summary.DealsCount, 2) : 0;

            summary.ProfitFactor = sumLoss > 0
                ? Math.Round(sumWin / sumLoss, 2)
                : (sumWin > 0 ? double.PositiveInfinity : 0);

            summary.TotalProfit = Math.Round(summary.TotalProfit, 2);
            summary.TotalProfitPrcnt = Math.Round(summary.TotalProfitPrcnt, 2);

            return summary;
        }

        /// <summary>Строки для журнала прогона.</summary>
        public List<string> ToLines()
        {
            return new List<string>()
            {
                string.Format("Итог по форвардным отрезкам ({0} шт.): в плюсе {1}, " +
                    "в минусе {2}, худшая череда убыточных подряд {3}.",
                    SegmentsCount, PositiveSegments, NegativeSegments, MaxLosingStreak),
                string.Format("Сделок вне выборки {0}: выигрышных {1}, убыточных {2} " +
                    "({3} %). Профит-фактор {4}.", DealsCount, WinningDealsCount,
                    LosingDealsCount, WinRatePrcnt, ProfitFactor),
                string.Format("Суммарная прибыль {0} р. ({1} %), без реинвестирования. " +
                    "Лучший отрезок {2} %, худший {3} %.", TotalProfit, TotalProfitPrcnt,
                    BestSegmentProfitPrcnt, WorstSegmentProfitPrcnt),
                string.Format("Худшая просадка отрезка {0} % — сквозную просадку " +
                    "склеенной кривой она занижает.", WorstSegmentDrawDownPrcnt),
            };
        }
    }
}
