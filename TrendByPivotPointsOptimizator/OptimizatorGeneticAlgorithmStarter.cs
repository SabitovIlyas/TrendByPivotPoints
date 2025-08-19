using PeparatorDataForSpreadTradingSystems;
using System;
using System.Collections.Generic;
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
        //Logger logger;

        public void Start()
        {
            var logger = new ConsoleLogger();

            var startTime = DateTime.Now;
            logger.Log("Старт! {0}", startTime);

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

            try
            {
                var context = new ContextLab();
                var randomProvider = new RandomProvider();

                var optimizator = Optimizator.Create();
                var ga = new GeneticAlgorithmDonchianChannel(populationSize: 50, generations: 100,
                    crossoverRate: 0.8, mutationRate: 0.1, randomProvider, tickers, settings, context,
                    optimizator, loggerNull);

                var results = new List<ForwardAnalysisResult>();

                for (var period = 0; period < 10; period++)
                {
                    logger.Log("Period = {0}", period);
                    var bestPopulation = ga.Run(period);

                    foreach (var chromosome in bestPopulation)
                        chromosome.ForwardAnalysisResults.First().BackwardFitness =
                            chromosome.FitnessValue;

                    foreach (var chromosome in bestPopulation)
                        chromosome.SetForwardBarsAsTickerBars();

                    ga.FitnessDonchianChannel.SetUpChromosomeFitnessValue(isCriteriaPassedNeedToCheck:
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

                    results.Add(new ForwardAnalysisResult()
                    {
                        BackwardFitness = avgResultsBackward,
                        ForwardFitness = avgResultsForward
                    });
                }

                var isStrategyViable = ga.IsStrategyViable(results);

                var stopTime = DateTime.Now;
                logger.Log("Стоп {0}", stopTime);

                var duration = stopTime-startTime;
                logger.Log("Время выполнения {0}", duration);

                logger.Log("Генетический алгоритм завершил работу. Результаты: {0}",
                    isStrategyViable);

                ////foreach (var res in results)
                ////{
                    
                ////}
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }
            Console.ReadLine();
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
                    if (str.Contains("1min"))
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
                    var shares = int.Parse(splStr[2]);
                    var commissionRate = double.Parse(splStr[3]);

                    result.Add(new SecurityData()
                    {
                        Name = name,
                        Currency = GetCurrency(currency),
                        Shares = shares,
                        CommissionRate = commissionRate
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
            //видимо, ошибка с сжатием баров. Надо оттестировать.
            var bars = CompressBars(baseBars, timeframe);

            var ticker = new Ticker(data.Name, data.Currency, data.Shares, bars,
                logger, data.CommissionRate);

            return ticker;
        }

        private List<Bar> CompressBars(List<Bar> bars, Interval timeframe)
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