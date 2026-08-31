using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TrendByPivotPointsStarter;

namespace TradingSystems.Tests
{
    /// <summary>
    /// Размер позиции задаётся риском на сделку из настроек.
    ///
    /// Раньше лабораторный стартер затирал этот параметр множителем стопа
    /// (riskValuePrcnt = kAtrForStopLoss). Последствий было три: настройка не
    /// работала вовсе; один ген задавал сразу ширину стопа и размер позиции,
    /// причём они взаимно сокращались и позиция получалась одинаковой при любом
    /// стопе; деление риска между уровнями пирамиды не срабатывало.
    ///
    /// Цены здесь порядка восьмидесяти тысяч, как у реального Si: при мелких
    /// ценах количество контрактов упирается в гарантийное обеспечение, а не в
    /// риск, и проверять было бы нечего.
    /// </summary>
    [TestClass()]
    public class PositionSizingTests
    {
        private const double Scale = 700;

        private void AddBar(List<Bar> bars, double open, double high, double low, double close)
        {
            var dateTime = new DateTime(2026, 1, 5, 10, 0, 0).AddHours(bars.Count);
            bars.Add(Bar.Create(dateTime, open * Scale, high * Scale, low * Scale,
                close * Scale, 1, "SPFB.TEST", "60", 0));
        }

        /// <summary>Плавное снижение, затем пробой верхней границы канала.</summary>
        private List<Bar> CreateBreakout()
        {
            var bars = new List<Bar>();
            for (var i = 0; i < 12; i++)
                AddBar(bars, 110 - i, 111 - i, 109 - i, 110 - i);

            AddBar(bars, 100, 112, 100, 111);

            for (var i = 0; i < 12; i++)
                AddBar(bars, 111 + i, 113 + i, 111 + i, 112 + i);

            return bars;
        }

        private int ContractsOfFirstDeal(double riskValuePrcnt, double kAtrForStopLoss)
        {
            var logger = new LoggerNull();
            var bars = CreateBreakout();
            var security = new SecurityLab(Currency.RUB, shares: 1, bars, logger,
                commissionRate: 0);

            var starter = new StarterDonchianTradingSystemLab(new ContextLab(),
                new List<Security>() { security }, logger);

            var parameters = new SystemParameters();
            parameters.Add("slowDonchian", 10);
            parameters.Add("fastDonchian", 5);
            parameters.Add("kAtrForStopLoss", kAtrForStopLoss);
            parameters.Add("kAtrForOpenPosition", 0.5d);
            parameters.Add("atrPeriod", 10);
            parameters.Add("limitOpenedPositions", 1);
            parameters.Add("maxBarsInPosition", 0);
            //Канальный выход выключен: иначе стоп берётся как максимум из
            //ATR-стопа и границы канала, и ширина стопа перестаёт зависеть от k.
            parameters.Add("useChannelExit", 0);
            parameters.Add("useTimeExit", 0);
            parameters.Add("positionSide", 0);
            parameters.Add("isUSD", 0);
            parameters.Add("rateUSD", 0d);
            parameters.Add("shares", 1d);
            parameters.Add("equity", 10000000d);
            parameters.Add("riskValuePrcnt", riskValuePrcnt);
            //Ноль — значит считать количество по риску, а не брать готовое.
            parameters.Add("contracts", 0);

            starter.SetParameters(parameters);
            starter.Initialize();
            starter.Run();

            var deals = security.GetDeals();
            Assert.IsTrue(deals.Count > 0, "Позиция не открылась — проверять нечего.");
            return deals[0].Contracts;
        }

        [TestMethod()]
        public void UdvoenieRiskaUdvaivaetPoziciyu()
        {
            var one = ContractsOfFirstDeal(riskValuePrcnt: 1, kAtrForStopLoss: 2);
            var two = ContractsOfFirstDeal(riskValuePrcnt: 2, kAtrForStopLoss: 2);

            Assert.IsTrue(one > 1, "Позиция слишком мала, проверка вырождается.");
            Assert.AreEqual(2.0, (double)two / one, 0.05,
                "Удвоение риска на сделку должно удваивать количество контрактов.");
        }

        [TestMethod()]
        public void ShirokiyStopUmenshaetPoziciyu()
        {
            //Ключевая проверка: раньше позиция была одинаковой при любом стопе,
            //потому что риск равнялся множителю и k сокращалось.
            var narrow = ContractsOfFirstDeal(riskValuePrcnt: 2, kAtrForStopLoss: 1);
            var wide = ContractsOfFirstDeal(riskValuePrcnt: 2, kAtrForStopLoss: 2);

            Assert.IsTrue(wide < narrow,
                "Вдвое более широкий стоп при том же риске должен уменьшать позицию.");
            Assert.AreEqual(2.0, (double)narrow / wide, 0.1,
                "Вдвое более широкий стоп должен вдвое уменьшать количество контрактов.");
        }

        [TestMethod()]
        public void RiskNeZavisitOtMnozhitelyaStopa()
        {
            //Прямая проверка того, что настройка больше не затирается: при
            //одинаковом риске и разных множителях стопа рискуемая сумма совпадает.
            var narrow = ContractsOfFirstDeal(riskValuePrcnt: 2, kAtrForStopLoss: 1);
            var wide = ContractsOfFirstDeal(riskValuePrcnt: 2, kAtrForStopLoss: 3);

            //Риск в деньгах = контракты x ширина стопа. Ширина пропорциональна k,
            //значит произведение должно сохраняться.
            var riskNarrow = narrow * 1.0;
            var riskWide = wide * 3.0;

            Assert.AreEqual(1.0, riskNarrow / riskWide, 0.1,
                "Рискуемая сумма должна определяться настройкой, а не множителем стопа.");
        }
    }
}
