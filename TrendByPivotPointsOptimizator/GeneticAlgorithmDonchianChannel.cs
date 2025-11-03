using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using TradingSystems;
using TrendByPivotPointsStarter;
using TSLab.DataSource;
using TSLab.Utils;
using Context = TradingSystems.Context;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class GeneticAlgorithmDonchianChannel
    {
        public List<ChromosomeDonchianChannel> GetPopulation() => population;
        public bool IsLastBackwardTesting = false;
        private List<ChromosomeDonchianChannel> population;
        private int populationSize;
        private int generations;
        private double crossoverRate;
        private double mutationRate;
        private readonly IRandomProvider randomProvider;
        private List<Ticker> tickers;
        private Settings settings;
        private Context context;
        private Optimizator optimizator;
        private Logger logger;
        private ForwardAnalysis forwardAnalysis;
        private double neighborhoodPercentage = 0.05;

        public GeneticAlgorithmDonchianChannel(int populationSize, int generations, double crossoverRate, double mutationRate,
            IRandomProvider randomProvider, List<Ticker> tickers, Settings settings, Context context, Optimizator optimizator, Logger logger)
        {
            this.populationSize = populationSize;
            this.generations = generations;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;
            this.randomProvider = randomProvider;
            this.tickers = tickers;
            this.settings = settings;
            this.context = context;
            this.logger = logger;
            this.optimizator = optimizator;            
        }

        public void Initialize()
        {
            population = new List<ChromosomeDonchianChannel>();
            for (int i = 0; i < populationSize; i++)
            {
                var isAdded = false;
                while (!isAdded)
                {
                    var ticker = tickers[randomProvider.Next(tickers.Count)];
                    var tfs = settings.TimeFrames;
                    var timeFrame = tfs[randomProvider.Next(tfs.Count)];
                    var sides = settings.Sides;
                    var side = sides[randomProvider.Next(sides.Count)];

                    //var fastDonchian = 26;
                    var fastDonchian = randomProvider.Next(10, 100);              //9..208;
                    //var slowDonchian = 70;
                    var slowDonchian = randomProvider.Next(fastDonchian, 100);    // -//-                   

                    //var atrPeriod = 11;
                    var atrPeriod = randomProvider.Next(2, 25);                  //1..25

                    //var limitOpenedPositions = 4;
                    //var kAtrForOpenPosition = 2;
                    //var kAtrForStopLoss = 0.5;

                    var limitOpenedPositions = randomProvider.Next(1, 5);
                    var kAtrForOpenPosition = 0.5 * randomProvider.Next(1, 5);
                    var kAtrForStopLoss = 0.5 * randomProvider.Next(1, 5);


                    var c = new ChromosomeDonchianChannel(ticker, timeFrame, side,
                        fastDonchian, slowDonchian, atrPeriod, limitOpenedPositions,
                        kAtrForOpenPosition, kAtrForStopLoss);

                    isAdded = AddNeighbour(c, population, neighborhoodPercentage);
                }                
            }
        }

        public void Evaluate(int period = 0)
        {
            var i = 0;
            foreach (var chromosome in population)
            {
                var trSysParams = CreateTradingSystemParameters(chromosome);
                var system = new StarterDonchianTradingSystemLab(context, new List<Security>() { trSysParams.Security }, logger);

                if(IsLastBackwardTesting)
                    PrepareChromosomeFinal(chromosome, period);
                else
                    PrepareChromosome(chromosome, period);
                chromosome.SetBackwardBarsAsTickerBars();            

                var FitnessDonchianChannel = new FitnessDonchianChannel(trSysParams, chromosome, system);

                Console.WriteLine("Расчёт фитнес-функции для {0} хромосомы из {1}.\r\n\r\nХромосома: {2}",
                    ++i, population.Count, chromosome.Name);
                FitnessDonchianChannel.SetUpChromosomeFitnessValue();
                Console.WriteLine("Функция удовлетворяет критерию? -{0}. Фитнес-функция = {1}. Количество сделок = " +
                    "{2}.\r\n",
                    chromosome.FitnessPassed, chromosome.FitnessValue, chromosome.DealsCount);
                //var tmp = trSysParams.Security.GetDeals();
                //var tmp1 = trSysParams.Security.GetMetaDeals();
            }
        }

        private TradingSystemParameters CreateTradingSystemParameters(ChromosomeDonchianChannel chrom)
        {
            var ticker = chrom.Ticker;
            var systemParameters = new SystemParameters();

            systemParameters.Add("slowDonchian", chrom.SlowDonchian);//1
            systemParameters.Add("fastDonchian", chrom.FastDonchian);//2
            systemParameters.Add("kAtrForStopLoss", chrom.KAtrForStopLoss);//3
            systemParameters.Add("kAtrForOpenPosition", chrom.KAtrForOpenPosition);//4
            systemParameters.Add("atrPeriod", chrom.AtrPeriod);//3
            systemParameters.Add("limitOpenedPositions", chrom.LimitOpenedPositions);//4
            
            if (ticker.IsUSD)
                systemParameters.Add("isUSD", 1);
            else
                systemParameters.Add("isUSD", 0);
            systemParameters.Add("rateUSD", ticker.RateUSD);
            
            systemParameters.Add("positionSide", chrom.Side);//5
            systemParameters.Add("timeFrame", chrom.TimeFrame);//6
            systemParameters.Add("shares", ticker.Shares);

            systemParameters.Add("equity", 100000d);
            systemParameters.Add("riskValuePrcnt", 2d);
            systemParameters.Add("contracts", 0);

            var security = new SecurityLab(ticker.Name, ticker.Currency, ticker.Shares, ticker.Bars,
                                ticker.Logger, ticker.CommissionRate);
            security.RateUSD = ticker.RateUSD;

            return new TradingSystemParameters()
            {
                Security = security,
                SystemParameters = systemParameters
            };
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
                chrom.FastDonchian = randomProvider.Next(10, 100);                  //9..208;

            if (randomProvider.NextDouble() < mutationRate)
                chrom.SlowDonchian = randomProvider.Next(chrom.FastDonchian, 100);  // -//-

            if (randomProvider.NextDouble() < mutationRate)
                chrom.AtrPeriod = randomProvider.Next(2, 25);                      //1..25

            //if (randomProvider.NextDouble() < mutationRate)
            //    chrom.FastDonchian = randomProvider.Next(10, 11);                  //9..208;

            //if (randomProvider.NextDouble() < mutationRate)
            //    chrom.SlowDonchian = randomProvider.Next(12, 13);  // -//-

            //if (randomProvider.NextDouble() < mutationRate)
            //    chrom.AtrPeriod = randomProvider.Next(2, 3);                      //1..25

            if (randomProvider.NextDouble() < mutationRate)
                chrom.LimitOpenedPositions = randomProvider.Next(1, 5);

            if (randomProvider.NextDouble() < mutationRate)
                chrom.KAtrForOpenPosition = 0.5 * randomProvider.Next(1, 5);

            if (randomProvider.NextDouble() < mutationRate)
                chrom.KAtrForStopLoss = 0.5 * randomProvider.Next(1, 5);

            //if (randomProvider.NextDouble() < mutationRate)
            //    chrom.LimitOpenedPositions = randomProvider.Next(3, 4);

            //if (randomProvider.NextDouble() < mutationRate)
            //    chrom.KAtrForOpenPosition = 0.5 * randomProvider.Next(1, 2);

            //if (randomProvider.NextDouble() < mutationRate)
            //    chrom.KAtrForStopLoss = 0.5 * randomProvider.Next(1, 2);

            chrom.UpdateName();
        }
            
        public List<ChromosomeDonchianChannel> SelectBestNonNeighborChromosomes
            (List<ChromosomeDonchianChannel> population, int count, 
            double neighborhoodPercentage)
        {
            // Сортируем популяцию по убыванию фитнес-функции
            var sortedPopulation = population.OrderByDescending(c => c.FitnessValue).ToList();

            // Список для хранения выбранных хромосом
            var selected = new List<ChromosomeDonchianChannel>();            

            foreach (var candidate in sortedPopulation)
            {
                if (selected.Count >= count)
                    break;

                AddNeighbour(candidate, selected, neighborhoodPercentage);
            }

            return selected;
        }

        private bool AddNeighbour(ChromosomeDonchianChannel chromosome, 
            List<ChromosomeDonchianChannel> population, double neighborhoodPercentage)
        {
            var isNeighBour = IsNeighBour(chromosome, population, neighborhoodPercentage);
            if (!isNeighBour)            
                population.Add(chromosome);
            
            return !isNeighBour;
        }

        private bool IsNeighBour(ChromosomeDonchianChannel chromosome,
            List<ChromosomeDonchianChannel> population, double neighborhoodPercentage)
        {            
            int fastMin = (int)(chromosome.FastDonchian * (1 - neighborhoodPercentage));
            int fastMax = (int)(chromosome.FastDonchian * (1 + neighborhoodPercentage));
            int slowMin = (int)(chromosome.SlowDonchian * (1 - neighborhoodPercentage));
            int slowMax = (int)(chromosome.SlowDonchian * (1 + neighborhoodPercentage));
            int atrMin = (int)(chromosome.AtrPeriod * (1 - neighborhoodPercentage));
            int atrMax = (int)(chromosome.AtrPeriod * (1 + neighborhoodPercentage));
            var side = chromosome.Side;
            var timeFrame = chromosome.TimeFrame;
            var ticker = chromosome.Ticker;
            var kAtrForOpenPosition = chromosome.KAtrForOpenPosition;
            var kAtrForStopLoss = chromosome.KAtrForStopLoss;
            var limitOpenedPositions = chromosome.LimitOpenedPositions;

            foreach (var p in population)
                if (p.FastDonchian >= fastMin &&
                    p.FastDonchian <= fastMax &&
                    p.SlowDonchian >= slowMin &&
                    p.SlowDonchian <= slowMax &&
                    p.AtrPeriod >= atrMin &&
                    p.AtrPeriod <= atrMax &&
                    p.Side == side &&
                    p.TimeFrame.Base == timeFrame.Base &&
                    p.TimeFrame.Value == timeFrame.Value &&
                    p.Ticker.Name == ticker.Name &&
                    p.KAtrForOpenPosition == kAtrForOpenPosition &&
                    p.KAtrForStopLoss == kAtrForStopLoss &&
                    p.LimitOpenedPositions == limitOpenedPositions)
                    return true;

            return false;
        }

        public List<ChromosomeDonchianChannel> Run(int period = 0)
        {
            Initialize();
            for (int gen = 0; gen < generations; gen++)
            {
                Console.WriteLine("Генерация № {0}\r\n", gen + 1);
                Evaluate();
                List<ChromosomeDonchianChannel> newPopulation = new List<ChromosomeDonchianChannel>();
                var qtyBestChromosomes = 1;//30                

                // Элитизм: сохраняем лучшие хромосомы, исключая соседних
                var best = SelectBestNonNeighborChromosomes(population, count: qtyBestChromosomes, 
                    neighborhoodPercentage);
                newPopulation.AddRange(best);
                         
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
                    AddNeighbour(child, newPopulation, neighborhoodPercentage);                    
                }
                population = newPopulation;
            }

            Evaluate();
            var populationPasssed = population.Where(population => population.FitnessPassed == true);
            return populationPasssed.OrderByDescending(c => c.FitnessValue).Take(10).ToList();
        }

        private void PrepareChromosome(ChromosomeDonchianChannel chromosome, int period)
        {
            forwardAnalysis = new ForwardAnalysis(genAlg: this, forwardPeriodDays: 30,
                backwardPeriodDays: 180, forwardPeriodsCount: 10);

            forwardAnalysis.Period = period;
            chromosome.ResetBarsToInitBars();
            forwardAnalysis.SetTradingPeriods(chromosome);         
        }

        private void PrepareChromosomeFinal(ChromosomeDonchianChannel chromosome, int period)
        {
            forwardAnalysis = new ForwardAnalysis(genAlg: this, forwardPeriodDays: 0,
                backwardPeriodDays: 180, forwardPeriodsCount: 1);

            forwardAnalysis.Period = period;
            chromosome.ResetBarsToInitBars();
            forwardAnalysis.SetTradingPeriodsFinal(chromosome);
        }

        public bool IsStrategyViable(List<ForwardAnalysisResult> results)
        {
            return forwardAnalysis.IsStrategyViable(results);
        }
    }
}