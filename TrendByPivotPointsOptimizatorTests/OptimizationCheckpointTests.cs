using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class OptimizationCheckpointTests
    {
        private Settings CreateSettings()
        {
            return new Settings
            {
                Strategy = "MeanReversion",
                Sides = new List<PositionSide>() { PositionSide.Long },
                TimeFrames = new List<Interval>() { new Interval(60, DataIntervals.MINUTE) },
                PopulationSize = 8,
                Generations = 3,
                BackwardDays = 40,
                ForwardDays = 10,
                ForwardPeriodsCount = 2,
                ShiftWindowDays = 5,
            };
        }

        //Синтетические часовые бары: детерминированное случайное блуждание.
        private List<Bar> CreateBars(int days)
        {
            var random = new Random(123);
            var bars = new List<Bar>();
            var price = 100000d;
            var date = new DateTime(2026, 1, 5);

            for (var d = 0; d < days; d++)
                for (var h = 10; h <= 23; h++)
                {
                    var open = price;
                    var close = open + random.Next(-150, 151);
                    var high = Math.Max(open, close) + random.Next(0, 80);
                    var low = Math.Min(open, close) - random.Next(0, 80);
                    bars.Add(Bar.Create(date.AddDays(d).AddHours(h), open, high, low,
                        close, 1, "TEST", "60", 0));
                    price = close;
                }
            return bars;
        }

        private Ticker CreateTicker()
        {
            return new Ticker("TEST", Currency.RUB, 1, CreateBars(days: 90),
                new LoggerNull(), commissionRate: 0, isUSD: false, rateUSD: 1);
        }

        [TestMethod()]
        public void SaveAndLoad_RestoresState()
        {
            var fileName = Path.GetTempFileName();
            var checkpoint = new OptimizationCheckpoint()
            {
                Fingerprint = "abc123",
                Seed = 42,
                RandomDraws = 12345,
                Stage = OptimizationCheckpoint.StageForward,
                Period = 3,
                Generation = 57,
                BestFitnessEver = 38.19,
                GenerationsWithoutImprovement = 12,
                BestGenes = new Dictionary<string, double>()
                {
                    { "maPeriod", 89 }, { "atrMultiplier", 0.5 },
                },
                FinalBackwardResult = new ForwardAnalysisResult()
                {
                    BackwardFitness = 4.27,
                    BackwardStart = new DateTime(2022, 5, 31, 23, 0, 0),
                    BackwardEnd = new DateTime(2026, 5, 29, 23, 0, 0),
                },
                CompletedResults = new List<ForwardAnalysisResult>()
                {
                    new ForwardAnalysisResult()
                    {
                        BackwardFitness = 38.19,
                        ForwardFitness = 3.35,
                        BackwardProfit = 256164,
                        ForwardProfitPrcnt = 94.39,
                        ForwardStart = new DateTime(2022, 5, 31, 23, 0, 0),
                        ForwardEnd = new DateTime(2026, 5, 29, 23, 0, 0),
                    },
                },
                Population = new List<CheckpointChromosome>()
                {
                    new CheckpointChromosome()
                    {
                        TickerName = "Si",
                        TimeFrameIndex = 0,
                        SideIndex = 0,
                        Genes = new Dictionary<string, double>() { { "maPeriod", 89 } },
                        Metrics = new ChromosomeMetrics()
                        {
                            FitnessValue = 3.35, Profit = 256164, ProfitPrcnt = 256.16,
                            RecoveryFactor = 3.35, MaxDrawDown = 12.5, DealsCount = 199,
                        },
                    },
                    //Непосчитанная хромосома: результата ещё нет.
                    new CheckpointChromosome()
                    {
                        TickerName = "Si",
                        Genes = new Dictionary<string, double>() { { "maPeriod", 150 } },
                        Metrics = null,
                    },
                },
            };

            try
            {
                checkpoint.Save(fileName);
                var loaded = OptimizationCheckpoint.Load(fileName);

                Assert.AreEqual("abc123", loaded.Fingerprint);
                Assert.AreEqual(42, loaded.Seed);
                Assert.AreEqual(12345, loaded.RandomDraws);
                Assert.AreEqual(OptimizationCheckpoint.StageForward, loaded.Stage);
                Assert.AreEqual(3, loaded.Period);
                Assert.AreEqual(57, loaded.Generation);
                Assert.AreEqual(38.19, loaded.BestFitnessEver);
                Assert.AreEqual(12, loaded.GenerationsWithoutImprovement);
                Assert.AreEqual(89, loaded.BestGenes["maPeriod"]);
                Assert.AreEqual(0.5, loaded.BestGenes["atrMultiplier"]);

                Assert.AreEqual(4.27, loaded.FinalBackwardResult.BackwardFitness);
                Assert.AreEqual(new DateTime(2022, 5, 31, 23, 0, 0),
                    loaded.FinalBackwardResult.BackwardStart);

                Assert.AreEqual(1, loaded.CompletedResults.Count);
                Assert.AreEqual(3.35, loaded.CompletedResults[0].ForwardFitness);
                Assert.AreEqual(256164, loaded.CompletedResults[0].BackwardProfit);
                Assert.AreEqual(94.39, loaded.CompletedResults[0].ForwardProfitPrcnt);

                Assert.AreEqual(2, loaded.Population.Count);
                Assert.AreEqual("Si", loaded.Population[0].TickerName);
                Assert.AreEqual(89, loaded.Population[0].Genes["maPeriod"]);
                Assert.AreEqual(199, loaded.Population[0].Metrics.DealsCount);
                Assert.AreEqual(256.16, loaded.Population[0].Metrics.ProfitPrcnt);
                Assert.IsNull(loaded.Population[1].Metrics,
                    "Непосчитанная хромосома не должна получить результат.");
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod()]
        public void Metrics_RoundTripWithDealsStatistics()
        {
            var metrics = new ChromosomeMetrics()
            {
                FitnessValue = 3.35,
                Profit = 256164,
                ProfitPrcnt = 256.16,
                RecoveryFactor = 5.77,
                MaxDrawDown = 44.32,
                DealsCount = 199,
                DealsStatistics = new DealsStatistics()
                {
                    DealsCount = 199,
                    WinningDealsCount = 84,
                    LosingDealsCount = 115,
                    WinRatePrcnt = 42.21,
                    AverageWin = 6120.5,
                    AverageLoss = 2240.75,
                    PayoffRatio = 2.73,
                    ProfitFactor = 1.99,
                    ExpectedPayoff = 1287.25,
                    LargestWin = 48120,
                    LargestLoss = 9870.5,
                    MaxConsecutiveLosses = 9,
                    MaxConsecutiveWins = 5,
                    AverageBarsInDeal = 37.4,
                },
            };

            var restored = ChromosomeMetrics.Parse(metrics.ToLine());

            Assert.AreEqual(3.35, restored.FitnessValue);
            Assert.AreEqual(199, restored.DealsCount);
            Assert.AreEqual(84, restored.DealsStatistics.WinningDealsCount);
            Assert.AreEqual(115, restored.DealsStatistics.LosingDealsCount);
            Assert.AreEqual(42.21, restored.DealsStatistics.WinRatePrcnt);
            Assert.AreEqual(6120.5, restored.DealsStatistics.AverageWin);
            Assert.AreEqual(2240.75, restored.DealsStatistics.AverageLoss);
            Assert.AreEqual(2.73, restored.DealsStatistics.PayoffRatio);
            Assert.AreEqual(1.99, restored.DealsStatistics.ProfitFactor);
            Assert.AreEqual(1287.25, restored.DealsStatistics.ExpectedPayoff);
            Assert.AreEqual(48120, restored.DealsStatistics.LargestWin);
            Assert.AreEqual(9870.5, restored.DealsStatistics.LargestLoss);
            Assert.AreEqual(9, restored.DealsStatistics.MaxConsecutiveLosses);
            Assert.AreEqual(5, restored.DealsStatistics.MaxConsecutiveWins);
            Assert.AreEqual(37.4, restored.DealsStatistics.AverageBarsInDeal);
        }

        [TestMethod()]
        public void Metrics_RoundTripInfinity()
        {
            //Прогон без убыточных сделок: отношения бесконечны — и такими же должны
            //прочитаться обратно, иначе продолженный прогон получит другие числа.
            var metrics = new ChromosomeMetrics()
            {
                DealsStatistics = new DealsStatistics()
                {
                    ProfitFactor = double.PositiveInfinity,
                    PayoffRatio = double.PositiveInfinity,
                },
            };

            var restored = ChromosomeMetrics.Parse(metrics.ToLine());

            Assert.IsTrue(double.IsPositiveInfinity(restored.DealsStatistics.ProfitFactor));
            Assert.IsTrue(double.IsPositiveInfinity(restored.DealsStatistics.PayoffRatio));
        }

        [TestMethod()]
        public void Metrics_ParseOldLineWithoutDealsStatistics()
        {
            //Чек-поинт, снятый до появления показателей: прерванный прогон должен
            //продолжиться после обновления оптимизатора, а не начаться заново.
            var restored = ChromosomeMetrics.Parse("3.35;256164;256.16;5.77;44.32;199");

            Assert.AreEqual(3.35, restored.FitnessValue);
            Assert.AreEqual(199, restored.DealsCount);
            Assert.IsNotNull(restored.DealsStatistics);
            Assert.AreEqual(199, restored.DealsStatistics.DealsCount);
            Assert.AreEqual(0, restored.DealsStatistics.WinningDealsCount);
        }

        [TestMethod()]
        public void Fingerprint_DependsOnSettingsAndRanges()
        {
            var settings = CreateSettings();
            var definition = new MeanReversionStrategyDefinition(PositionSide.Long);
            var original = OptimizationCheckpoint.CalculateFingerprint(settings, definition);

            Assert.AreEqual(original,
                OptimizationCheckpoint.CalculateFingerprint(CreateSettings(),
                    new MeanReversionStrategyDefinition(PositionSide.Long)),
                "Одни и те же настройки должны давать один отпечаток.");

            var otherWindow = CreateSettings();
            otherWindow.BackwardDays = 41;
            Assert.AreNotEqual(original,
                OptimizationCheckpoint.CalculateFingerprint(otherWindow, definition),
                "Смена окна бэктеста должна менять отпечаток.");

            var otherStrategy = new DonchianStrategyDefinition();
            Assert.AreNotEqual(original,
                OptimizationCheckpoint.CalculateFingerprint(settings, otherStrategy),
                "Смена стратегии должна менять отпечаток.");

            var narrowed = new MeanReversionStrategyDefinition(PositionSide.Long);
            settings.ParameterRanges["maPeriod"] = new ParameterRange()
            {
                Min = 100, Max = 150, Step = 10,
            };
            narrowed.ApplyRangeOverrides(settings);
            Assert.AreNotEqual(original,
                OptimizationCheckpoint.CalculateFingerprint(settings, narrowed),
                "Смена диапазона поиска должна менять отпечаток.");
        }

        [TestMethod()]
        public void Replay_ContinuesSameRandomSequence()
        {
            var straight = new RandomProvider(42);
            var firstDraws = new List<double>();
            for (var i = 0; i < 10; i++)
                firstDraws.Add(straight.NextDouble());

            var expected = new List<double>();
            for (var i = 0; i < 5; i++)
                expected.Add(straight.NextDouble());

            Assert.AreEqual(15, straight.DrawsCount);

            //Разные способы взять число сдвигают генератор одинаково — на один шаг.
            var mixed = new RandomProvider(42);
            for (var i = 0; i < 5; i++)
                mixed.Next(100);
            for (var i = 0; i < 5; i++)
                mixed.NextDouble();

            var restored = new RandomProvider(42);
            restored.Replay(10);
            Assert.AreEqual(10, restored.DrawsCount);

            for (var i = 0; i < 5; i++)
                Assert.AreEqual(expected[i], restored.NextDouble(),
                    "Продолженная последовательность разошлась с непрерывной.");
        }

        [TestMethod()]
        public void ResumedRun_MatchesUninterruptedRun()
        {
            //Прогон целиком.
            var straightRandom = new RandomProvider(42);
            var straight = CreateGa(straightRandom, out Settings settings,
                out List<Ticker> tickers);

            OptimizationCheckpoint snapshot = null;
            var drawsAtSnapshot = 0;
            straight.GenerationCompleted = progress =>
            {
                //Снимок делаем на первом же поколении — дальше прогон идёт своим ходом.
                if (snapshot != null)
                    return;

                snapshot = new OptimizationCheckpoint()
                {
                    Stage = OptimizationCheckpoint.StageFinalBackward,
                    Generation = progress.Generation,
                    BestFitnessEver = progress.BestFitnessEver,
                    GenerationsWithoutImprovement = progress.GenerationsWithoutImprovement,
                    Population = OptimizationCheckpoint.FromPopulation(
                        progress.Population, settings),
                };
                drawsAtSnapshot = straightRandom.DrawsCount;
            };

            var straightBest = straight.Run(period: 0).First();

            Assert.IsNotNull(snapshot, "Снимок поколения не был сделан.");

            //Прогон, продолженный со снимка: тот же сид, прокрученный до места снимка.
            var resumedRandom = new RandomProvider(42);
            resumedRandom.Replay(drawsAtSnapshot);
            var resumed = CreateGa(resumedRandom, out Settings resumedSettings,
                out List<Ticker> resumedTickers);

            var progressFromSnapshot = new GaProgress()
            {
                Generation = snapshot.Generation,
                BestFitnessEver = snapshot.BestFitnessEver,
                GenerationsWithoutImprovement = snapshot.GenerationsWithoutImprovement,
                Population = snapshot.ToPopulation(resumedSettings, resumedTickers),
            };

            var resumedBest = resumed.Run(period: 0, seedGenes: null,
                resumeFrom: progressFromSnapshot).First();

            Assert.AreEqual(straightBest.Name, resumedBest.Name,
                "Продолженный прогон нашёл другую хромосому.");
            Assert.AreEqual(straightBest.FitnessValue, resumedBest.FitnessValue);
            Assert.AreEqual(straightBest.Profit, resumedBest.Profit);
            Assert.AreEqual(straightBest.DealsCount, resumedBest.DealsCount);
        }

        private GeneticAlgorithmUniversal CreateGa(RandomProvider randomProvider,
            out Settings settings, out List<Ticker> tickers)
        {
            settings = CreateSettings();
            tickers = new List<Ticker>() { CreateTicker() };
            var logger = new LoggerNull();

            var ga = new GeneticAlgorithmUniversal(settings.PopulationSize,
                settings.Generations, crossoverRate: 0.85, mutationRate: 0.10,
                randomProvider, tickers, settings, new ContextLab(),
                new MeanReversionStrategyDefinition(PositionSide.Long), logger, logger);
            ga.IsLastBackwardTesting = true;
            return ga;
        }
    }
}
