using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TrendByPivotPointsStarter;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class TradingSecurityLabTests1
    {
        List<Bar> bars;
        Security security;
        SecurityLab sec;

        [TestInitialize]
        public void TestInitialize()
        {
            var logger = new LoggerNull();
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

            security = new SecurityLab(Currency.Ruble, shares: 1, bars, logger);

            var context = new ContextLab();
            var securities = new List<Security>() { security };
            var starter = new StarterDonchianTradingSystemLab(context, securities, logger);
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

            systemParameters.Add("equity", 100000d);
            systemParameters.Add("riskValuePrcnt", 100d); //реализовать и оттестировать в другом тесте
            systemParameters.Add("contracts", 1);

            starter.SetParameters(systemParameters);
            starter.Initialize();
            starter.Run();

            sec = security as SecurityLab;
        }

        [TestMethod()]
        public void GetLastActiveForSignal_Test()
        {
            var position = security.GetLastActiveForSignal("LE Вход №1", barNumber: 11);
            Assert.IsNotNull(position);
        }

        [TestMethod()]
        public void GetOrdersByBarsTest()
        {
            var orders = sec.GetOrders(barNumber: 9);
            Assert.IsTrue(orders.Count == 0);

            orders = sec.GetOrders(barNumber: 10);
            Assert.IsTrue(orders.Count == 1);
            Assert.IsTrue(orders[0].Price == 90000);

            orders = sec.GetOrders(barNumber: 11);
            Assert.IsTrue(orders.Count == 2);
            Assert.IsTrue(orders[0].Price == 90000);
            Assert.IsTrue(orders[1].Price == 89000);

            orders = sec.GetOrders(barNumber: 12);
            Assert.IsTrue(orders.Count == 3);
            Assert.IsTrue(orders[0].Price == 90000);
            Assert.IsTrue(orders[1].Price == 89000);
            Assert.IsTrue(orders[2].Price == 86000);

            orders = sec.GetOrders(barNumber: 13);
            Assert.IsTrue(orders.Count == 4);
            Assert.IsTrue(orders[0].Price == 90000);
            Assert.IsTrue(orders[1].Price == 89000);
            Assert.IsTrue(orders[2].Price == 86000);
            Assert.IsTrue(orders[3].Price == 87000);

            orders = sec.GetOrders(barNumber: 18);
            Assert.IsTrue(orders.Count == 9);
            Assert.IsTrue(orders[0].Price == 90000);
            Assert.IsTrue(orders[1].Price == 89000);
            Assert.IsTrue(orders[2].Price == 86000);
            Assert.IsTrue(orders[3].Price == 87000);
            Assert.IsTrue(orders[4].Price == 88000);
            Assert.IsTrue(orders[5].Price == 89000);
            Assert.IsTrue(orders[6].Price == 90000);
            Assert.IsTrue(orders[7].Price == 91000);
            Assert.IsTrue(orders[8].Price == 92000);
        }

        [TestMethod()]
        public void GetProfit()
        {
            double expected = 92000 - 89000;
            double actual = sec.GetProfit(barNumber: 13);
            Assert.AreEqual(expected, actual);
        }
    }
}