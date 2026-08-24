using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TrendByPivotPointsStarter;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class TradingSystemMeanReversionTests
    {
        //Восходящий тренд из 21 бара (закрытия 100..140 с шагом 2), затем резкий
        //провал вниз при цене выше SMA(20) — сигнал на вход в лонг по бару № 21,
        //вход по рынку на открытии бара № 22.
        private List<Bar> CreateUptrendWithDip()
        {
            var bars = new List<Bar>();
            for (var i = 0; i <= 20; i++)
                AddBar(bars, 99 + 2 * i, 101 + 2 * i, 98 + 2 * i, 100 + 2 * i);

            AddBar(bars, 139, 139, 130, 131);   //бар № 21: провал, закрытие выше SMA
            return bars;
        }

        //Нисходящий тренд (закрытия 140..100), затем резкий выброс вверх при цене
        //ниже SMA(20) — сигнал на вход в шорт по бару № 21.
        private List<Bar> CreateDowntrendWithSpike()
        {
            var bars = new List<Bar>();
            for (var i = 0; i <= 20; i++)
                AddBar(bars, 141 - 2 * i, 142 - 2 * i, 139 - 2 * i, 140 - 2 * i);

            AddBar(bars, 101, 110, 101, 109);   //бар № 21: выброс, закрытие ниже SMA
            return bars;
        }

        private void AddBar(List<Bar> bars, double open, double high, double low, double close)
        {
            var dateTime = new DateTime(2026, 1, 5, 10, 0, 0).AddHours(bars.Count);
            bars.Add(Bar.Create(dateTime, open, high, low, close, 1, "SPFB.TEST", "60", 0));
        }

        private SecurityLab Run(List<Bar> bars, int positionSide,
            double atrMultiplier = 3.0, int useTrailingStop = 0, int rsiExitLevel = 50,
            int? rsiEntryMode = null, int? rsiExitMode = null)
        {
            var logger = new LoggerNull();
            var security = new SecurityLab(Currency.RUB, shares: 1, bars, logger,
                commissionRate: 0);

            var context = new ContextLab();
            var securities = new List<Security>() { security };
            var starter = new StarterMeanReversionTradingSystemLab(context, securities, logger);

            var parameters = new SystemParameters();
            parameters.Add("maPeriod", 20);
            parameters.Add("rsiEntryPeriod", 2);
            parameters.Add("rsiExitPeriod", 2);
            parameters.Add("atrPeriod", 2);
            parameters.Add("rsiEntryLevel", 50);
            parameters.Add("rsiExitLevel", rsiExitLevel);
            parameters.Add("atrMultiplier", atrMultiplier);
            parameters.Add("useTrailingStop", useTrailingStop);

            //Не передаём вовсе, если режим не задан: так проверяется и то, что
            //стратегия работает по-старому со старым набором параметров.
            if (rsiEntryMode.HasValue)
                parameters.Add("rsiEntryMode", rsiEntryMode.Value);
            if (rsiExitMode.HasValue)
                parameters.Add("rsiExitMode", rsiExitMode.Value);

            parameters.Add("positionSide", positionSide);
            parameters.Add("isUSD", 0);
            parameters.Add("rateUSD", 1d);
            parameters.Add("shares", 1d);
            parameters.Add("riskValuePrcnt", 2d);
            parameters.Add("contracts", 1);
            parameters.Add("equity", 100000d);

            starter.SetParameters(parameters);
            starter.Initialize();
            starter.Run();

            return security;
        }

        [TestMethod()]
        public void NoEntry_WhenCloseBelowSma_Long()
        {
            //Постоянное падение: RSI низкий (условие входа выполнено), но закрытие
            //всегда ниже SMA — фильтр тренда должен запретить вход.
            var bars = new List<Bar>();
            for (var i = 0; i <= 25; i++)
                AddBar(bars, 141 - 2 * i, 142 - 2 * i, 139 - 2 * i, 140 - 2 * i);

            var security = Run(bars, positionSide: 0);

            Assert.AreEqual(0, security.GetDeals().Count);
            Assert.IsNull(security.GetLastActiveForSignal("LE Вход №1", bars.Count - 1));
        }

        [TestMethod()]
        public void OpensLong_OnRsiDipInUptrend()
        {
            var bars = CreateUptrendWithDip();
            //Гладкие бары после сигнала: позиция должна открыться и остаться открытой.
            AddBar(bars, 131, 132, 129, 130);
            AddBar(bars, 129, 130, 127, 128);
            AddBar(bars, 127, 128, 125, 126);

            var security = Run(bars, positionSide: 0);

            var position = security.GetLastActiveForSignal("LE Вход №1", bars.Count - 1);
            Assert.IsNotNull(position);
            Assert.AreEqual(PositionSide.Long, position.PositionSide);
            Assert.AreEqual(22, position.BarNumberOpenPosition);
            Assert.AreEqual(131, position.EntryPrice);    //открытие бара № 22
        }

        [TestMethod()]
        public void ClosesLong_ByAtrStop()
        {
            var bars = CreateUptrendWithDip();
            AddBar(bars, 131, 132, 129, 130);   //бар № 22: вход по рынку, стоп выставлен
            AddBar(bars, 105, 106, 95, 96);     //бар № 23: обвал с гэпом сквозь стоп
            AddBar(bars, 96, 97, 94, 95);

            var security = Run(bars, positionSide: 0);

            var deals = security.GetDeals();
            Assert.AreEqual(1, deals.Count);
            var deal = deals[0];
            Assert.AreEqual(23, deal.BarNumberClosePosition);
            Assert.AreEqual(105, deal.ExitPrice);    //гэп: исполнение по открытию бара
            StringAssert.StartsWith(deal.SignalNameForClosePosition, "LXS Выход №1");
            Assert.IsFalse(deal.SignalNameForClosePosition.Contains("RSI"));
        }

        [TestMethod()]
        public void ClosesLong_ByRsiCross()
        {
            var bars = CreateUptrendWithDip();
            AddBar(bars, 132, 152, 131, 151);   //бар № 22: вход и резкий рост — RSI
                                                //пересекает порог выхода снизу вверх
            AddBar(bars, 151, 152, 149, 150);   //бар № 23: выход по рынку на открытии
            AddBar(bars, 150, 151, 148, 149);

            var security = Run(bars, positionSide: 0);

            var deals = security.GetDeals();
            Assert.AreEqual(1, deals.Count);
            var deal = deals[0];
            Assert.AreEqual(22, deal.BarNumberOpenPosition);
            Assert.AreEqual(132, deal.EntryPrice);
            Assert.AreEqual(23, deal.BarNumberClosePosition);
            Assert.AreEqual(151, deal.ExitPrice);
            Assert.AreEqual("LXS Выход №1 RSI", deal.SignalNameForClosePosition);
        }

        [TestMethod()]
        public void OpensShort_OnRsiSpikeInDowntrend()
        {
            var bars = CreateDowntrendWithSpike();
            //Вход в шорт: RSI выше порога 50 после выброса, закрытие ниже SMA.
            //Слабый дрейф вверх: RSI остаётся высоким, пересечения порога выхода
            //сверху вниз нет — шорт не закрывается.
            AddBar(bars, 108, 110, 107, 109);
            AddBar(bars, 109, 111, 108, 110);
            AddBar(bars, 110, 112, 109, 111);

            var security = Run(bars, positionSide: 1);

            var position = security.GetLastActiveForSignal("SE Вход №1", bars.Count - 1);
            Assert.IsNotNull(position);
            Assert.AreEqual(PositionSide.Short, position.PositionSide);
            Assert.AreEqual(22, position.BarNumberOpenPosition);
            Assert.AreEqual(108, position.EntryPrice);
        }

        [TestMethod()]
        public void TrailingStop_ClosesPositionOnPullback_FixedStopDoesNot()
        {
            //Вход, рост с мелкими откатами (RSI не достигает порога выхода 95),
            //затем откат: трейлинг-стоп подтянулся за ценой и закрывает позицию,
            //а фиксированный стоп остаётся далеко и не срабатывает.
            var bars = CreateUptrendWithDip();
            AddBar(bars, 131, 132, 129, 130);   //бар № 22: вход
            AddBar(bars, 131, 142, 130, 141);   //рост
            AddBar(bars, 141, 142, 139, 140);   //мелкий откат — RSI остаётся < 95
            AddBar(bars, 140, 147, 139, 146);   //рост
            AddBar(bars, 148, 149, 138, 139);   //бар № 26: откат до 138
            AddBar(bars, 139, 152, 139, 151);   //рост: повторного входа по бару нет

            var lastBar = bars.Count - 1;

            var withoutTrailing = Run(bars, positionSide: 0, atrMultiplier: 0.5,
                useTrailingStop: 0, rsiExitLevel: 95);
            var withTrailing = Run(bars, positionSide: 0, atrMultiplier: 0.5,
                useTrailingStop: 1, rsiExitLevel: 95);

            //Без трейлинга исходная позиция жива: фиксированный стоп не тронут откатом.
            //GetDeals включает и активные позиции, поэтому единственная «сделка» —
            //это открытая позиция.
            var position = withoutTrailing.GetLastActiveForSignal("LE Вход №1", lastBar);
            Assert.IsNotNull(position);
            Assert.AreEqual(22, position.BarNumberOpenPosition);
            Assert.AreEqual(1, withoutTrailing.GetDeals().Count);
            Assert.AreEqual(int.MaxValue, withoutTrailing.GetDeals()[0].BarNumberClosePosition);

            //С трейлингом позиция, открытая на баре № 22, закрыта на откате (бар № 26).
            var deals = withTrailing.GetDeals();
            var closedDeal = deals.Find(d => d.BarNumberOpenPosition == 22);
            Assert.IsNotNull(closedDeal);
            Assert.AreEqual(26, closedDeal.BarNumberClosePosition);
        }

        //Бары теста ClosesLong_ByRsiCross: вход на баре № 22 и резкий рост, на
        //котором RSI пересекает порог выхода 50 снизу вверх.
        private List<Bar> CreateEntryThenRsiSpike()
        {
            var bars = CreateUptrendWithDip();
            AddBar(bars, 132, 152, 131, 151);
            AddBar(bars, 151, 152, 149, 150);
            AddBar(bars, 150, 151, 148, 149);
            return bars;
        }

        [TestMethod()]
        public void ExitModeOff_KeepsPositionOpenWhenRsiCrossesExitLevel()
        {
            //Режим «выход по RSI выключен»: закрывать позицию может только стоп.
            //На золоте в лонг оптимизатор добивался того же самого окольным путём —
            //задирал порог выхода в недостижимую зону.
            var bars = CreateEntryThenRsiSpike();

            var security = Run(bars, positionSide: 0,
                rsiExitMode: TradingSystemMeanReversion.ExitModeOff);

            var position = security.GetLastActiveForSignal("LE Вход №1", bars.Count - 1);
            Assert.IsNotNull(position, "Выход по RSI выключен — позиция должна остаться открытой.");
            Assert.AreEqual(22, position.BarNumberOpenPosition);
            Assert.AreEqual(int.MaxValue, position.BarNumberClosePosition);
        }

        [TestMethod()]
        public void ExitModeTargetReached_IsTheDefaultBehaviour()
        {
            //Тот же набор баров без указания режима и с явным режимом 1 должен
            //давать один и тот же результат.
            var bars = CreateEntryThenRsiSpike();

            var byDefault = Run(bars, positionSide: 0);
            var explicitMode = Run(bars, positionSide: 0,
                rsiExitMode: TradingSystemMeanReversion.ExitModeTargetReached);

            var dealByDefault = byDefault.GetDeals()[0];
            var dealExplicit = explicitMode.GetDeals()[0];

            Assert.AreEqual(dealByDefault.BarNumberClosePosition,
                dealExplicit.BarNumberClosePosition);
            Assert.AreEqual(23, dealExplicit.BarNumberClosePosition);
            Assert.AreEqual("LXS Выход №1 RSI", dealExplicit.SignalNameForClosePosition);
        }

        [TestMethod()]
        public void ExitModeLevel_ClosesPositionWithoutRequiringCrossing()
        {
            //Режим «уровень» не требует пересечения: единственный, которому
            //безразлично, где RSI находился в момент входа.
            var bars = CreateEntryThenRsiSpike();

            var security = Run(bars, positionSide: 0,
                rsiExitMode: TradingSystemMeanReversion.ExitModeLevel);

            var deals = security.GetDeals();
            Assert.AreEqual(1, deals.Count);
            Assert.AreEqual(23, deals[0].BarNumberClosePosition);
            Assert.AreEqual("LXS Выход №1 RSI", deals[0].SignalNameForClosePosition);
        }

        [TestMethod()]
        public void EntryModeReversal_WaitsForRsiToTurnUp()
        {
            //Провал на баре № 21 роняет RSI(2) ниже порога 50. Режим «уровень»
            //входит сразу, режим «разворот» ждёт возврата RSI выше порога, который
            //случается на следующем баре.
            var bars = CreateUptrendWithDip();
            AddBar(bars, 132, 141, 131, 140);   //бар № 22: отскок, RSI снова выше 50
            AddBar(bars, 140, 142, 139, 141);
            AddBar(bars, 141, 143, 140, 142);

            var byLevel = Run(bars, positionSide: 0, rsiExitLevel: 95,
                rsiEntryMode: TradingSystemMeanReversion.EntryModeLevel);
            var byReversal = Run(bars, positionSide: 0, rsiExitLevel: 95,
                rsiEntryMode: TradingSystemMeanReversion.EntryModeReversal);

            var positionByLevel = byLevel.GetDeals()[0];
            var positionByReversal = byReversal.GetDeals()[0];

            Assert.AreEqual(22, positionByLevel.BarNumberOpenPosition);
            Assert.AreEqual(23, positionByReversal.BarNumberOpenPosition,
                "Разворот подтверждается на бар позже, чем срабатывает уровень.");
        }

        [TestMethod()]
        public void EntryModeEnterZone_EntersOnTheDipBar()
        {
            //Режим «вход в зону»: пересечение порога сверху вниз. Провал на баре
            //№ 21 — как раз такое пересечение, вход на открытии бара № 22.
            var bars = CreateUptrendWithDip();
            AddBar(bars, 131, 132, 129, 130);
            AddBar(bars, 129, 130, 127, 128);
            AddBar(bars, 127, 128, 125, 126);

            var security = Run(bars, positionSide: 0,
                rsiEntryMode: TradingSystemMeanReversion.EntryModeEnterZone);

            var position = security.GetLastActiveForSignal("LE Вход №1", bars.Count - 1);
            Assert.IsNotNull(position);
            Assert.AreEqual(22, position.BarNumberOpenPosition);
            Assert.AreEqual(131, position.EntryPrice);
        }
    }
}
