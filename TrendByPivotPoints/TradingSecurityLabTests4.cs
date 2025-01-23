using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TrendByPivotPointsStarter;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class TradingSecurityLabTests4
    {
        List<Bar> bars;
        Security security;
        SecurityLab sec;
        Starter starter;

        [TestInitialize]
        public void TestInitialize()
        {
            FillBars();
            security = new SecurityLab(Currency.Ruble, shares: 1, bars);

            var context = new ContextLab();
            var securities = new List<Security>() { security };
            var logger = new LoggerNull();
            starter = new StarterDonchianTradingSystemLab(context, securities, logger);
            var systemParameters = new SystemParameters();

            systemParameters.Add("slowDonchian", 10);
            systemParameters.Add("fastDonchian", 5);
            systemParameters.Add("kAtr", 0.5d);
            systemParameters.Add("atrPeriod", 5);
            systemParameters.Add("limitOpenedPositions", 4);
            systemParameters.Add("isUSD", 0);
            systemParameters.Add("rateUSD", 0d);
            systemParameters.Add("positionSide", 0);
            systemParameters.Add("shares", 1);

            systemParameters.Add("equity", 1000000d);
            systemParameters.Add("riskValuePrcnt", 2d);
            systemParameters.Add("contracts", 0);

            starter.SetParameters(systemParameters);
            starter.Initialize();
            starter.Run();

            sec = security as SecurityLab;
        }

        private void FillBars()
        {
            bars = new List<Bar>()
            {
                //Bar.Create(new DateTime(2023,12,31,09,30,00), open: 5425, high: 5500, low: 5400, close: 5475, volume: 10000, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,01,09,30,00), open: 5425, high: 5500, low: 5400, close: 5475, volume: 10000, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),

                // Восходящий тренд
                Bar.Create(new DateTime(2024,01,02,09,30,00), open: 5000, high: 5100, low: 4950, close: 5050, volume: 12345, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,03,09,31,00), open: 5060, high: 5150, low: 5040, close: 5130, volume: 12567, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,04,09,32,00), open: 5140, high: 5200, low: 5120, close: 5180, volume: 12789, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,05,09,33,00), open: 5190, high: 5250, low: 5170, close: 5230, volume: 13012, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,06,09,34,00), open: 5240, high: 5300, low: 5220, close: 5280, volume: 13235, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,07,09,35,00), open: 5290, high: 5350, low: 5270, close: 5330, volume: 13458, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,08,09,36,00), open: 5340, high: 5400, low: 5320, close: 5380, volume: 13681, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,09,09,37,00), open: 5390, high: 5450, low: 5370, close: 5430, volume: 13904, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,10,09,38,00), open: 5440, high: 5500, low: 5420, close: 5480, volume: 14128, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,11,09,39,00), open: 5490, high: 5550, low: 5470, close: 5530, volume: 14351, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,12,09,40,00), open: 5540, high: 5600, low: 5520, close: 5580, volume: 14574, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,13,09,41,00), open: 5590, high: 5650, low: 5570, close: 5630, volume: 14797, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,14,09,42,00), open: 5640, high: 5700, low: 5620, close: 5680, volume: 15020, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,15,09,43,00), open: 5690, high: 5750, low: 5670, close: 5730, volume: 15244, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,16,09,44,00), open: 5740, high: 5800, low: 5720, close: 5780, volume: 15467, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,17,09,45,00), open: 5790, high: 5850, low: 5770, close: 5830, volume: 15690, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,18,09,46,00), open: 5840, high: 5900, low: 5820, close: 5880, volume: 15913, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,19,09,47,00), open: 5890, high: 5950, low: 5870, close: 5930, volume: 16136, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,20,09,48,00), open: 5940, high: 6000, low: 5920, close: 5980, volume: 16360, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,21,09,49,00), open: 5990, high: 6050, low: 5970, close: 6030, volume: 16583, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,22,09,50,00), open: 6040, high: 6100, low: 6020, close: 6080, volume: 16806, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,23,09,51,00), open: 6090, high: 6150, low: 6070, close: 6130, volume: 17029, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,24,09,52,00), open: 6140, high: 6200, low: 6120, close: 6180, volume: 17253, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,25,09,53,00), open: 6190, high: 6250, low: 6170, close: 6230, volume: 17476, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,26,09,54,00), open: 6240, high: 6300, low: 6220, close: 6280, volume: 17699, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,27,09,55,00), open: 6290, high: 6350, low: 6270, close: 6330, volume: 17922, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,28,09,56,00), open: 6340, high: 6400, low: 6320, close: 6380, volume: 18146, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,29,09,57,00), open: 6390, high: 6450, low: 6370, close: 6430, volume: 18369, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,30,09,58,00), open: 6440, high: 6500, low: 6420, close: 6480, volume: 18592, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,01,31,09,59,00), open: 6490, high: 6550, low: 6470, close: 6530, volume: 18815, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),

                // Нисходящий тренд
                Bar.Create(new DateTime(2024,02,01,10,00,00), open: 6540, high: 6600, low: 6520, close: 6580, volume: 19039, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,02,10,01,00), open: 6570, high: 6590, low: 6510, close: 6530, volume: 19262, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,03,10,02,00), open: 6520, high: 6560, low: 6460, close: 6480, volume: 19485, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,04,10,03,00), open: 6470, high: 6510, low: 6410, close: 6430, volume: 19708, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,05,10,04,00), open: 6420, high: 6460, low: 6360, close: 6380, volume: 19932, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,06,10,05,00), open: 6370, high: 6410, low: 6310, close: 6330, volume: 20155, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,07,10,06,00), open: 6320, high: 6360, low: 6260, close: 6280, volume: 20378, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,08,10,07,00), open: 6270, high: 6310, low: 6210, close: 6230, volume: 20601, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,09,10,08,00), open: 6220, high: 6260, low: 6160, close: 6180, volume: 20825, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,10,10,09,00), open: 6170, high: 6210, low: 6110, close: 6130, volume: 21048, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,11,10,10,00), open: 6120, high: 6160, low: 6060, close: 6080, volume: 21271, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,12,10,11,00), open: 6070, high: 6110, low: 6010, close: 6030, volume: 21494, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,13,10,12,00), open: 6020, high: 6060, low: 5960, close: 5980, volume: 21718, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,14,10,13,00), open: 5970, high: 6010, low: 5910, close: 5930, volume: 21941, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,15,10,14,00), open: 5920, high: 5960, low: 5860, close: 5880, volume: 22164, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,16,10,15,00), open: 5870, high: 5910, low: 5810, close: 5830, volume: 22387, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,17,10,16,00), open: 5820, high: 5860, low: 5760, close: 5780, volume: 22611, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,18,10,17,00), open: 5770, high: 5810, low: 5710, close: 5730, volume: 22834, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,19,10,18,00), open: 5720, high: 5760, low: 5660, close: 5680, volume: 23057, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,20,10,19,00), open: 5670, high: 5710, low: 5610, close: 5630, volume: 23280, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,21,10,20,00), open: 5620, high: 5660, low: 5560, close: 5580, volume: 23504, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,22,10,21,00), open: 5570, high: 5610, low: 5510, close: 5530, volume: 23727, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,23,10,22,00), open: 5520, high: 5560, low: 5460, close: 5480, volume: 23950, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,24,10,23,00), open: 5470, high: 5510, low: 5410, close: 5430, volume: 24173, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,25,10,24,00), open: 5420, high: 5460, low: 5360, close: 5380, volume: 24397, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,26,10,25,00), open: 5370, high: 5410, low: 5310, close: 5330, volume: 24620, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,27,10,26,00), open: 5320, high: 5360, low: 5260, close: 5280, volume: 24843, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,28,10,27,00), open: 5270, high: 5310, low: 5210, close: 5230, volume: 25066, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,02,29,10,28,00), open: 5220, high: 5260, low: 5160, close: 5180, volume: 25290, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),

                // Восходящий тренд
                Bar.Create(new DateTime(2024,03,01,10,29,00), open: 5180, high: 5240, low: 5140, close: 5220, volume: 25513, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,02,10,30,00), open: 5230, high: 5290, low: 5190, close: 5270, volume: 25736, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,03,10,31,00), open: 5280, high: 5340, low: 5240, close: 5320, volume: 25959, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,04,10,32,00), open: 5330, high: 5390, low: 5290, close: 5370, volume: 26183, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,05,10,33,00), open: 5380, high: 5440, low: 5340, close: 5420, volume: 26406, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,06,10,34,00), open: 5430, high: 5490, low: 5390, close: 5470, volume: 26629, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,07,10,35,00), open: 5480, high: 5540, low: 5440, close: 5520, volume: 26852, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,08,10,36,00), open: 5530, high: 5590, low: 5490, close: 5570, volume: 27076, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,09,10,37,00), open: 5580, high: 5640, low: 5540, close: 5620, volume: 27299, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,10,10,38,00), open: 5630, high: 5690, low: 5590, close: 5670, volume: 27522, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,11,10,39,00), open: 5680, high: 5740, low: 5640, close: 5720, volume: 27745, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,12,10,40,00), open: 5730, high: 5790, low: 5690, close: 5770, volume: 27969, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,13,10,41,00), open: 5780, high: 5840, low: 5740, close: 5820, volume: 28192, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,14,10,42,00), open: 5830, high: 5890, low: 5790, close: 5870, volume: 28415, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,15,10,43,00), open: 5880, high: 5940, low: 5840, close: 5920, volume: 28638, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,16,10,44,00), open: 5930, high: 5990, low: 5890, close: 5970, volume: 28862, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,17,10,45,00), open: 5980, high: 6040, low: 5940, close: 6020, volume: 29085, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,18,10,46,00), open: 6030, high: 6090, low: 5990, close: 6070, volume: 29308, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,19,10,47,00), open: 6080, high: 6140, low: 6040, close: 6120, volume: 29531, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,20,10,48,00), open: 6130, high: 6190, low: 6090, close: 6170, volume: 29755, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,21,10,49,00), open: 6180, high: 6240, low: 6140, close: 6220, volume: 29978, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,22,10,50,00), open: 6230, high: 6290, low: 6190, close: 6270, volume: 30201, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,23,10,51,00), open: 6280, high: 6340, low: 6240, close: 6320, volume: 30424, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,24,10,52,00), open: 6330, high: 6390, low: 6290, close: 6370, volume: 30648, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,25,10,53,00), open: 6380, high: 6440, low: 6340, close: 6420, volume: 30871, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,26,10,54,00), open: 6430, high: 6490, low: 6390, close: 6470, volume: 31094, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
                Bar.Create(new DateTime(2024,03,27,10,55,00), open: 6480, high: 6540, low: 6440, close: 6520, volume: 31317, ticker:"TEST.TICKER", period: "1", digitsAfterPoint: 0),
            };
        }

        [TestMethod()]
        public void GetDeals()
        {   
            double expectedQtyDeals = 8;
            var deals = sec.GetDeals(barNumber: bars.Count - 1);
            double actual = deals.Count;
            Assert.AreEqual(expectedQtyDeals, actual);
        }
    }
}