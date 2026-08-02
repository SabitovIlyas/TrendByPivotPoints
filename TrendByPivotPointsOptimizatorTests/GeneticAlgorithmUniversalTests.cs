using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class GeneticAlgorithmUniversalTests
    {
        //Синтетические часовые бары: детерминированное случайное блуждание.
        private List<Bar> CreateBars(int days)
        {
            var random = new Random(123);
            var bars = new List<Bar>();
            var price = 100000d;
            var date = new DateTime(2026, 1, 5);

            for (var d = 0; d < days; d++)
            {
                for (var h = 10; h <= 23; h++)
                {
                    var open = price;
                    var delta = random.Next(-150, 151);
                    var close = open + delta;
                    var high = Math.Max(open, close) + random.Next(0, 80);
                    var low = Math.Min(open, close) - random.Next(0, 80);
                    bars.Add(Bar.Create(date.AddDays(d).AddHours(h), open, high, low,
                        close, 1, "TEST", "60", 0));
                    price = close;
                }
            }
            return bars;
        }

        private Settings CreateTestSettings(PositionSide side = PositionSide.Long)
        {
            return new Settings
            {
                Sides = new List<PositionSide>() { side },
                TimeFrames = new List<Interval>() { new Interval(60, DataIntervals.MINUTE) },
                PopulationSize = 8,
                Generations = 2,
                BackwardDays = 40,
                ForwardDays = 10,
                ForwardPeriodsCount = 2,
                ShiftWindowDays = 5,
            };
        }

        //Журнал, который запоминает строки, чтобы проверить вывод прогона.
        private class LoggerMemory : Logger
        {
            public readonly List<string> Lines = new List<string>();

            public override void Log(string text)
            {
                Lines.Add(text);
            }

            public override void Log(string text, params object[] args)
            {
                Lines.Add(string.Format(text, args));
            }
        }

        private List<ChromosomeUniversal> RunGa(StrategyDefinition definition, int seed,
            PositionSide side = PositionSide.Long, Logger runLogger = null,
            bool isLastBackwardTesting = true, int threads = 1)
        {
            var bars = CreateBars(days: 90);
            var logger = new LoggerNull();
            var ticker = new Ticker("TEST", Currency.RUB, 1, bars, logger,
                commissionRate: 0, isUSD: false, rateUSD: 1);
            var settings = CreateTestSettings(side);
            settings.Threads = threads;
            var context = new ContextLab();
            var randomProvider = new RandomProvider(seed);

            var ga = new GeneticAlgorithmUniversal(settings.PopulationSize,
                settings.Generations, crossoverRate: 0.85, mutationRate: 0.10,
                randomProvider, new List<Ticker>() { ticker }, settings, context,
                definition, logger, runLogger ?? new LoggerNull());
            ga.IsLastBackwardTesting = isLastBackwardTesting;
            return ga.Run(period: 0);
        }

        [TestMethod()]
        public void SameSeed_ProducesIdenticalResult_MeanReversion()
        {
            var first = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 42);
            var second = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 42);

            Assert.IsTrue(first.Any(), "Оптимизатор не вернул ни одной хромосомы.");
            Assert.AreEqual(first.First().Name, second.First().Name);
            Assert.AreEqual(first.First().FitnessValue, second.First().FitnessValue);
            Assert.AreEqual(first.First().Profit, second.First().Profit);
        }

        [TestMethod()]
        public void DifferentSeeds_BothProduceResult_MeanReversion()
        {
            var first = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 42);
            var second = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 43);

            Assert.IsTrue(first.Any());
            Assert.IsTrue(second.Any());
        }

        [TestMethod()]
        public void DonchianDefinition_RunsThroughUniversalGa()
        {
            var best = RunGa(new DonchianStrategyDefinition(), seed: 42);

            Assert.IsTrue(best.Any(), "Оптимизатор не вернул ни одной хромосомы.");
        }

        [TestMethod()]
        public void ShortSide_UsesShortRanges()
        {
            //Для шорта пороги RSI ищутся в собственных диапазонах:
            //вход [50;95], выход [5;50] — независимо от лонга.
            var definition = new MeanReversionStrategyDefinition(PositionSide.Short);
            var best = RunGa(definition, seed: 42, side: PositionSide.Short);

            Assert.IsTrue(best.Any(), "Оптимизатор не вернул ни одной хромосомы.");
            var genes = best.First().Genes;
            Assert.IsTrue(genes["rsiEntryLevel"] >= 50 && genes["rsiEntryLevel"] <= 95,
                "rsiEntryLevel шорта вне диапазона [50; 95]: " + genes["rsiEntryLevel"]);
            Assert.IsTrue(genes["rsiExitLevel"] >= 5 && genes["rsiExitLevel"] <= 50,
                "rsiExitLevel шорта вне диапазона [5; 50]: " + genes["rsiExitLevel"]);
        }

        [TestMethod()]
        public void ParallelRun_GivesSameResultAsSingleThreaded()
        {
            //Хромосомы считаются независимо, генератор случайных чисел при расчёте
            //не трогается — значит от количества потоков результат зависеть не должен.
            var single = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long),
                seed: 42, threads: 1).First();
            var parallel = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long),
                seed: 42, threads: 4).First();

            Assert.AreEqual(single.Name, parallel.Name,
                "Параллельный прогон нашёл другую хромосому.");
            Assert.AreEqual(single.FitnessValue, parallel.FitnessValue);
            Assert.AreEqual(single.Profit, parallel.Profit);
            Assert.AreEqual(single.MaxDrawDown, parallel.MaxDrawDown);
            Assert.AreEqual(single.DealsCount, parallel.DealsCount);
        }

        [TestMethod()]
        public void ThreadsCount_FollowsSettingsAndWork()
        {
            var settings = CreateTestSettings();
            var ga = CreateGa(settings);

            //0 — по числу ядер, но не больше, чем хромосом в очереди.
            settings.Threads = 0;
            Assert.AreEqual(Math.Min(Environment.ProcessorCount, 50),
                ga.GetThreadsCount(chromosomesCount: 50));
            Assert.AreEqual(1, ga.GetThreadsCount(chromosomesCount: 1));

            settings.Threads = 3;
            Assert.AreEqual(3, ga.GetThreadsCount(chromosomesCount: 50));
            Assert.AreEqual(2, ga.GetThreadsCount(chromosomesCount: 2));
        }

        private GeneticAlgorithmUniversal CreateGa(Settings settings)
        {
            var logger = new LoggerNull();
            var ticker = new Ticker("TEST", Currency.RUB, 1, CreateBars(days: 90), logger,
                commissionRate: 0, isUSD: false, rateUSD: 1);

            return new GeneticAlgorithmUniversal(settings.PopulationSize,
                settings.Generations, crossoverRate: 0.85, mutationRate: 0.10,
                new RandomProvider(42), new List<Ticker>() { ticker }, settings,
                new ContextLab(), new MeanReversionStrategyDefinition(PositionSide.Long),
                logger, logger);
        }

        [TestMethod()]
        public void BestChromosome_IsReadyForForwardTesting()
        {
            //Повторяем то, что делает оптимизатор после Run в форвардном периоде:
            //гоняет лучшую хромосому на барах форвардного окна через её же
            //фитнес-функцию. Расчёт хромосом освобождает бары и стартер ради памяти —
            //для возвращённых хромосом всё это должно быть восстановлено.
            var best = RunGa(new MeanReversionStrategyDefinition(PositionSide.Long),
                seed: 42, isLastBackwardTesting: false).First();

            Assert.IsNotNull(best.Fitness, "У лучшей хромосомы нет фитнес-функции.");
            Assert.AreEqual(1, best.ForwardAnalysisResults.Count,
                "Окно тестирования должно быть ровно одно.");

            var result = best.ForwardAnalysisResults.First();
            Assert.IsNotNull(result.BackwardBars, "Потеряны бары окна бэктеста.");
            Assert.IsNotNull(result.ForwardBars, "Потеряны бары форвардного окна.");
            Assert.IsTrue(result.ForwardStart > result.BackwardEnd,
                "Форвардное окно должно идти после окна бэктеста.");

            best.Fitness.Bars = result.ForwardBars;
            Assert.AreSame(result.ForwardBars, best.Fitness.Bars,
                "Форвардный тест пошёл бы не по тем барам.");

            //Главное — прогон проходит целиком: до правки памяти здесь падало
            //исключение, потому что бары и стартер были отпущены безвозвратно.
            best.Fitness.SetUpChromosomeFitnessValue(isCriteriaPassedNeedToCheck: false);
        }

        [TestMethod()]
        public void Evaluate_DoesNotKeepBarsOfEveryChromosome()
        {
            //Кэш хранит только числа, а посчитанные хромосомы отпускают бары и
            //стартер: иначе прогон удерживает историю каждой особи.
            var bars = CreateBars(days: 90);
            var logger = new LoggerNull();
            var ticker = new Ticker("TEST", Currency.RUB, 1, bars, logger,
                commissionRate: 0, isUSD: false, rateUSD: 1);
            var settings = CreateTestSettings();
            var definition = new MeanReversionStrategyDefinition(PositionSide.Long);

            var ga = new GeneticAlgorithmUniversal(settings.PopulationSize,
                settings.Generations, crossoverRate: 0.85, mutationRate: 0.10,
                new RandomProvider(42), new List<Ticker>() { ticker }, settings,
                new ContextLab(), definition, logger, new LoggerNull());
            ga.IsLastBackwardTesting = true;

            ga.Initialize();
            ga.Evaluate(period: 0);

            var withBars = ga.GetPopulation().Count(c =>
                c.ForwardAnalysisResults.Any(r => r.BackwardBars != null) ||
                c.Fitness != null);

            Assert.AreEqual(0, withBars,
                "После расчёта хромосомы не должны держать бары и стартер.");

            //Даты окна — лёгкие, их оставляем: по ним пишется отчёт.
            Assert.IsTrue(ga.GetPopulation().All(c => c.ForwardAnalysisResults.Count == 1),
                "У каждой хромосомы должно остаться описание её окна тестирования.");
        }

        [TestMethod()]
        public void EliteCount_IsFractionOfPopulation()
        {
            Assert.AreEqual(20, GeneticAlgorithmUniversal.GetEliteCount(100, 0.2));
            Assert.AreEqual(10, GeneticAlgorithmUniversal.GetEliteCount(50, 0.2));
            Assert.AreEqual(5, GeneticAlgorithmUniversal.GetEliteCount(100, 0.05));

            //Хотя бы одна особь переносится всегда, даже при нулевой доле.
            Assert.AreEqual(1, GeneticAlgorithmUniversal.GetEliteCount(100, 0));
            Assert.AreEqual(1, GeneticAlgorithmUniversal.GetEliteCount(10, 0.01));

            //И не больше всей популяции.
            Assert.AreEqual(10, GeneticAlgorithmUniversal.GetEliteCount(10, 1.5));
        }

        [TestMethod()]
        public void Run_LogsDiversityForEveryGeneration()
        {
            var runLogger = new LoggerMemory();
            RunGa(new MeanReversionStrategyDefinition(PositionSide.Long), seed: 42,
                runLogger: runLogger);

            var initialization = runLogger.Lines.Count(l =>
                l.StartsWith("Разнообразие после инициализации:"));
            Assert.AreEqual(1, initialization,
                "Разнообразие стартовой популяции не попало в журнал прогона.");

            //Настройки теста: 2 поколения, значит две строки с разнообразием и порогом.
            var generations = runLogger.Lines.Count(l =>
                l.StartsWith("Поколение ") && l.Contains("Разнообразие =") &&
                l.Contains("порог остановки") && l.Contains("Поколений без улучшения:"));
            Assert.AreEqual(2, generations,
                "В журнале нет разнообразия по поколениям:\r\n" +
                string.Join("\r\n", runLogger.Lines));
        }

        [TestMethod()]
        public void GeneValues_StayWithinDescriptorBounds()
        {
            var definition = new MeanReversionStrategyDefinition(PositionSide.Long);
            var best = RunGa(definition, seed: 7);

            Assert.IsTrue(best.Any());
            foreach (var descriptor in definition.Parameters)
            {
                var value = best.First().Genes[descriptor.Name];
                Assert.IsTrue(value >= descriptor.Min && value <= descriptor.Max,
                    string.Format("Ген {0} = {1} вне диапазона [{2}; {3}].",
                        descriptor.Name, value, descriptor.Min, descriptor.Max));
            }
        }
    }
}
