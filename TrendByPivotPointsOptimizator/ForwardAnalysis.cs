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
        private readonly GeneticAlgorithmDonchianChannel genAlg;
        private readonly Security security;
        private readonly int forwardPeriodDays;
        private readonly int backwardPeriodDays;
        private readonly int forwardPeriodsCount;

        public ForwardAnalysis(Security security, int forwardPeriodDays, int backwardPeriodDays, int forwardPeriodsCount)
        {
            this.security = security ?? throw new ArgumentNullException(nameof(security));
            this.forwardPeriodDays = forwardPeriodDays;
            this.backwardPeriodDays = backwardPeriodDays;
            this.forwardPeriodsCount = forwardPeriodsCount;

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (forwardPeriodDays <= 0)
                throw new ArgumentException("Forward period must be positive", nameof(forwardPeriodDays));
            if (backwardPeriodDays <= 0)
                throw new ArgumentException("Backward period must be positive", nameof(backwardPeriodDays));
            if (forwardPeriodsCount <= 0)
                throw new ArgumentException("Forward periods count must be positive", nameof(forwardPeriodsCount));

            if (!security.Bars.Any())
                throw new InvalidOperationException("Bars list is empty");

            var totalRequiredDays = backwardPeriodDays + (forwardPeriodDays * forwardPeriodsCount);
            var availableDays = (security.Bars.Max(b => b.Date) - security.Bars.Min(b => b.Date)).Days + 1;

            if (availableDays < totalRequiredDays)
                throw new InvalidOperationException(
                    $"Insufficient data: available {availableDays} days, required {totalRequiredDays} days");
        }

        public ForwardAnalysis(GeneticAlgorithmDonchianChannel genAlg, int forwardPeriodDays, int backwardPeriodDays, int forwardPeriodsCount)
        {
            this.genAlg = genAlg;
            this.forwardPeriodDays = forwardPeriodDays;
            this.backwardPeriodDays = backwardPeriodDays;
            this.forwardPeriodsCount = forwardPeriodsCount;

            Generate();
        }

        private void Generate()
        {
            //Остановился здесь. Эта строчка неверная.
            var result = genAlg.Run();
        }        

        //Работает с датами! То что надо!
        public List<ForwardAnalysisResult> PerformAnalysis(Func<List<Bar>, double> fitnessFunction)
        {
            var results = new List<ForwardAnalysisResult>();
            var sortedBars = security.Bars.OrderBy(b => b.Date).ToList();
            var latestDate = sortedBars.Last().Date;

            for (int i = 0; i < forwardPeriodsCount; i++)
            {
                var forwardEnd = latestDate.AddDays(-forwardPeriodDays * i);
                var forwardStart = forwardEnd.AddDays(-forwardPeriodDays + 1);
                var backwardEnd = forwardStart.AddDays(-1);
                var backwardStart = backwardEnd.AddDays(-backwardPeriodDays + 1);

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

        public void PerformAnalysis(List<ChromosomeDonchianChannel> population)
        {
            var results = new List<ForwardAnalysisResult>();
            var sortedBars = security.Bars.OrderBy(b => b.Date).ToList();
            var latestDate = sortedBars.Last().Date;

            for (int i = 0; i < forwardPeriodsCount; i++)
            {
                var forwardEnd = latestDate.AddDays(-forwardPeriodDays * i);
                var forwardStart = forwardEnd.AddDays(-forwardPeriodDays + 1);
                var backwardEnd = forwardStart.AddDays(-1);
                var backwardStart = backwardEnd.AddDays(-backwardPeriodDays + 1);

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

                foreach (var chromosome in population)
                {
                    var result = new ForwardAnalysisResult
                    {
                        BackwardStart = backwardStart,
                        BackwardEnd = backwardEnd,
                        ForwardStart = forwardStart,
                        ForwardEnd = forwardEnd,
                        BackwardBars = backwardBars,
                        ForwardBars = forwardBars
                    };
                    chromosome.ForwardAnalysisResults.Add(result);                   
                }                        
            }            
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
}