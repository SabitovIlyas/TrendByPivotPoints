using PeparatorDataForSpreadTradingSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using TradingSystems;
using TSLab.DataSource;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStarter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Старт!");

            OpenFileDialog openFileDialog = new OpenFileDialog();            
            openFileDialog.Title = "Выберите файлы с историческими данными";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            var fullFileName = openFileDialog.FileName;
            var converter = ConverterTextDataToBar.Create(fullFileName);
            var fileName = fullFileName.Split('\\').Last();
            var securityName = fileName.Split('.').First();
            var bars = converter.ConvertFileWithBarsToListOfBars();
            var interval = new Interval(60, DataIntervals.MINUTE);
            bars = BarCompressor.CompressBars(bars, interval);

            var logger = new LoggerNull();

            try
            {
                var context = new ContextLab();
                var security = new SecurityLab(securityName, Currency.USD, shares: 10,
                    5000, 4500, bars, logger);

                var securities = new List<TradingSystems.Security>() { security };
                var system = new StarterDonchianTradingSystemLab(context, securities, logger);
                var systemParameters = GetSystemParameters();
                system.SetParameters(systemParameters);
                system.Initialize();

                var equity = system.Account.Equity;
                Console.WriteLine($"Эквити начальный: {equity}");

                system.Run();

                Console.WriteLine($"Эквити конечный: {equity}");
                Console.WriteLine($"Максимальная просадка: {system.Account.GetMaxDrawDown()}%");

                var deals = security.GetDeals();

                Console.WriteLine($"Всего {deals.Count} сделок.");
                var dNmbr = 0;
                foreach (var d in deals)
                {                    
                    Console.WriteLine($"Deal № {dNmbr}, {d.PositionSide}, {d.BarNumberOpenPosition}, " +
                        $"{d.BarNumberClosePosition}, {d.EntryPrice}, {d.ExitPrice}, {d.Contracts}, " +
                        $"{d.GetProfit()}, {d.SignalNameForOpenPosition}, {d.SignalNameForClosePosition}," +
                        $" {equity}");
                    dNmbr++;
                }

                deals = security.GetMetaDeals();
                Console.WriteLine($"\r\nВсего {deals.Count} метасделок.");
                dNmbr = 0;
                foreach (var d in deals)
                {
                    Console.WriteLine($"Deal № {dNmbr}, {d.PositionSide}, {d.BarNumberOpenPosition}, " +
                        $"{d.BarNumberClosePosition}, {d.EntryPrice}, {d.ExitPrice}, {d.Contracts}, " +
                        $"{d.GetProfit()}, {equity}");
                    dNmbr++;
                }

                Console.WriteLine($"\r\nПрибыль составила: " +
                    $"{system.Account.Equity-system.Account.InitDeposit}.");

            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
            }

            Console.ReadLine();
        }

        private static SystemParameters GetSystemParameters()
        {
            var systemParameters = new SystemParameters();

            systemParameters.Add("slowDonchian", 39);
            systemParameters.Add("fastDonchian", 14);
            systemParameters.Add("kAtrForOpenPosition", 0.5d);
            systemParameters.Add("kAtrForStopLoss", 0.5d);            
            systemParameters.Add("atrPeriod", 15);

            systemParameters.Add("limitOpenedPositions", 2);
            systemParameters.Add("isUSD", 1);
            systemParameters.Add("rateUSD", 84d);
            systemParameters.Add("positionSide", 1);
            systemParameters.Add("shares", 10);
            systemParameters.Add("scaleContractsPrcnt", 100d);
            systemParameters.Add("riskValuePrcnt", 100d);
            systemParameters.Add("contracts", 0);
            systemParameters.Add("equity", 100000d);

            return systemParameters;
        }
    }
}