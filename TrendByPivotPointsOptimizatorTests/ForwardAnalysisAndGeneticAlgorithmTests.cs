using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using TradingSystems;
using TrendByPivotPointsOptimizator;

namespace TrendByPivotPointsOptimizatorTests
{
    [TestClass()]
    public class ForwardAnalysisAndGeneticAlgorithmTests
    {
        //[TestMethod]
        public void foo()
        {
            var context = new ContextLab();
            var randomProvider = new RandomProvider();
            var logger = new ConsoleLogger();
            
            var tickers = new List<Ticker>();
            var bars = new List<Bar>();
            var ticker = new Ticker(name: "Brent", Currency.USD, shares: 10, bars, logger,
                commissionRate: 0);
            tickers.Add(ticker);
            var settings = new Settings();

            var optimizator = Optimizator.Create();
            var ga = new GeneticAlgorithmDonchianChannel(populationSize: 50, generations: 100, 
                crossoverRate: 0.8, mutationRate: 0.1, randomProvider, tickers, settings, context,
                optimizator, logger);

            var results = new List<ForwardAnalysisResult>();

            for (var period = 0; period < 10; period++)
            {                
                var bestPopulation = ga.Run(period);

                foreach (var chromosome in bestPopulation)
                    chromosome.ForwardAnalysisResults.First().BackwardFitness =
                        chromosome.FitnessValue;

                foreach (var chromosome in bestPopulation)
                    chromosome.SetForwardBarsAsTickerBars();
                
                ga.FitnessDonchianChannel.SetUpChromosomeFitnessValue(isCriteriaPassedNeedToCheck:
                    false);

                foreach (var chromosome in bestPopulation)
                    chromosome.ForwardAnalysisResults.First().ForwardFitness =
                            chromosome.FitnessValue;
                                
                var sumResultsBackward = 0d;
                var sumResultsForward = 0d;
                foreach (var chromosome in bestPopulation)
                {
                    sumResultsBackward += chromosome.ForwardAnalysisResults.First().BackwardFitness;
                    sumResultsForward += chromosome.ForwardAnalysisResults.First().ForwardFitness;                   
                }

                var avgResultsBackward = sumResultsBackward / bestPopulation.Count;
                var avgResultsForward = sumResultsForward / bestPopulation.Count;

                results.Add(new ForwardAnalysisResult()
                {
                    BackwardFitness = avgResultsBackward,
                    ForwardFitness = avgResultsForward
                });
            }

            var isStrategyViable = ga.IsStrategyViable(results);
        }
    }
}