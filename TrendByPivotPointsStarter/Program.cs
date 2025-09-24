using PeparatorDataForSpreadTradingSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradingSystems;

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

            var logger = new ConsoleLogger();            

            try
            {
                var context = new ContextLab();
                var security = new SecurityLab(securityName, Currency.USD, shares: 10,
                    5000, 4500, bars, logger);

                var securities = new List<Security>() { security };
                var system = new StarterDonchianTradingSystemLab(context, securities, logger);
                var systemParameters = GetSystemParameters();
                system.SetParameters(systemParameters);
                system.Initialize();
                system.Run();                
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

            return systemParameters;
        }
    }
}