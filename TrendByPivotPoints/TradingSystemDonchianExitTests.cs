using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TrendByPivotPointsStarter;

namespace TradingSystems.Tests
{
    /// <summary>
    /// Выход по времени и отключаемый канальный выход у стратегии пробоя канала.
    /// Замеры по Si показывают, что преимущество пробоя проявляется на 100–400
    /// часах, а канальный стоп тянется за ценой и закрывает позицию заметно
    /// раньше — до этого срока она не доживает.
    /// </summary>
    [TestClass()]
    public class TradingSystemDonchianExitTests
    {
        private void AddBar(List<Bar> bars, double open, double high, double low, double close)
        {
            var dateTime = new DateTime(2026, 1, 5, 10, 0, 0).AddHours(bars.Count);
            bars.Add(Bar.Create(dateTime, open, high, low, close, 1, "SPFB.TEST", "60", 0));
        }

        /// <summary>
        /// Плавное снижение, затем пробой верхней границы канала и ровный рост.
        /// Снижение важно: на плоском ряду верхняя граница совпадает с максимумами
        /// баров, и стоп-заявка на вход исполнилась бы задолго до пробоя.
        /// Вход приходится на бар № 12 по цене 109.
        /// </summary>
        private List<Bar> CreateBreakoutThenSteadyRise(int risingBars)
        {
            var bars = new List<Bar>();
            for (var i = 0; i < 12; i++)
                AddBar(bars, 110 - i, 111 - i, 109 - i, 110 - i);

            AddBar(bars, 100, 112, 100, 111);   //бар № 12: пробой, исполнение по 109

            for (var i = 0; i < risingBars; i++)
                AddBar(bars, 111 + i, 113 + i, 111 + i, 112 + i);

            return bars;
        }

        private SecurityLab Run(List<Bar> bars, int maxBarsInPosition = 0,
            int useChannelExit = 1, double kAtrForStopLoss = 2, int? useTimeExit = null)
        {
            var logger = new LoggerNull();
            var security = new SecurityLab(Currency.RUB, shares: 1, bars, logger,
                commissionRate: 0);

            var context = new ContextLab();
            var securities = new List<Security>() { security };
            var starter = new StarterDonchianTradingSystemLab(context, securities, logger);

            var parameters = new SystemParameters();
            parameters.Add("slowDonchian", 10);
            parameters.Add("fastDonchian", 5);
            parameters.Add("kAtrForStopLoss", kAtrForStopLoss);
            parameters.Add("kAtrForOpenPosition", 0.5d);
            parameters.Add("atrPeriod", 10);
            parameters.Add("limitOpenedPositions", 1);
            parameters.Add("maxBarsInPosition", maxBarsInPosition);
            parameters.Add("useChannelExit", useChannelExit);

            //Не передаём вовсе, если не задан: так проверяется и старое правило,
            //по которому признаком служила ненулевая длительность.
            if (useTimeExit.HasValue)
                parameters.Add("useTimeExit", useTimeExit.Value);

            parameters.Add("positionSide", 0);
            parameters.Add("isUSD", 0);
            parameters.Add("rateUSD", 0d);
            parameters.Add("shares", 1d);
            parameters.Add("equity", 100000d);
            parameters.Add("riskValuePrcnt", 100d);
            parameters.Add("contracts", 1);

            starter.SetParameters(parameters);
            starter.Initialize();
            starter.Run();

            return security;
        }

        [TestMethod()]
        public void WithoutTimeExit_PositionStaysOpenWhileTrendHolds()
        {
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);

            var security = Run(bars, maxBarsInPosition: 0);

            var deals = security.GetDeals();
            Assert.AreEqual(1, deals.Count);
            Assert.AreEqual(int.MaxValue, deals[0].BarNumberClosePosition,
                "Без выхода по времени ровный рост позицию не закрывает.");
        }

        [TestMethod()]
        public void TimeExit_ClosesPositionAfterGivenNumberOfBars()
        {
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);

            var withoutTimeExit = Run(bars, maxBarsInPosition: 0);
            var withTimeExit = Run(bars, maxBarsInPosition: 5);

            var openBar = withoutTimeExit.GetDeals()[0].BarNumberOpenPosition;
            var deal = withTimeExit.GetDeals()[0];

            Assert.AreEqual(openBar, deal.BarNumberOpenPosition,
                "Вход не должен зависеть от выхода по времени.");
            //Срок вышел на баре openBar + 5, приказ исполняется на следующем.
            Assert.AreEqual(openBar + 6, deal.BarNumberClosePosition);
            StringAssert.Contains(deal.SignalNameForClosePosition, "время");
        }

        [TestMethod()]
        public void TimeExit_DoesNotFireBeforeItsTerm()
        {
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);

            var security = Run(bars, maxBarsInPosition: 50);

            var deals = security.GetDeals();
            Assert.AreEqual(1, deals.Count);
            Assert.AreEqual(int.MaxValue, deals[0].BarNumberClosePosition,
                "Срок не вышел — позиция остаётся открытой.");
        }

        [TestMethod()]
        public void TimeExitSwitchOff_IgnoresDurationEvenWhenItIsSet()
        {
            //Переключатель главнее длительности: при выключенном выходе срок не
            //имеет значения.
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);

            var security = Run(bars, maxBarsInPosition: 5, useTimeExit: 0);

            var deals = security.GetDeals();
            Assert.AreEqual(1, deals.Count);
            Assert.AreEqual(int.MaxValue, deals[0].BarNumberClosePosition);
        }

        [TestMethod()]
        public void TimeExitSwitchOn_ClosesPositionByDuration()
        {
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);

            var openBar = Run(bars, useTimeExit: 0).GetDeals()[0].BarNumberOpenPosition;
            var deal = Run(bars, maxBarsInPosition: 5, useTimeExit: 1).GetDeals()[0];

            Assert.AreEqual(openBar + 6, deal.BarNumberClosePosition);
            StringAssert.Contains(deal.SignalNameForClosePosition, "время");
        }

        [TestMethod()]
        public void TimeExitSwitchOn_WithZeroDurationClosesNoEarlierThanNextBar()
        {
            //Нулевая длительность при включённом выходе не должна закрывать позицию
            //на том же баре, на котором она открылась.
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);

            var openBar = Run(bars, useTimeExit: 0).GetDeals()[0].BarNumberOpenPosition;
            var deal = Run(bars, maxBarsInPosition: 0, useTimeExit: 1).GetDeals()[0];

            Assert.IsTrue(deal.BarNumberClosePosition > deal.BarNumberOpenPosition,
                "Позиция не может закрыться на баре открытия.");
            Assert.AreEqual(openBar + 2, deal.BarNumberClosePosition);
        }

        [TestMethod()]
        public void WithoutSwitch_NonZeroDurationStillWorks()
        {
            //Старые файлы настроек переключателя не содержат: признаком включения
            //служит ненулевая длительность.
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);

            var openBar = Run(bars, maxBarsInPosition: 0).GetDeals()[0].BarNumberOpenPosition;
            var deal = Run(bars, maxBarsInPosition: 5).GetDeals()[0];

            Assert.AreEqual(openBar + 6, deal.BarNumberClosePosition);
        }

        [TestMethod()]
        public void ChannelExitOff_SurvivesPullbackThatChannelStopWouldCatch()
        {
            //Рост, затем откат ниже минимума последних баров: канальный стоп его
            //ловит, а стоп по ATR стоит от цены входа далеко и не срабатывает.
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);
            AddBar(bars, 123, 123, 112, 113);
            AddBar(bars, 113, 115, 112, 114);
            AddBar(bars, 114, 116, 113, 115);

            var withChannel = Run(bars, useChannelExit: 1);
            var withoutChannel = Run(bars, useChannelExit: 0);

            var closedByChannel = withChannel.GetDeals()[0];
            var stillOpen = withoutChannel.GetDeals()[0];

            Assert.AreNotEqual(int.MaxValue, closedByChannel.BarNumberClosePosition,
                "С канальным выходом откат должен закрыть позицию.");
            Assert.AreEqual(int.MaxValue, stillOpen.BarNumberClosePosition,
                "Без канального выхода позицию держит только стоп по ATR.");
        }

        [TestMethod()]
        public void ChannelExitOff_KeepsChannelWhenAtrStopIsAbsent()
        {
            //При нулевом множителе ATR-стоп вырождается в цену входа. Отключать
            //канал в этом случае нельзя — позиция выбивалась бы сразу.
            var bars = CreateBreakoutThenSteadyRise(risingBars: 12);

            var security = Run(bars, useChannelExit: 0, kAtrForStopLoss: 0);

            var deals = security.GetDeals();
            Assert.AreEqual(1, deals.Count);
            Assert.AreEqual(int.MaxValue, deals[0].BarNumberClosePosition,
                "Позиция не должна закрываться на первом же баре после входа.");
        }
    }
}
