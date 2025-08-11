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
        [TestMethod]
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
                       

            var bestPopulation = ga.Run();

            foreach (var c in bestPopulation)
            {
                c.ForwardAnalysisResults.First().BackwardFitness = c.FitnessValue;
                c.SetForwardBarsAsTickerBars();
            }

            ga.FitnessDonchianChannel.SetUpChromosomeFitnessValue();
            ga.FitnessDonchianChannel.IsCriteriaPassedNeedToCheck = false;

            foreach (var c in bestPopulation)
                c.ForwardAnalysisResults.First().ForwardFitness = c.FitnessValue;            

            //Теперь надо повторить это для прочих 9 периодов
        }
    }
}