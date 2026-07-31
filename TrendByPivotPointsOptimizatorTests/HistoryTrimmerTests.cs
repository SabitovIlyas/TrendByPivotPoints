using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class HistoryTrimmerTests
    {
        //Бары подряд с заданным шагом в днях.
        private List<Bar> CreateBars(int count, int stepDays = 1)
        {
            var bars = new List<Bar>();
            var firstDate = new DateTime(2010, 1, 1);

            for (var i = 0; i < count; i++)
                bars.Add(Bar.Create(firstDate.AddDays(i * stepDays), open: 100, high: 101,
                    low: 99, close: 100));

            return bars;
        }

        private Settings CreateSettings()
        {
            return new Settings()
            {
                BackwardDays = 100,
                ForwardDays = 50,
                ForwardPeriodsCount = 3,
                ShiftWindowDays = 10,
            };
        }

        [TestMethod()]
        public void GetRequiredDays_CountsWindowsAndShifts()
        {
            //100 бэктест + 50 форвард + 10 сдвиг × 2 дополнительных периода
            Assert.AreEqual(170, HistoryTrimmer.GetRequiredDays(CreateSettings()));
        }

        [TestMethod()]
        public void Trim_KeepsOnlyBarsNeededForTesting()
        {
            var bars = CreateBars(1000);
            var settings = CreateSettings();

            var trimmed = HistoryTrimmer.Trim(bars, settings);

            //Дневные бары: 170 нужных дней — 170 баров, начиная с начала самого
            //раннего окна бэктеста.
            Assert.AreEqual(170, trimmed.Count);
            Assert.AreEqual(bars.Last().Date, trimmed.Last().Date);
        }

        [TestMethod()]
        public void Trim_KeepsBarAtOrBeforeEarliestWindowStart()
        {
            //Шаг в 3 дня: начало окна почти наверняка приходится на пропуск в истории.
            var bars = CreateBars(1000, stepDays: 3);
            var settings = CreateSettings();

            var earliestRequiredDate = HistoryTrimmer.GetEarliestRequiredDate(bars, settings);
            var trimmed = HistoryTrimmer.Trim(bars, settings);

            //ForwardAnalysis считает историю недостаточной, если первый бар оказался
            //позже начала окна, — граничный бар обязан остаться.
            Assert.IsTrue(trimmed.First().Date <= earliestRequiredDate);

            //И это именно ближайший бар слева: ничего лишнего не оставили.
            var newestBarBeforeWindows = bars.Where(b => b.Date <= earliestRequiredDate)
                .Max(b => b.Date);
            Assert.AreEqual(newestBarBeforeWindows, trimmed.First().Date);
        }

        [TestMethod()]
        public void GetReadFromDate_StepsBackBeyondWindowStart()
        {
            var settings = CreateSettings();
            var lastBarDate = new DateTime(2026, 5, 29, 23, 0, 0);

            var earliestRequiredDate = HistoryTrimmer.GetEarliestRequiredDate(lastBarDate,
                settings);
            var readFromDate = HistoryTrimmer.GetReadFromDate(lastBarDate, settings);

            //Читаем с запасом и с полуночи: начало окна может попасть на выходные или
            //длинные праздники, а резать сутки посередине нельзя из-за сжатия баров.
            Assert.IsTrue(readFromDate < earliestRequiredDate);
            Assert.AreEqual(readFromDate.Date, readFromDate);
            Assert.AreEqual(earliestRequiredDate.AddDays(-HistoryTrimmer.ExtraDaysToRead).Date,
                readFromDate);
        }

        [TestMethod()]
        public void Trim_KeepsAllBarsWhenNothingIsLeftOfWindowStart()
        {
            //История начинается уже после начала самого раннего окна: обрезать нечего,
            //иначе ForwardAnalysis сочтёт, что данных не хватает.
            var bars = CreateBars(1000);
            var settings = CreateSettings();
            var earliestRequiredDate = HistoryTrimmer.GetEarliestRequiredDate(bars, settings);
            var barsAfterWindowStart = bars.Where(b => b.Date > earliestRequiredDate).ToList();

            var trimmed = HistoryTrimmer.Trim(barsAfterWindowStart, settings);

            Assert.AreEqual(barsAfterWindowStart.Count, trimmed.Count);
        }

        [TestMethod()]
        public void Trim_KeepsAllBarsWhenHistoryIsShorterThanWindows()
        {
            var bars = CreateBars(50);
            var settings = CreateSettings();

            var trimmed = HistoryTrimmer.Trim(bars, settings);

            Assert.AreEqual(bars.Count, trimmed.Count);
        }

        [TestMethod()]
        public void Trim_DoesNotChangeBarsInsideTestingWindows()
        {
            var bars = CreateBars(1000);
            var settings = CreateSettings();

            var trimmed = HistoryTrimmer.Trim(bars, settings);

            //Обрезка не должна менять сами бары: хвост истории совпадает бар в бар.
            var tail = bars.Skip(bars.Count - trimmed.Count).ToList();
            for (var i = 0; i < trimmed.Count; i++)
                Assert.AreSame(tail[i], trimmed[i]);
        }
    }
}
