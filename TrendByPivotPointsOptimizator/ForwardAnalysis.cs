using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    public class ForwardAnalysis
    {
        private readonly Security _security;
        private readonly int _forwardPeriodDays;
        private readonly int _backwardPeriodDays;
        private readonly int _forwardPeriodsCount;

        public ForwardAnalysis(Security security, int forwardPeriodDays, int backwardPeriodDays, int forwardPeriodsCount)
        {
            _security = security ?? throw new ArgumentNullException(nameof(security));
            _forwardPeriodDays = forwardPeriodDays;
            _backwardPeriodDays = backwardPeriodDays;
            _forwardPeriodsCount = forwardPeriodsCount;

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (_forwardPeriodDays <= 0)
                throw new ArgumentException("Forward period must be positive", nameof(_forwardPeriodDays));
            if (_backwardPeriodDays <= 0)
                throw new ArgumentException("Backward period must be positive", nameof(_backwardPeriodDays));
            if (_forwardPeriodsCount <= 0)
                throw new ArgumentException("Forward periods count must be positive", nameof(_forwardPeriodsCount));

            if (!_security.Bars.Any())
                throw new InvalidOperationException("Bars list is empty");

            var totalRequiredDays = _backwardPeriodDays + (_forwardPeriodDays * _forwardPeriodsCount);
            var availableDays = (_security.Bars.Max(b => b.Date) - _security.Bars.Min(b => b.Date)).Days;

            if (availableDays < totalRequiredDays)
                throw new InvalidOperationException(
                    $"Insufficient data: available {availableDays} days, required {totalRequiredDays} days");
        }

        public List<ForwardAnalysisResult> PerformAnalysis(Func<List<Bar>, double> fitnessFunction)
        {
            var results = new List<ForwardAnalysisResult>();
            var sortedBars = _security.Bars.OrderBy(b => b.Date).ToList();
            var latestDate = sortedBars.Last().Date;

            for (int i = 0; i < _forwardPeriodsCount; i++)
            {
                var forwardEnd = latestDate.AddDays(-_forwardPeriodDays * i);
                var forwardStart = forwardEnd.AddDays(-_forwardPeriodDays + 1);
                var backwardEnd = forwardStart.AddDays(-1);
                var backwardStart = backwardEnd.AddDays(-_backwardPeriodDays + 1);

                if (backwardStart < sortedBars.First().Date)
                    throw new InvalidOperationException("Not enough historical data for backward testing");

                var backwardBars = sortedBars
                    .Where(b => b.Date >= backwardStart && b.Date <= backwardEnd)
                    .ToList();

                var forwardBars = sortedBars
                    .Where(b => b.Date >= forwardStart && b.Date <= forwardEnd)
                    .ToList();

                if (!backwardBars.Any() || !forwardBars.Any())
                    continue;

                var result = new ForwardAnalysisResult
                {
                    BackwardFitness = fitnessFunction(backwardBars),
                    ForwardFitness = fitnessFunction(forwardBars),
                    BackwardStart = backwardStart,
                    BackwardEnd = backwardEnd,
                    ForwardStart = forwardStart,
                    ForwardEnd = forwardEnd
                };

                results.Add(result);
            }

            return results;
        }

        public bool IsStrategyViable(List<ForwardAnalysisResult> results, double correlationThreshold = 0.7)
        {
            if (!results.Any())
                return false;

            // Простая проверка корреляции между бэктестом и форвардным тестированием
            var backwardFitness = results.Select(r => r.BackwardFitness).ToList();
            var forwardFitness = results.Select(r => r.ForwardFitness).ToList();

            // Рассчитываем корреляцию Пирсона
            double meanBackward = backwardFitness.Average();
            double meanForward = forwardFitness.Average();

            double sumProduct = 0;
            double sumSquareBackward = 0;
            double sumSquareForward = 0;

            for (int i = 0; i < backwardFitness.Count; i++)
            {
                var diffBackward = backwardFitness[i] - meanBackward;
                var diffForward = forwardFitness[i] - meanForward;

                sumProduct += diffBackward * diffForward;
                sumSquareBackward += diffBackward * diffBackward;
                sumSquareForward += diffForward * diffForward;
            }

            double correlation = sumProduct / Math.Sqrt(sumSquareBackward * sumSquareForward);

            // Стратегия считается жизнеспособной, если корреляция выше порога
            return correlation >= correlationThreshold;
        }
    }
    public class ForwardAnalysisResult
    {
        public double BackwardFitness { get; set; }
        public double ForwardFitness { get; set; }
        public DateTime BackwardStart { get; set; }
        public DateTime BackwardEnd { get; set; }
        public DateTime ForwardStart { get; set; }
        public DateTime ForwardEnd { get; set; }
    }
}
