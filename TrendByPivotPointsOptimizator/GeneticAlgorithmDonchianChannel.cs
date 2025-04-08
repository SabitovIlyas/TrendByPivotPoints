using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using TSLab.Utils;

namespace TrendByPivotPointsOptimizator
{
    public class GeneticAlgorithmDonchianChannel
    {
        public List<Chromosome> GetPopulation() => population;

        private List<Chromosome> population;
        private int populationSize;
        private int generations;
        private double crossoverRate;
        private double mutationRate;
        private readonly IRandomProvider randomProvider;

        public GeneticAlgorithmDonchianChannel(int populationSize, int generations, double crossoverRate, double mutationRate, 
            IRandomProvider randomProvider)
        {
            this.populationSize = populationSize;
            this.generations = generations;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;
            this.randomProvider = randomProvider;
            population = new List<Chromosome>();
        }

        public void Initialize(List<Ticker> tickers, Settings settings)
        {
            for (int i = 0; i < populationSize; i++)
            {
                var rand = randomProvider;
                
                var ticker = tickers[rand.Next(tickers.Count)];
                var tfs = settings.TimeFrames;
                var timeFrame = tfs[rand.Next(tfs.Count)];
                var sides = settings.Sides;
                var side = sides[rand.Next(sides.Count)];

                var fastDonchian = rand.Next(10, 208);              //9..208;
                var slowDonchian = rand.Next(fastDonchian, 208);    // -//-
                var atrPeriod = rand.Next(2, 25);                  //1..25
                var limitOpenedPositions = rand.Next(1, 5);
                var kAtrForOpenPosition = 0.5 * rand.Next(1, 5);
                var kAtrForStopLoss = 0.5 * rand.Next(1, 5);

                var systemParameters = new SystemParameters();

                systemParameters.Add("slowDonchian", slowDonchian);//1
                systemParameters.Add("fastDonchian", fastDonchian);//2
                systemParameters.Add("kAtrForStopLoss", kAtrForStopLoss);//3
                systemParameters.Add("kAtrForOpenPosition", kAtrForOpenPosition);//4
                systemParameters.Add("atrPeriod", atrPeriod);//3
                systemParameters.Add("limitOpenedPositions", limitOpenedPositions);//4
                systemParameters.Add("isUSD", 0);
                systemParameters.Add("rateUSD", 0d);
                systemParameters.Add("positionSide", pSide);//5
                systemParameters.Add("timeFrame", tF);//6
                systemParameters.Add("shares", ticker.Shares);

                systemParameters.Add("equity", 1000000d);
                systemParameters.Add("riskValuePrcnt", 2d);
                systemParameters.Add("contracts", 0);

                var security = new SecurityLab(ticker.Name, ticker.Currency, ticker.Shares, bars,
                                    ticker.Logger, ticker.CommissionRate);
                //logger.Log(counter.ToString());

                //listTradingSystemParameters.AddLast(new TradingSystemParameters()
                //{
                //    Security = security,
                //    SystemParameter = systemParameters
                //});


                //population.Add(new Chromosome(fast, slow));
            }
        }

        public void Evaluate(double[] prices)
        {
            foreach (var chrom in population)
            {
                chrom.Fitness = SimulateStrategy(prices, chrom.FastPeriod, chrom.SlowPeriod);
            }
        }

        public Chromosome TournamentSelection()
        {
            int tournamentSize = 3;
            Chromosome best = null;
            for (int i = 0; i < tournamentSize; i++)
            {
                Chromosome candidate = population[randomProvider.Next(populationSize)];
                if (best == null || candidate.Fitness > best.Fitness)
                {
                    best = candidate;
                }
            }
            return best;
        }

        public Chromosome Crossover(Chromosome parent1, Chromosome parent2)
        {
            int fast = randomProvider.Next(2) == 0 ? parent1.FastPeriod : parent2.FastPeriod;
            int slow = randomProvider.Next(2) == 0 ? parent1.SlowPeriod : parent2.SlowPeriod;
            if (slow <= fast)
            {
                int temp = fast;
                fast = slow;
                slow = temp;
            }
            return new Chromosome(fast, slow);
        }

        public void Mutate(Chromosome chrom)
        {
            if (randomProvider.NextDouble() < mutationRate)
            {
                chrom.FastPeriod = randomProvider.Next(1, chrom.SlowPeriod);
            }
            if (randomProvider.NextDouble() < mutationRate)
            {
                chrom.SlowPeriod = randomProvider.Next(chrom.FastPeriod + 1, 201);
            }
        }

        public Chromosome Run(double[] prices)
        {
            Initialize();
            for (int gen = 0; gen < generations; gen++)
            {
                Evaluate(prices);
                List<Chromosome> newPopulation = new List<Chromosome>();
                // Элитизм: сохраняем лучшую хромосому
                Chromosome best = population.OrderByDescending(c => c.Fitness).First();
                newPopulation.Add(best);
                // Создаем потомков
                while (newPopulation.Count < populationSize)
                {
                    Chromosome parent1 = TournamentSelection();
                    Chromosome parent2 = TournamentSelection();
                    Chromosome child;
                    if (randomProvider.NextDouble() < crossoverRate)
                    {
                        child = Crossover(parent1, parent2);
                    }
                    else
                    {
                        child = new Chromosome(parent1.FastPeriod, parent1.SlowPeriod);
                    }
                    Mutate(child);
                    newPopulation.Add(child);
                }
                population = newPopulation;
            }
            Evaluate(prices);
            return population.OrderByDescending(c => c.Fitness).First();
        }

        public static double[] CalculateSMA(double[] prices, int period)
        {
            double[] sma = new double[prices.Length];
            double sum = 0;
            for (int i = 0; i < period && i < prices.Length; i++)
            {
                sum += prices[i];
                sma[i] = sum / (i + 1);
            }
            for (int i = period; i < prices.Length; i++)
            {
                sum += prices[i] - prices[i - period];
                sma[i] = sum / period;
            }
            return sma;
        }

        public static double SimulateStrategy(double[] prices, int fastPeriod, int slowPeriod)
        {
            double[] fastSMA = CalculateSMA(prices, fastPeriod);
            double[] slowSMA = CalculateSMA(prices, slowPeriod);
            bool inPosition = false;
            double entryPrice = 0;
            double profit = 0;
            for (int i = 1; i < prices.Length; i++)
            {
                if (!inPosition && fastSMA[i] > slowSMA[i] && fastSMA[i - 1] <= slowSMA[i - 1])
                {
                    inPosition = true;
                    entryPrice = prices[i];
                }
                else if (inPosition && fastSMA[i] < slowSMA[i] && fastSMA[i - 1] >= slowSMA[i - 1])
                {
                    profit += prices[i] - entryPrice;
                    inPosition = false;
                }
            }
            if (inPosition)
            {
                profit += prices[prices.Length - 1] - entryPrice;
            }
            return profit;
        }
    }
}