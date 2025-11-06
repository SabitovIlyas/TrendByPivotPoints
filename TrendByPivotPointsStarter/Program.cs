using PeparatorDataForSpreadTradingSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsStarter
{
    class Program
    {
        static double rateUSD = 1d;

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
                var security = new SecurityLab(securityName, Currency.USD, shares: 1,
                    5000, 4500, bars, logger);
                security.RateUSD = rateUSD;

                var securities = new List<TradingSystems.Security>() { security };
                var system = new StarterDonchianTradingSystemLab(context, securities, logger);
                var systemParameters = GetSystemParameters();
                system.SetParameters(systemParameters);
                system.Initialize();

                var account = (AccountLab)system.Account;
                var equity = system.Account.Equity;

                Console.WriteLine();
                Console.WriteLine($"Эквити начальный: {equity}");

                system.Run();

                var profit = securities.First().GetProfit();
                Console.WriteLine($"Прибыль составила: {profit}");
                Console.WriteLine($"Максимальная просадка: {system.Account.GetMaxDrawDownPrcnt()}%");

                var profitPrcnt = account.GetProfitPrcnt();
                Console.WriteLine($"Прибыль составила: {profitPrcnt}%");

                var rec = account.GetRecoveryFactor();
                Console.WriteLine($"Фактор воcстановления составил: {rec}");                

                var deals = security.GetDeals();

                Console.WriteLine();
                Console.WriteLine($"Всего {deals.Count} сделок.");
                Console.WriteLine();

                var dNmbr = 0;
                foreach (var d in deals)
                {
                    var e = account.GetEquity(d.BarNumberClosePosition);
                    Console.WriteLine($"Deal № {dNmbr}, {d.PositionSide}, {d.BarNumberOpenPosition}, " +
                        $"{d.BarNumberClosePosition}, {d.EntryPrice}, {d.ExitPrice}, {d.Contracts}, " +
                        $"{d.GetProfit()}, {d.SignalNameForOpenPosition}, {d.SignalNameForClosePosition}," +
                        $" {e}");
                    dNmbr++;
                }

                deals = security.GetMetaDeals();
                Console.WriteLine($"\r\nВсего {deals.Count} метасделок.");
                dNmbr = 0;
                foreach (var d in deals)
                {
                    var e = account.GetEquity(d.BarNumberClosePosition);
                    Console.WriteLine($"Deal № {dNmbr}, {d.PositionSide}, {d.BarNumberOpenPosition}, " +
                        $"{d.BarNumberClosePosition}, {d.EntryPrice}, {d.ExitPrice}, {d.Contracts}, " +
                        $"{d.GetProfit()}, {e}");
                    dNmbr++;
                }                
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

            systemParameters.Add("slowDonchian", 40);
            systemParameters.Add("fastDonchian", 17);
            systemParameters.Add("kAtrForOpenPosition", 1d);
            systemParameters.Add("kAtrForStopLoss", 1d);            
            systemParameters.Add("atrPeriod", 8);

            systemParameters.Add("limitOpenedPositions", 1);
            systemParameters.Add("isUSD", 0);
            systemParameters.Add("rateUSD", rateUSD);
            systemParameters.Add("positionSide", 1);
            systemParameters.Add("shares", 1);
            systemParameters.Add("scaleContractsPrcnt", 100d);
            systemParameters.Add("riskValuePrcnt", 100d);
            systemParameters.Add("contracts", 0);
            systemParameters.Add("equity", 100000d);

            return systemParameters;
        }
    }
}