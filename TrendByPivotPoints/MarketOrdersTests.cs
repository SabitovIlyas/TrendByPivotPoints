using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class MarketOrdersTests
    {
        List<Bar> bars;
        SecurityLab security;

        [TestInitialize]
        public void TestInitialize()
        {
            var logger = new LoggerNull();
            bars = new List<Bar>()
            {
                Bar.Create(new DateTime(2026,7,29,10,00,00),100,110,95,102,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2026,7,29,11,00,00),105,112,104,108,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2026,7,29,12,00,00),107,115,106,110,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2026,7,29,13,00,00),111,118,109,116,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2026,7,29,14,00,00),117,120,113,114,1, "SPFB.TEST", "1",0),
            };

            security = new SecurityLab(Currency.RUB, shares: 1, bars, logger, commissionRate: 0);
        }

        [TestMethod()]
        public void BuyAtMarket_OpensLongAtNextBarOpen()
        {
            security.Update(0);
            security.BuyAtMarket(barNumber: 1, contracts: 2, "LE");
            security.Update(1);

            var position = security.GetLastActiveForSignal("LE", barNumber: 1);
            Assert.IsNotNull(position);
            Assert.AreEqual(PositionSide.Long, position.PositionSide);
            Assert.AreEqual(105, position.EntryPrice);
            Assert.AreEqual(2, position.Contracts);
        }

        [TestMethod()]
        public void SellAtMarket_OpensShortAtNextBarOpen()
        {
            security.Update(0);
            security.SellAtMarket(barNumber: 1, contracts: 3, "SE");
            security.Update(1);

            var position = security.GetLastActiveForSignal("SE", barNumber: 1);
            Assert.IsNotNull(position);
            Assert.AreEqual(PositionSide.Short, position.PositionSide);
            Assert.AreEqual(105, position.EntryPrice);
            Assert.AreEqual(3, position.Contracts);
        }

        [TestMethod()]
        public void CloseAtMarket_ClosesLongAtNextBarOpen()
        {
            security.Update(0);
            security.BuyAtMarket(barNumber: 1, contracts: 1, "LE");
            security.Update(1);
            var position = security.GetLastActiveForSignal("LE", barNumber: 1);

            security.CloseAtMarket(barNumber: 2, "LXM", "", position);
            security.Update(2);

            Assert.IsNull(security.GetLastActiveForSignal("LE", barNumber: 2));
            Assert.AreEqual(107, position.ExitPrice);
            Assert.AreEqual(2, position.BarNumberClosePosition);
        }

        [TestMethod()]
        public void CloseAtMarket_CancelsActiveStopOrderOfSamePosition()
        {
            security.Update(0);
            security.BuyAtMarket(barNumber: 1, contracts: 1, "LE");
            security.Update(1);
            var position = security.GetLastActiveForSignal("LE", barNumber: 1);

            //Стоп внутри диапазона бара 2 (Low = 106) — без отмены он бы тоже исполнился.
            security.CloseAtStop(barNumber: 2, stopPrice: 106.5, "LXS", "", position);
            security.CloseAtMarket(barNumber: 2, "LXM", "", position);
            security.Update(2);
            security.Update(3);
            security.Update(4);

            Assert.IsNull(security.GetLastActiveForSignal("LE", barNumber: 2));
            //Позиция закрыта рыночным ордером по открытию бара 2, а не по стопу.
            Assert.AreEqual(107, position.ExitPrice);
            Assert.AreEqual("LXM", position.SignalNameForClosePosition);
            Assert.AreEqual(1, security.GetDeals().Count);
        }

        [TestMethod()]
        public void CloseByStopLossMarket_RemovesPositionFromActive()
        {
            security.Update(0);
            security.SellAtMarket(barNumber: 1, contracts: 1, "SE");
            security.Update(1);
            var position = security.GetLastActiveForSignal("SE", barNumber: 1);

            security.CloseAtMarket(barNumber: 2, "SXM", "", position);
            security.Update(2);
            security.Update(3);

            Assert.IsNull(security.GetLastActiveForSignal("SE", barNumber: 3));
            Assert.AreEqual(107, position.ExitPrice);
        }
    }
}
