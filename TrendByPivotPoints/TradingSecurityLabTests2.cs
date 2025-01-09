using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TrendByPivotPointsStarter;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class TradingSecurityLabTests2
    {
        List<Bar> bars;
        Security security;
        SecurityLab sec;
        Starter starter;

        [TestInitialize]
        public void TestInitialize()
        {
            bars = new List<Bar>()
            {
                Bar.Create(new DateTime(2025,11,27,10,00,00),90000,90000,90000,90000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),80000,80000,80000,80000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),81000,81000,81000,81000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),82000,82000,82000,82000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),83000,83000,83000,83000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),84000,84000,84000,84000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),85000,85000,85000,85000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),86000,86000,86000,86000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),87000,87000,87000,87000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),88000,88000,88000,88000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),89000,89000,89000,89000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),90000,90000,90000,90000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),91000,91000,91000,91000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),92000,92000,92000,92000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),93000,93000,93000,93000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),94000,94000,94000,94000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),95000,95000,95000,95000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),94000,94000,94000,94000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),93000,93000,93000,93000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),92000,92000,92000,92000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),91000,91000,91000,91000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),90000,90000,90000,90000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),89000,89000,89000,89000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),88000,88000,88000,88000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),87000,87000,87000,87000,1, "SPFB.TEST", "1",0)
            };

            security = new SecurityLab(Currency.Ruble, shares: 1, bars);

            var context = new ContextLab();
            var securities = new List<Security>() { security };
            var logger = new LoggerNull();
            starter = new StarterDonchianTradingSystemLab(context, securities, logger);
            var systemParameters = new SystemParameters();

            systemParameters.Add("slowDonchian", 10);
            systemParameters.Add("fastDonchian", 5);
            systemParameters.Add("kAtr", 0d);
            systemParameters.Add("atrPeriod", 10);
            systemParameters.Add("limitOpenedPositions", 1);
            systemParameters.Add("isUSD", 0);
            systemParameters.Add("rateUSD", 0d);
            systemParameters.Add("positionSide", 0);
            systemParameters.Add("shares", 1);

            systemParameters.Add("Equity", 100000); //реализовать и оттестировать в другом тесте
            systemParameters.Add("riskValue", 1); //реализовать и оттестировать в другом тесте
            systemParameters.Add("contracts", 2);

            starter.SetParameters(systemParameters);
            starter.Initialize();
            starter.Run();

            sec = security as SecurityLab;
        }        

        [TestMethod()]
        public void GetProfit()
        {
            var expected = 2 * (92000 - 89000);
            var actual = sec.GetProfit(barNumber: 13);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetEquity()
        {
            var expected = 100000 + 2 * (92000 - 89000);
            var account = starter.Account;
            var actual =  ((AccountLab)account).GetEquity(barNumber: 13);
            Assert.AreEqual(expected, actual);
        }
    }
}