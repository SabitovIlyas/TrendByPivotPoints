using PeparatorDataForSpreadTradingSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradingSystems;
using TrendByPivotPointsStarter;

namespace TrendByPivotPointsOptimizator
{
    public class OptimizatorStarter
    {
        public void Start()
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

                //TODO: Реализовать список из security с разным значениям Currency, Shares и пр.
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

            systemParameters.Add("slowDonchian", 50);
            systemParameters.Add("fastDonchian", 20);
            systemParameters.Add("kAtr", 2d);
            systemParameters.Add("atrPeriod", 20);

            systemParameters.Add("limitOpenedPositions", 4);
            systemParameters.Add("isUSD", 1);
            systemParameters.Add("rateUSD", 100d);
            systemParameters.Add("positionSide", 0);
            systemParameters.Add("shares", 10);

            return systemParameters;
        }
    }
}