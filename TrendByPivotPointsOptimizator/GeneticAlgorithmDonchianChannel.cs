using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using TSLab.Utils;

namespace TrendByPivotPointsOptimizator
{
    public class GeneticAlgorithmDonchianChannel
    {
        public List<ChromosomeDonchianChannel> GetPopulation() => population;

        private List<ChromosomeDonchianChannel> population;
        private int populationSize;
        private int generations;
        private double crossoverRate;
        private double mutationRate;
        private readonly IRandomProvider randomProvider;
        private List<Ticker> tickers;
        private Settings settings;        

        public GeneticAlgorithmDonchianChannel(int populationSize, int generations, double crossoverRate, double mutationRate, 
            IRandomProvider randomProvider, List<Ticker> tickers, Settings settings)
        {
            this.populationSize = populationSize;
            this.generations = generations;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;
            this.randomProvider = randomProvider;
            this.tickers = tickers;
            this.settings = settings;
            population = new List<ChromosomeDonchianChannel>();
        }

        public void Initialize()
        {
            for (int i = 0; i < populationSize; i++)
            {                
                var ticker = tickers[randomProvider.Next(tickers.Count)];
                var tfs = settings.TimeFrames;
                var timeFrame = tfs[randomProvider.Next(tfs.Count)];
                var sides = settings.Sides;
                var side = sides[randomProvider.Next(sides.Count)];

                var fastDonchian = randomProvider.Next(10, 208);              //9..208;
                var slowDonchian = randomProvider.Next(fastDonchian, 208);    // -//-
                var atrPeriod = randomProvider.Next(2, 25);                  //1..25
                var limitOpenedPositions = randomProvider.Next(1, 5);
                var kAtrForOpenPosition = 0.5 * randomProvider.Next(1, 5);
                var kAtrForStopLoss = 0.5 * randomProvider.Next(1, 5);

                population.Add(new ChromosomeDonchianChannel(ticker, timeFrame, side,
                    fastDonchian,slowDonchian, atrPeriod, limitOpenedPositions,
                    kAtrForOpenPosition, kAtrForStopLoss));
            }
        }

        public void Evaluate(double[] prices)
        {
            foreach (var chrom in population)
            {
                //chrom.Fitness = SimulateStrategy(prices, chrom.FastPeriod, chrom.SlowPeriod);
            }
        }

        public ChromosomeDonchianChannel TournamentSelection()
        {
            int tournamentSize = 3;
            ChromosomeDonchianChannel best = null;
            for (int i = 0; i < tournamentSize; i++)
            {
                var candidate = population[randomProvider.Next(populationSize)];
                if (best == null || (candidate.FitnessPassed && candidate.FitnessValue > best.FitnessValue))
                {
                    best = candidate;
                }
            }
            return best;
        }

        public ChromosomeDonchianChannel Crossover(ChromosomeDonchianChannel parent1, ChromosomeDonchianChannel parent2)
        {
            var ticker = randomProvider.Next(2) == 0 ? parent1.Ticker : parent2.Ticker;
            var timeFrame = randomProvider.Next(2) == 0 ? parent1.TimeFrame : parent2.TimeFrame;
            var side = randomProvider.Next(2) == 0 ? parent1.Side : parent2.Side;
            var fastDonchian = randomProvider.Next(2) == 0 ? parent1.FastDonchian : parent2.FastDonchian;
            var slowDonchian = randomProvider.Next(2) == 0 ? parent1.SlowDonchian : parent2.SlowDonchian;
            var atrPeriod = randomProvider.Next(2) == 0 ? parent1.AtrPeriod : parent2.AtrPeriod;
            var limitOpenedPositions = randomProvider.Next(2) == 0 ? parent1.LimitOpenedPositions : parent2.LimitOpenedPositions;
            var kAtrForOpenPosition = randomProvider.Next(2) == 0 ? parent1.KAtrForOpenPosition : parent2.KAtrForOpenPosition;
            var kAtrForStopLoss = randomProvider.Next(2) == 0 ? parent1.KAtrForStopLoss : parent2.KAtrForStopLoss;

            if (slowDonchian < fastDonchian)
            {
                int temp = fastDonchian;
                fastDonchian = slowDonchian;
                slowDonchian = temp;
            }
            return new ChromosomeDonchianChannel(ticker, timeFrame, side,
                fastDonchian, slowDonchian, atrPeriod, limitOpenedPositions,
                kAtrForOpenPosition, kAtrForStopLoss);
        }

        public void Mutate(ChromosomeDonchianChannel chrom)
        {
            if (randomProvider.NextDouble() < mutationRate)
                chrom.Ticker = tickers[randomProvider.Next(tickers.Count)];

            if (randomProvider.NextDouble() < mutationRate)
            {
                var tfs = settings.TimeFrames;
                chrom.TimeFrame = tfs[randomProvider.Next(tfs.Count)];
            }

            if (randomProvider.NextDouble() < mutationRate)
            {
                var sides = settings.Sides;
                chrom.Side = sides[randomProvider.Next(sides.Count)];
            }

            if (randomProvider.NextDouble() < mutationRate)            
                chrom.FastDonchian = randomProvider.Next(10, 208);                  //9..208;
                
            if (randomProvider.NextDouble() < mutationRate)
                chrom.SlowDonchian = randomProvider.Next(chrom.FastDonchian, 208);  // -//-

            if (randomProvider.NextDouble() < mutationRate)
                chrom.AtrPeriod = randomProvider.Next(2, 25);                      //1..25

            if (randomProvider.NextDouble() < mutationRate)
                chrom.LimitOpenedPositions = randomProvider.Next(1, 5);

            if (randomProvider.NextDouble() < mutationRate)
                chrom.KAtrForOpenPosition = 0.5 * randomProvider.Next(1, 5);

            if (randomProvider.NextDouble() < mutationRate)  
                chrom.KAtrForStopLoss = 0.5 * randomProvider.Next(1, 5);            
        }

        public List<ChromosomeDonchianChannel> Run(double[] prices)
        {
            Initialize();
            for (int gen = 0; gen < generations; gen++)
            {
                Evaluate(prices);
                List<ChromosomeDonchianChannel> newPopulation = new List<ChromosomeDonchianChannel>();
                // Элитизм: сохраняем лучшую хромосому
                ChromosomeDonchianChannel best = population.OrderByDescending(c => c.FitnessValue).First();
                newPopulation.Add(best);
                // Создаем потомков
                while (newPopulation.Count < populationSize)
                {
                    ChromosomeDonchianChannel parent1 = TournamentSelection();
                    ChromosomeDonchianChannel parent2 = TournamentSelection();
                    ChromosomeDonchianChannel child;
                    if (randomProvider.NextDouble() < crossoverRate)
                    {
                        child = Crossover(parent1, parent2);
                    }
                    else
                    {
                        child = new ChromosomeDonchianChannel(parent1.Ticker, parent1.TimeFrame, parent1.Side,
                            parent1.FastDonchian, parent1.SlowDonchian, parent1.AtrPeriod,
                            parent1.LimitOpenedPositions, parent1.KAtrForOpenPosition, parent1.KAtrForStopLoss);
                    }
                    Mutate(child);
                    newPopulation.Add(child);
                }
                population = newPopulation;
            }
            Evaluate(prices);
            var populationPasssed = population.Where(population => population.FitnessPassed == true);
            return populationPasssed.OrderByDescending(c => c.FitnessValue).Take(30).ToList();
        }        
    }
}