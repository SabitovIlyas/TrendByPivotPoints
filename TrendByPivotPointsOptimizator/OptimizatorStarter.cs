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
            openFileDialog.Title = "Выберите файл с инструментами";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            var fullFileName = openFileDialog.FileName;
            List<SecurityData> securitiesData = GetSecuritiesData(fullFileName);
            List<Security> secs = CreateSecurities(securitiesData, fullFileName);
            

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
                        Currency = currency,
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

        private List<Security> CreateSecurities(List<SecurityData> securitiesData, string fullFileName)
        {
            var result = new List<Security>();
            foreach (var data in securitiesData)
            {
                var securityName = data.Name;
                var fileNameSplitted = fullFileName.Split('\\');

                var path = string.Empty;
                for (var i = 0; i < fileNameSplitted.Length - 1; i++)
                    path += fileNameSplitted[i] + "\\";
                var fileName = path + securityName + ".txt";

                var security = CreateSecurity(fileName);

                result.Add(security);
            }
            return result;
        }

        private Security CreateSecurity(string fileName)
        {
            var converter = ConverterTextDataToBar.Create(fileName);
            var bars = converter.ConvertFileWithBarsToListOfBars();



            return null;
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

    struct SecurityData
    {
        public string Name;
        public string Currency;
        public int Shares;
        public double CommissionRate;
    }
}