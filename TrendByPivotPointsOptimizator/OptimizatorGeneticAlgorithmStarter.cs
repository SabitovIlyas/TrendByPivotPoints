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

            var resultFileName = $"{tickers.First().Name}_{settings.Sides.First()}.csv";
            CreateTxtFile(resultFileName);
            try
            {
                var context = new ContextLab();
                var randomProvider = new RandomProvider();

                var optimizator = Optimizator.Create();
                var ga = new GeneticAlgorithmDonchianChannel(populationSize: 100, generations: 300,
                    crossoverRate: 0.85, mutationRate: 0.10, randomProvider, tickers, settings, context,
                    optimizator, loggerNull);

                logger.Log("Старт генетического алгоритма");
                logger.Log("Актуальная оптимизация!");
                ga.IsLastBackwardTesting = true;
                bestPopulationLast = ga.Run(period: 0);

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
                var bestChromosome = bestPopulationLast.First();

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

        private Settings CreateSettings(string fullFileName)
        {
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new Settings() { Sides = sides, TimeFrames = timeFrames };
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