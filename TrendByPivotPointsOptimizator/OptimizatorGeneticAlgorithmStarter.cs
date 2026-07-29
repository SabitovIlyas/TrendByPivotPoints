using Newtonsoft.Json;
using PeparatorDataForSpreadTradingSystems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TradingSystems;
using TrendByPivotPointsStarter;
using TSLab.DataSource;
using TSLab.Script;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class OptimizatorGeneticAlgorithmStarter
    {
        // Сид генератора случайных чисел. null — случайный запуск (боевой режим).
        // Задайте число, чтобы прогоны стали воспроизводимыми (отладка, регрессии).
        private int? seed = null;

        public void Start()
        {
            var logger = new ConsoleLogger();

            var startTime = DateTime.Now;
            logger.Log("Старт! {0}\r\n", startTime);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файл с настройками";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            var fullFileName = openFileDialog.FileName;

            var settings = CreateSettings(fullFileName);

            //Если в настройках указана стратегия — работаем через универсальный
            //оптимизатор; иначе — прежний путь Дончиана.
            var definition = CreateStrategyDefinition(settings.Strategy);
            if (definition != null)
            {
                StartUniversal(settings, definition, openFileDialog, logger, startTime);
                return;
            }

            openFileDialog.Title = "Выберите файл с инструментами";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;           

            fullFileName = openFileDialog.FileName;

            List<SecurityData> securitiesData = GetSecuritiesData(fullFileName);

            var loggerNull = new LoggerNull();

            List<Ticker> tickers = CreateTickers(securitiesData, fullFileName, settings, loggerNull);

            var converter = ConverterTextDataToBar.Create(fullFileName);
            var fileName = fullFileName.Split('\\').Last();
            var securityName = fileName.Split('.').First();
            var results = new List<ForwardAnalysisResult>();

            List<ChromosomeDonchianChannel> bestPopulation = null;
            List<ChromosomeDonchianChannel> bestPopulationLast = null;
            ChromosomeDonchianChannel bestChromosome = null;
            SurogateChromosome bestSurogateChromosome = null;

            openFileDialog.Title = "Выберите файл с инструментами";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fullFileName = openFileDialog.FileName;                
                bestSurogateChromosome = CreateBestChromosome(fullFileName);
            }

            var resultFileName = $"{tickers.First().Name}_{settings.Sides.First()}.csv";
            CreateTxtFile(resultFileName);
            try
            {
                var context = new ContextLab();
                var randomProvider = seed.HasValue
                    ? new RandomProvider(seed.Value)
                    : new RandomProvider();

                var optimizator = Optimizator.Create();
                var ga = new GeneticAlgorithmDonchianChannel(populationSize: 100, generations: 300,
                    crossoverRate: 0.85, mutationRate: 0.10, randomProvider, tickers, settings, context,
                    optimizator, loggerNull);

                logger.Log("Старт генетического алгоритма");
                logger.Log("Актуальная оптимизация!");
                ga.IsLastBackwardTesting = true;
                bestPopulationLast = ga.Run(period: 0, bestSurogateChromosome);

                foreach (var chromosome in bestPopulationLast)
                    chromosome.ForwardAnalysisResults.First().BackwardFitness =
                        chromosome.FitnessValue;

                var sumResults = 0d;
                foreach (var chromosome in bestPopulationLast)
                    sumResults += chromosome.ForwardAnalysisResults.First().BackwardFitness;

                var avgResults = sumResults / bestPopulationLast.Count;
                var tmpRes = new ForwardAnalysisResult() { BackwardFitness = avgResults, };

                if (bestPopulationLast.Count > 0)
                {
                    tmpRes.BackwardStart = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardStart;
                    tmpRes.BackwardEnd = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardEnd;
                    tmpRes.BackwardProfit = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardProfit;
                    tmpRes.BackwardProfitPrcnt = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardProfitPrcnt;
                }

                PrintToTxtFile(bestPopulationLast);
                bestChromosome = bestPopulationLast.First();

                ga.IsLastBackwardTesting = false;
                for (var period = 0; period < 10; period++)
                {
                    logger.Log("Период № {0}", period + 1);
                    bestPopulation = ga.Run(period, bestChromosome);

                    foreach (var chromosome in bestPopulation)
                        chromosome.ForwardAnalysisResults.First().BackwardFitness =
                            chromosome.FitnessValue;                    

                    foreach (var chromosome in bestPopulation)
                        chromosome.SetForwardBarsAsTickerBars();

                    foreach (var chromosome in bestPopulation)                    
                        chromosome.FitnessDonchianChannel.SetUpChromosomeFitnessValue(isCriteriaPassedNeedToCheck:
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

                    var tmp = new ForwardAnalysisResult()
                    {
                        BackwardFitness = avgResultsBackward,
                        ForwardFitness = avgResultsForward,                        
                    };

                    if (bestPopulation.Count > 0)
                    {
                        tmp.BackwardStart = bestPopulation.First().ForwardAnalysisResults.First().BackwardStart;
                        tmp.BackwardEnd = bestPopulation.First().ForwardAnalysisResults.First().BackwardEnd;
                        tmp.ForwardStart = bestPopulation.First().ForwardAnalysisResults.First().ForwardStart;
                        tmp.ForwardEnd = bestPopulation.First().ForwardAnalysisResults.First().ForwardEnd;
                        tmp.BackwardProfit = bestPopulation.First().ForwardAnalysisResults.First().BackwardProfit;
                        tmp.ForwardProfit = bestPopulation.First().ForwardAnalysisResults.First().ForwardProfit;
                        tmp.BackwardProfitPrcnt = bestPopulation.First().ForwardAnalysisResults.First().BackwardProfitPrcnt;
                        tmp.ForwardProfitPrcnt = bestPopulation.First().ForwardAnalysisResults.First().ForwardProfitPrcnt;
                    }

                    results.Add(tmp);
                    AppendToTxtFile(tmp, resultFileName);

                    var bestPopulationFile = $"{tickers.First().Name}_{settings.Sides.First()}_Period_{period}.csv";
                    CreateTxtFile(bestPopulationFile);
                    PrintToTxtFile(bestPopulation, bestPopulationFile);
                }
                results.Add(tmpRes);
                AppendToTxtFile(tmpRes, resultFileName);

                var stopTime = DateTime.Now;
                logger.Log("Стоп {0}", stopTime);

                var duration = stopTime-startTime;
                logger.Log("Время выполнения {0}", duration);
                logger.Log("Генетический алгоритм завершил работу.");
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }           

            Console.ReadLine();
        }

        /// <summary>Фабрика описаний стратегий по имени из файла настроек.</summary>
        public StrategyDefinition CreateStrategyDefinition(string strategyName)
        {
            if (string.IsNullOrEmpty(strategyName))
                return null;

            switch (strategyName.Trim().ToLowerInvariant())
            {
                case "meanreversion":
                    return new MeanReversionStrategyDefinition();
                case "donchian":
                case "donchianuniversal":
                    return new DonchianStrategyDefinition();
                default:
                    throw new Exception("Неизвестная стратегия в файле настроек: " +
                        strategyName);
            }
        }

        //Оптимизация через универсальный генетический алгоритм: стратегия и все
        //параметры задаются файлом настроек, хардкода нет. Здесь — только диалоги
        //выбора файлов, вся работа в StartUniversalCore.
        private void StartUniversal(Settings settings, StrategyDefinition definition,
            OpenFileDialog openFileDialog, Logger logger, DateTime startTime)
        {
            openFileDialog.Title = "Выберите файл с инструментами";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            var securitiesFileName = openFileDialog.FileName;

            Dictionary<string, double> seedGenes = null;
            openFileDialog.Title = "Выберите файл с лучшей хромосомой (необязательно)";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                seedGenes = LoadSeedGenes(openFileDialog.FileName);

            StartUniversalCore(settings, definition, securitiesFileName, seedGenes, logger,
                startTime);

            Console.ReadLine();
        }

        /// <summary>
        /// Ядро универсальной оптимизации без пользовательского интерфейса —
        /// пригодно для автоматизированных запусков.
        /// </summary>
        public void StartUniversalCore(Settings settings, StrategyDefinition definition,
            string securitiesFileName, Dictionary<string, double> seedGenes, Logger logger,
            DateTime startTime)
        {
            logger.Log("Стратегия: {0}", definition.Name);

            var securitiesData = GetSecuritiesData(securitiesFileName);
            var loggerNull = new LoggerNull();
            var tickers = CreateTickers(securitiesData, securitiesFileName, settings,
                loggerNull);

            var results = new List<ForwardAnalysisResult>();
            var resultFileName = $"{tickers.First().Name}_{settings.Sides.First()}_" +
                $"{definition.Name}.csv";
            CreateTxtFile(resultFileName);

            try
            {
                var context = new ContextLab();
                var effectiveSeed = settings.Seed ?? seed;
                var randomProvider = effectiveSeed.HasValue
                    ? new RandomProvider(effectiveSeed.Value)
                    : new RandomProvider();

                var ga = new GeneticAlgorithmUniversal(settings.PopulationSize,
                    settings.Generations, settings.CrossoverRate, settings.MutationRate,
                    randomProvider, tickers, settings, context, definition, loggerNull);

                logger.Log("Старт генетического алгоритма");
                logger.Log("Актуальная оптимизация!");
                ga.IsLastBackwardTesting = true;
                var bestPopulationLast = ga.Run(period: 0, seedGenes);

                foreach (var chromosome in bestPopulationLast)
                    chromosome.ForwardAnalysisResults.First().BackwardFitness =
                        chromosome.FitnessValue;

                var sumResults = 0d;
                foreach (var chromosome in bestPopulationLast)
                    sumResults += chromosome.ForwardAnalysisResults.First().BackwardFitness;

                var avgResults = sumResults / bestPopulationLast.Count;
                var tmpRes = new ForwardAnalysisResult() { BackwardFitness = avgResults, };

                if (bestPopulationLast.Count > 0)
                {
                    tmpRes.BackwardStart = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardStart;
                    tmpRes.BackwardEnd = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardEnd;
                    tmpRes.BackwardProfit = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardProfit;
                    tmpRes.BackwardProfitPrcnt = bestPopulationLast.First().ForwardAnalysisResults.First().BackwardProfitPrcnt;
                }

                PrintToTxtFile(bestPopulationLast, definition);
                var bestChromosome = bestPopulationLast.First();

                ga.IsLastBackwardTesting = false;
                for (var period = 0; period < settings.ForwardPeriodsCount; period++)
                {
                    logger.Log("Период № {0}", period + 1);
                    var bestPopulation = ga.Run(period, bestChromosome.Genes);

                    foreach (var chromosome in bestPopulation)
                        chromosome.ForwardAnalysisResults.First().BackwardFitness =
                            chromosome.FitnessValue;

                    foreach (var chromosome in bestPopulation)
                        chromosome.SetForwardBarsAsTickerBars();

                    foreach (var chromosome in bestPopulation)
                        chromosome.Fitness.SetUpChromosomeFitnessValue(
                            isCriteriaPassedNeedToCheck: false);

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

                    var tmp = new ForwardAnalysisResult()
                    {
                        BackwardFitness = avgResultsBackward,
                        ForwardFitness = avgResultsForward,
                    };

                    if (bestPopulation.Count > 0)
                    {
                        tmp.BackwardStart = bestPopulation.First().ForwardAnalysisResults.First().BackwardStart;
                        tmp.BackwardEnd = bestPopulation.First().ForwardAnalysisResults.First().BackwardEnd;
                        tmp.ForwardStart = bestPopulation.First().ForwardAnalysisResults.First().ForwardStart;
                        tmp.ForwardEnd = bestPopulation.First().ForwardAnalysisResults.First().ForwardEnd;
                        tmp.BackwardProfit = bestPopulation.First().ForwardAnalysisResults.First().BackwardProfit;
                        tmp.ForwardProfit = bestPopulation.First().ForwardAnalysisResults.First().ForwardProfit;
                        tmp.BackwardProfitPrcnt = bestPopulation.First().ForwardAnalysisResults.First().BackwardProfitPrcnt;
                        tmp.ForwardProfitPrcnt = bestPopulation.First().ForwardAnalysisResults.First().ForwardProfitPrcnt;
                    }

                    results.Add(tmp);
                    AppendToTxtFile(tmp, resultFileName);

                    var bestPopulationFile = $"{tickers.First().Name}_" +
                        $"{settings.Sides.First()}_{definition.Name}_Period_{period}.csv";
                    CreateTxtFile(bestPopulationFile);
                    PrintToTxtFile(bestPopulation, definition, bestPopulationFile);
                }
                results.Add(tmpRes);
                AppendToTxtFile(tmpRes, resultFileName);

                var stopTime = DateTime.Now;
                logger.Log("Стоп {0}", stopTime);

                var duration = stopTime - startTime;
                logger.Log("Время выполнения {0}", duration);
                logger.Log("Генетический алгоритм завершил работу.");
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
        }

        //Затравочные гены: JSON-словарь «имя параметра — значение».
        private Dictionary<string, double> LoadSeedGenes(string fullFileName)
        {
            var file = File.ReadAllText(fullFileName);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<Dictionary<string, double>>(
                new JsonTextReader(new StringReader(file)));
        }

        private void PrintToTxtFile(List<ChromosomeUniversal> population,
            StrategyDefinition definition, string fileName = "")
        {
            if (population == null || population.Count == 0)
                return;

            var t = population.Last();

            if (fileName == "")
                fileName = $"{t.Ticker.Name}_{t.Side}_{definition.Name}_params.csv";

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                //Заголовки: общие колонки + имена параметров стратегии
                var header = $"{nameof(t.FitnessValue)};{nameof(t.DealsCount)};" +
                    $"{nameof(t.TimeFrame)};{nameof(t.Side)};{nameof(t.Ticker.Name)}";
                foreach (var descriptor in definition.Parameters)
                    header += ";" + descriptor.Name;
                header += $";{nameof(t.Profit)};{nameof(t.ProfitPrcnt)}";
                writer.WriteLine(header);

                foreach (var c in population)
                {
                    var line = $"{c.FitnessValue};{c.DealsCount};{c.TimeFrame};{c.Side};" +
                        $"{c.Ticker.Name}";
                    foreach (var descriptor in definition.Parameters)
                        line += ";" + c.Genes[descriptor.Name];
                    line += $";{c.Profit};{c.ProfitPrcnt}";
                    writer.WriteLine(line);
                }
            }
        }

        private SurogateChromosome CreateBestChromosome(string fullFileName)
        {
            var file = File.ReadAllText(fullFileName);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<SurogateChromosome>(new JsonTextReader(new StringReader(file)));                        
        }

        private void PrintToTxtFile(List<ChromosomeDonchianChannel> population, string fileName ="")
        {
            if (population == null || population.Count == 0)
                return;

            var t = population.Last();

            if (fileName == "")
                fileName= $"{t.Ticker.Name}_{t.Side}_params.csv";

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                // Запись заголовков столбцов                
                writer.WriteLine($"{nameof(t.FitnessValue)};{nameof(t.DealsCount)};" +
                    $"{nameof(t.TimeFrame)};{nameof(t.Side)};{nameof(t.Ticker.Name)};" +
                    $"{nameof(t.SlowDonchian)};{nameof(t.FastDonchian)};{nameof(t.AtrPeriod)};" +
                    $"{nameof(t.LimitOpenedPositions)};{nameof(t.KAtrForOpenPosition)};" +
                    $"{nameof(t.KAtrForStopLoss)};{nameof(t.Profit)};{nameof(t.ProfitPrcnt)}");

                foreach (var c in population)
                {
                    // Запись строк с данными
                    writer.WriteLine($"{c.FitnessValue};{c.DealsCount};{c.TimeFrame};{c.Side};" +
                        $"{c.Ticker.Name};{c.SlowDonchian};{c.FastDonchian};{c.AtrPeriod};" +
                        $"{c.LimitOpenedPositions};{c.KAtrForOpenPosition};{c.KAtrForStopLoss};" +
                        $"{c.Profit};{c.ProfitPrcnt}");
                }
            }
        }

        private void CreateTxtFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine($"BackwardFitness;ForwardFitness;" +
                    $"BackwardProfit;ForwardProfit;" +
                    $"BackwardProfitPrcnt;ForwardProfitPrcnt;" +
                    $"BackwardTestDates;ForwardTestDates;");
            }
        }

        private void AppendToTxtFile(ForwardAnalysisResult result, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, append: true))
            {
                writer.WriteLine($"{result.BackwardFitness};{result.ForwardFitness};" +
                    $"{result.BackwardProfit};{result.ForwardProfit};" +
                    $"{result.BackwardProfitPrcnt};{result.ForwardProfitPrcnt};" +
                    $"{result.BackwardStart}-{result.BackwardEnd};" +
                    $"{result.ForwardStart} - {result.ForwardEnd}");
            }
        }

        public Settings CreateSettings(string fullFileName)
        {
            var settings = new Settings();
            var sides = new List<PositionSide>();
            var timeFrames = new List<Interval>();

            try
            {
                if (!System.IO.File.Exists(fullFileName))
                    throw new Exception("Файл не найден!");

                string[] listStrings = System.IO.File.ReadAllLines(fullFileName);

                if (listStrings == null)
                    throw new Exception("Файл пустой!");

                foreach (var str in listStrings)
                {
                    if (str.Contains("Long"))
                        sides.Add(PositionSide.Long);
                    if (str.Contains("Short"))
                        sides.Add(PositionSide.Short);
                    if (str.Contains("01min"))
                        timeFrames.Add(new Interval(1, DataIntervals.MINUTE));
                    if (str.Contains("05min"))
                        timeFrames.Add(new Interval(5, DataIntervals.MINUTE));
                    if (str.Contains("15min"))
                        timeFrames.Add(new Interval(15, DataIntervals.MINUTE));
                    if (str.Contains("30min"))
                        timeFrames.Add(new Interval(30, DataIntervals.MINUTE));
                    if (str.Contains("60min"))
                        timeFrames.Add(new Interval(60, DataIntervals.MINUTE));
                    if (str.Contains("1d"))
                        timeFrames.Add(new Interval(1, DataIntervals.DAYS));

                    ParseSettingsKeyValue(str, settings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            settings.Sides = sides;
            settings.TimeFrames = timeFrames;
            return settings;
        }

        //Разбор строк вида «Ключ:Значение»; неизвестные ключи игнорируются.
        private void ParseSettingsKeyValue(string line, Settings settings)
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
                return;

            var key = line.Substring(0, separatorIndex).Trim();
            var value = line.Substring(separatorIndex + 1).Trim();

            try
            {
                switch (key)
                {
                    case "Strategy": settings.Strategy = value; break;
                    case "Seed": settings.Seed = int.Parse(value); break;
                    case "PopulationSize": settings.PopulationSize = int.Parse(value); break;
                    case "Generations": settings.Generations = int.Parse(value); break;
                    case "CrossoverRate": settings.CrossoverRate = ParseDouble(value); break;
                    case "MutationRate": settings.MutationRate = ParseDouble(value); break;
                    case "Patience": settings.Patience = int.Parse(value); break;
                    case "BackwardDays": settings.BackwardDays = int.Parse(value); break;
                    case "ForwardDays": settings.ForwardDays = int.Parse(value); break;
                    case "ForwardPeriodsCount": settings.ForwardPeriodsCount = int.Parse(value); break;
                    case "ShiftWindowDays": settings.ShiftWindowDays = int.Parse(value); break;
                    case "Equity": settings.Equity = ParseDouble(value); break;
                    case "RiskValuePrcnt": settings.RiskValuePrcnt = ParseDouble(value); break;
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Не удалось разобрать строку настроек: " + line);
            }
        }

        private double ParseDouble(string value)
        {
            return double.Parse(value.Replace(',', '.'),
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private List<SecurityData> GetSecuritiesData(string fullFileName)
        {
            var result = new List<SecurityData>();
            try
            {
                if (!System.IO.File.Exists(fullFileName))
                    throw new Exception("Файл не найден!");

                string[] listStrings = System.IO.File.ReadAllLines(fullFileName);

                if (listStrings == null)
                    throw new Exception("Файл пустой!");
                foreach (var str in listStrings)
                {
                    var splStr = str.Split(';');
                    var name = splStr[0];
                    var currency = splStr[1];
                    var shares = double.Parse(splStr[2]);
                    var commissionRate = double.Parse(splStr[3]);
                    var isUSD = int.Parse(splStr[4]);
                    var rateUSD = double.Parse(splStr[5]);

                    result.Add(new SecurityData()
                    {
                        Name = name,
                        Currency = GetCurrency(currency),
                        Shares = shares,
                        CommissionRate = commissionRate,
                        IsUSD = isUSD == 1,
                        RateUSD = rateUSD,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        private Currency GetCurrency(string currency)
        {
            if (currency == Currency.USD.ToString())
                return Currency.USD;

            if (currency == Currency.RUB.ToString())
                return Currency.RUB;

            throw new Exception("Неверное значение валюты");
        }

        private List<Ticker> CreateTickers(List<SecurityData> securitiesData, string fullFileName,
            Settings settings, Logger logger)
        {
            var result = new List<Ticker>();

            foreach (var side in settings.Sides)
            {
                foreach (var timeFrame in settings.TimeFrames)
                {
                    foreach (var data in securitiesData)
                    {
                        var securityName = data.Name;
                        var fileNameSplitted = fullFileName.Split('\\');

                        var path = string.Empty;
                        for (var i = 0; i < fileNameSplitted.Length - 1; i++)
                            path += fileNameSplitted[i] + "\\";
                        var fileName = path + securityName + ".txt";

                        var ticker = CreateTicker(fileName, data, timeFrame, logger);

                        result.Add(ticker);
                    }
                }
            }

            return result;
        }

        private Ticker CreateTicker(string fileName, SecurityData data, Interval timeframe, Logger logger)
        {
            var converter = ConverterTextDataToBar.Create(fileName);
            var baseBars = converter.ConvertFileWithBarsToListOfBars();
            var bars = CompressBars(baseBars, timeframe);

            var ticker = new Ticker(data.Name, data.Currency, data.Shares, bars,
                logger, data.CommissionRate, data.IsUSD, data.RateUSD);

            return ticker;
        }

        public List<Bar> CompressBars(List<Bar> bars, Interval timeframe)
        {
            var compressor = new BarCompressor();
            var result = new List<Bar>();

            switch (timeframe.Base)
            {
                case DataIntervals.MINUTE:
                    {
                        switch (timeframe.Value)
                        {
                            case 1:
                                {
                                    result = bars;
                                    break;
                                }

                            case 5:
                                {
                                    result = compressor.To5Minute(bars);
                                    break;
                                }
                            case 15:
                                {
                                    result = compressor.To15Minute(bars);
                                    break;
                                }
                            case 30:
                                {
                                    result = compressor.To30Minute(bars);
                                    break;
                                }
                            case 60:
                                {
                                    result = compressor.ToHourly(bars);
                                    break;
                                }
                            default:
                                {
                                    throw new NotImplementedException();
                                }
                        }
                        break;
                    }
                case DataIntervals.DAYS:
                    {
                        switch (timeframe.Value)
                        {
                            case 1:
                                {
                                    result = compressor.ToDaily(bars);
                                    break;
                                }
                            default:
                                {
                                    throw new NotImplementedException();
                                }
                        }
                        break;
                    }
            }

            return result;
        }        
    }
}