using PeparatorDataForSpreadTradingSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator
{
    public class OptimizatorStarter
    {
        Logger logger;

        public void Start()
        {
            Console.WriteLine("Старт!");

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
            List<Ticker> tickers = CreateTickers(securitiesData, fullFileName, settings);

            var converter = ConverterTextDataToBar.Create(fullFileName);
            var fileName = fullFileName.Split('\\').Last();
            var securityName = fileName.Split('.').First();
            var bars = converter.ConvertFileWithBarsToListOfBars();

            logger = new ConsoleLogger();

            try
            {
                var context = new ContextLab();

                foreach (var ticker in tickers)//возможно, что я делаю лишний перебор с pSide и tFrame
                {
                    var listSystemParameters = CreateSystemParameters(settings, tickers);
                    foreach (var sp in listSystemParameters)
                    {
                        //var system = new StarterDonchianTradingSystemLab(context,
                        //    new List<Security>() { ticker }, logger);
                        //system.SetParameters(ticker);
                        //system.Initialize();
                        //system.Run();
                    }
                }
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
                    if (str.Contains("5min"))
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
            Settings settings)
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

                        var ticker = CreateTicker(fileName, data, timeFrame);

                        result.Add(ticker);
                    }
                }
            }

            return result;
        }

        private Ticker CreateTicker(string fileName, SecurityData data, Interval timeframe)
        {
            var converter = ConverterTextDataToBar.Create(fileName);
            var bars = converter.ConvertFileWithBarsToListOfBars();            

            var ticker = new Ticker(data.Name, data.Currency, data.Shares, bars,
                logger, data.CommissionRate);

            return ticker;
        }

        private List<Bar> CompressBars(List<Bar> bars)
        {
            //TODO: реализовать сжатие баров
            return bars;
        }

        private List<TradingSystemParameters> CreateSystemParameters(Settings settings, List<Ticker> tickers)
        {
            var listTradingSystemParameters = new List<TradingSystemParameters>();

            foreach (var ticker in tickers)
            {
                foreach (var tF in settings.TimeFrames)
                {                    
                    var bars = CompressBars(ticker.Bars);
                    var security = new SecurityLab(ticker.Name, ticker.Currency, ticker.Shares, bars,
                ticker.Logger, ticker.CommissionRate);
                    foreach (var pSide in settings.Sides)
                    {
                        for (var slowDonchian = 9; slowDonchian <= 208; slowDonchian++)
                        {
                            for (var fastDonchian = 9; fastDonchian <= 208; fastDonchian++)
                            {
                                for (var atrPeriod = 1; atrPeriod <= 25; atrPeriod++)
                                {
                                    for (var limitOpenedPositions = 1; limitOpenedPositions <= 4;
                                        limitOpenedPositions++)
                                    {
                                        for (var kAtrForOpenPosition = 0.5; kAtrForOpenPosition <= 2;
                                            kAtrForOpenPosition = kAtrForOpenPosition + 0.5)
                                        {
                                            for (var kAtrForStopLoss = 0.5; kAtrForStopLoss <= 2;
                                            kAtrForStopLoss = kAtrForStopLoss + 0.5)
                                            {
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
                                                systemParameters.Add("shares", 1);// разобраться!!!

                                                systemParameters.Add("equity", 1000000d);
                                                systemParameters.Add("riskValuePrcnt", 2d);
                                                systemParameters.Add("contracts", 0);

                                                listTradingSystemParameters.Add(new TradingSystemParameters()
                                                {
                                                    Security = security,
                                                    SystemParameter = systemParameters
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return listTradingSystemParameters;
        }
    }
}