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

            ga.Initialize(); //создали популяцию хромосом

            var population = ga.GetPopulation();

            var fa = new ForwardAnalysis(genAlg: ga, forwardPeriodDays: 30,
                backwardPeriodDays: 120, forwardPeriodsCount: 10);

            fa.PerformAnalysis(population);

            foreach (var c in population)
            {
                c.SetBackwardBarsAsTickerBars();
                c.FitnessValue = 0;
            }
                
            

            population.First()


            
            

        }
    }
}