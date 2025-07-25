using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
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


            var optimizator = Optimizator.Create();
            //var ga = new GeneticAlgorithmDonchianChannel(populationSize: 50, generations: 100, 
            //    crossoverRate: 0.8, mutationRate: 0.1, randomProvider, tickers, settings, context,
            //    optimizator, logger);


        }
    }
}