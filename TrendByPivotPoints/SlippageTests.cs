using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TradingSystems.Tests
{
    /// <summary>
    /// Проскальзывание задаётся в рублях на сторону, а не долей от цены: шаг цены
    /// фиксированный, и процентная модель занижает издержки на низких ценах.
    /// Проверяем, что оно списывается с обеих сторон сделки и не ломает прежние
    /// прогоны, где его не было.
    /// </summary>
    [TestClass()]
    public class SlippageTests
    {
        private List<Bar> bars;

        [TestInitialize]
        public void TestInitialize()
        {
            bars = new List<Bar>()
            {
                Bar.Create(new DateTime(2026, 7, 29, 10, 00, 00), 100, 110, 95, 102, 1, "SPFB.TEST", "1", 0),
                Bar.Create(new DateTime(2026, 7, 29, 11, 00, 00), 105, 112, 104, 108, 1, "SPFB.TEST", "1", 0),
                Bar.Create(new DateTime(2026, 7, 29, 12, 00, 00), 120, 125, 118, 122, 1, "SPFB.TEST", "1", 0),
                Bar.Create(new DateTime(2026, 7, 29, 13, 00, 00), 124, 130, 123, 128, 1, "SPFB.TEST", "1", 0),
            };
        }

        [TestMethod()]
        public void BezProskalzyvaniya_PribylKakRanshe()
        {
            //Вход по 105, выход по 120, один контракт, комиссии нет.
            var profit = RunTrade(slippagePerSide: 0);

            Assert.AreEqual(15, profit, 1e-9);
        }

        [TestMethod()]
        public void ProskalzyvanieSpisyvaetsyaSObeikhStoron()
        {
            //Два рубля на сторону — значит четыре рубля со сделки.
            var profit = RunTrade(slippagePerSide: 2);

            Assert.AreEqual(15 - 4, profit, 1e-9);
        }

        [TestMethod()]
        public void ProskalzyvanieUmnozhaetsyaNaKontrakty()
        {
            var one = RunTrade(slippagePerSide: 2, contracts: 1);
            var three = RunTrade(slippagePerSide: 2, contracts: 3);

            //Прибыль без издержек втрое больше, издержки — тоже втрое.
            Assert.AreEqual(3 * one, three, 1e-9);
            Assert.AreEqual(3 * (15 - 4), three, 1e-9);
        }

        [TestMethod()]
        public void ProskalzyvanieNeZavisitOtTseny()
        {
            //Комиссия — доля от цены, проскальзывание — фиксированная величина.
            //При удвоении цены комиссия удваивается, проскальзывание нет.
            var cheap = GetSlippagePerSide(price: 100, slippagePerSide: 3);
            var expensive = GetSlippagePerSide(price: 200, slippagePerSide: 3);

            Assert.AreEqual(cheap, expensive, 1e-9,
                "Проскальзывание не должно зависеть от цены.");
            Assert.AreEqual(3, cheap, 1e-9);
        }

        [TestMethod()]
        public void KlonBumagiSokhranyaetProskalzyvanie()
        {
            //Фитнес-функция клонирует бумагу; потеря настройки означала бы, что
            //часть прогонов считается по другим издержкам.
            var security = CreateSecurity(commissionRate: 0.0001, slippagePerSide: 5);
            var clone = (SecurityLab)security.GetClone();

            Assert.AreEqual(5, clone.SlippagePerSide, 1e-9);
            Assert.AreEqual(0.0001, clone.CommissionRate, 1e-12);
        }

        /// <summary>
        /// Во сколько обошлось проскальзывание одной стороны при заданном уровне цен:
        /// разница между сделкой с ним и без него, поделённая на две стороны.
        /// Бумага каждый раз новая — она ведёт номер бара и повторно не запускается.
        /// </summary>
        private double GetSlippagePerSide(double price, double slippagePerSide)
        {
            var scaled = ScaleBars(price);

            var withSlippage = Trade(CreateSecurity(0, slippagePerSide, scaled), contracts: 1);
            var withoutSlippage = Trade(CreateSecurity(0, 0, scaled), contracts: 1);

            return (withoutSlippage - withSlippage) / 2;
        }

        private List<Bar> ScaleBars(double price)
        {
            var scaled = new List<Bar>();
            var factor = price / 100;

            foreach (var bar in bars)
                scaled.Add(Bar.Create(bar.Date, bar.Open * factor, bar.High * factor,
                    bar.Low * factor, bar.Close * factor, 1, "SPFB.TEST", "1", 0));

            return scaled;
        }

        private double RunTrade(double slippagePerSide, int contracts = 1)
        {
            var security = CreateSecurity(commissionRate: 0, slippagePerSide: slippagePerSide);
            return Trade(security, contracts);
        }

        private double Trade(SecurityLab security, int contracts)
        {
            security.Update(0);
            security.BuyAtMarket(barNumber: 0, contracts: contracts, "LE");
            security.Update(1);

            var position = security.GetLastActiveForSignal("LE", barNumber: 1);
            Assert.IsNotNull(position, "Позиция не открылась.");

            security.CloseAtMarket(barNumber: 1, "LX", "выход", position);
            security.Update(2);

            return position.GetProfit(barNumber: 2);
        }

        private SecurityLab CreateSecurity(double commissionRate, double slippagePerSide,
            List<Bar> barsForSecurity = null)
        {
            var security = new SecurityLab(Currency.RUB, shares: 1,
                barsForSecurity ?? bars, new LoggerNull(), commissionRate);
            security.SlippagePerSide = slippagePerSide;
            return security;
        }
    }
}
