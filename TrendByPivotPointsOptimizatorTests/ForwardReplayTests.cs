using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class ForwardReplayTests
    {
        private const string SummaryHeader = "BackwardFitness;ForwardFitness;" +
            "BackwardProfit;ForwardProfit;BackwardProfitPrcnt;ForwardProfitPrcnt;" +
            "BackwardTestDates;ForwardTestDates;";

        private static string WriteTempFile(IEnumerable<string> lines)
        {
            var fileName = Path.GetTempFileName();
            File.WriteAllLines(fileName, lines, Encoding.UTF8);
            return fileName;
        }

        private static string SummaryRow(string forwardStart, string forwardEnd)
        {
            return "7,22;0,5;100;10;10;1;01.01.2010 23:00:00-01.01.2014 23:00:00;" +
                forwardStart + " - " + forwardEnd;
        }

        [TestMethod()]
        public void ReadForwardWindows_ReadsDatesInPeriodOrder()
        {
            var fileName = WriteTempFile(new[]
            {
                SummaryHeader,
                SummaryRow("07.09.2019 23:00:00", "05.12.2019 23:00:00"),
                SummaryRow("09.06.2019 23:00:00", "06.09.2019 23:00:00"),
            });

            var windows = ForwardReplay.ReadForwardWindows(fileName, 2);

            Assert.AreEqual(2, windows.Count);
            Assert.AreEqual(new DateTime(2019, 9, 7, 23, 0, 0), windows[0].Start);
            Assert.AreEqual(new DateTime(2019, 12, 5, 23, 0, 0), windows[0].End);
            Assert.AreEqual(new DateTime(2019, 6, 9, 23, 0, 0), windows[1].Start);
        }

        /// <summary>
        /// Последняя строка отчёта описывает итоговый бэктест по всей истории:
        /// форвардного окна у неё нет, и за окно периода её принимать нельзя.
        /// </summary>
        [TestMethod()]
        public void ReadForwardWindows_SkipsFinalBacktestRow()
        {
            var fileName = WriteTempFile(new[]
            {
                SummaryHeader,
                SummaryRow("07.09.2019 23:00:00", "05.12.2019 23:00:00"),
                SummaryRow("01.01.0001 0:00:00", "01.01.0001 0:00:00"),
            });

            var windows = ForwardReplay.ReadForwardWindows(fileName, 1);

            Assert.AreEqual(1, windows.Count);
            Assert.AreEqual(new DateTime(2019, 9, 7, 23, 0, 0), windows[0].Start);
        }

        [TestMethod()]
        public void ReadForwardWindows_ThrowsWhenPeriodsMissing()
        {
            var fileName = WriteTempFile(new[]
            {
                SummaryHeader,
                SummaryRow("07.09.2019 23:00:00", "05.12.2019 23:00:00"),
            });

            Assert.ThrowsException<Exception>(
                () => ForwardReplay.ReadForwardWindows(fileName, 3));
        }

        [TestMethod()]
        public void EnsureWindowMatchesReport_PassesWhenDatesEqual()
        {
            var report = new Dictionary<int, ForwardReplay.ReportWindow>()
            {
                [0] = new ForwardReplay.ReportWindow()
                {
                    Start = new DateTime(2019, 9, 7, 23, 0, 0),
                    End = new DateTime(2019, 12, 5, 23, 0, 0),
                },
            };

            ForwardReplay.EnsureWindowMatchesReport(0,
                new DateTime(2019, 9, 7, 23, 0, 0),
                new DateTime(2019, 12, 5, 23, 0, 0), report);
        }

        /// <summary>
        /// Дописали месяц котировок — окна отсчитываются от последнего бара и все
        /// уехали. Гены при этом остались от старого прогона, поэтому повтор обязан
        /// остановиться, а не считать их на других отрезках.
        /// </summary>
        [TestMethod()]
        public void EnsureWindowMatchesReport_ThrowsWhenWindowShifted()
        {
            var report = new Dictionary<int, ForwardReplay.ReportWindow>()
            {
                [0] = new ForwardReplay.ReportWindow()
                {
                    Start = new DateTime(2019, 9, 7, 23, 0, 0),
                    End = new DateTime(2019, 12, 5, 23, 0, 0),
                },
            };

            Assert.ThrowsException<Exception>(
                () => ForwardReplay.EnsureWindowMatchesReport(0,
                    new DateTime(2019, 10, 8, 23, 0, 0),
                    new DateTime(2020, 1, 5, 23, 0, 0), report));
        }

        [TestMethod()]
        public void EnsureWindowMatchesReport_ThrowsWhenPeriodAbsent()
        {
            var report = new Dictionary<int, ForwardReplay.ReportWindow>();

            Assert.ThrowsException<Exception>(
                () => ForwardReplay.EnsureWindowMatchesReport(0, DateTime.Now,
                    DateTime.Now, report));
        }

        [TestMethod()]
        public void ReadGenes_TakesValuesByColumnName()
        {
            var definition = new DonchianStrategyDefinition();
            var names = new List<string>();
            foreach (var descriptor in definition.Parameters)
                names.Add(descriptor.Name);

            //Перед генами в отчёте идут служебные колонки, а дробная часть
            //отделена запятой — так его пишет оптимизатор.
            var header = "TimeFrame;Side;Name;" + string.Join(";", names);
            var values = "60M;Short;Si;10;80;20;1;2,5;0,5;1;701;1";

            var fileName = WriteTempFile(new[] { header, values });
            var genes = ForwardReplay.ReadGenes(fileName, definition);

            Assert.AreEqual(10, genes["fastDonchian"]);
            Assert.AreEqual(80, genes["slowDonchian"]);
            Assert.AreEqual(20, genes["atrPeriod"]);
            Assert.AreEqual(2.5, genes["kAtrForOpenPosition"]);
            Assert.AreEqual(0.5, genes["kAtrForStopLoss"]);
            Assert.AreEqual(701, genes["maxBarsInPosition"]);
        }

        [TestMethod()]
        public void ReadGenes_ThrowsWhenGeneColumnMissing()
        {
            var fileName = WriteTempFile(new[]
            {
                "TimeFrame;Side;Name;fastDonchian",
                "60M;Short;Si;10",
            });

            Assert.ThrowsException<Exception>(
                () => ForwardReplay.ReadGenes(fileName, new DonchianStrategyDefinition()));
        }

        [TestMethod()]
        public void AppendWindow_MeasuresDrawdownFromRunningPeak()
        {
            var curve = new List<ForwardReplay.CurvePoint>();
            var peak = double.MinValue;
            var bars = new List<TradingSystems.Bar>();
            var equities = new[] { 100d, 120d, 90d, 110d };

            for (var i = 0; i < equities.Length; i++)
                bars.Add(new TradingSystems.Bar() { Date = new DateTime(2020, 1, 1).AddHours(i) });

            ForwardReplay.AppendWindow(curve, new AccountStub(equities), bars, 7, ref peak);

            Assert.AreEqual(4, curve.Count);
            Assert.AreEqual(7, curve[0].Period);
            Assert.AreEqual(0, curve[1].DrawdownPrcnt, 1e-9);
            Assert.AreEqual(25, curve[2].DrawdownPrcnt, 1e-9);
            Assert.AreEqual(120, peak, 1e-9);
        }

        private class AccountStub : TradingSystems.Account
        {
            private readonly double[] equities;

            public AccountStub(double[] equities)
            {
                this.equities = equities;
            }

            public override double GetEquity(int barNumber)
            {
                return equities[barNumber];
            }

            public override double InitDeposit
            {
                get { return equities[0]; }
            }

            public override double Equity
            {
                get { return equities[equities.Length - 1]; }
            }

            public override double FreeBalance
            {
                get { return Equity; }
            }

            public override double GetMaxDrawDownPrcnt()
            {
                throw new NotImplementedException();
            }
        }
    }
}
