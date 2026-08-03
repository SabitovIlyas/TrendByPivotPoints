using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using TradingSystems;

namespace TrendByPivotPointsOptimizator.Tests
{
    [TestClass()]
    public class ReportWritingTests
    {
        //Журнал, который запоминает строки, чтобы проверить сообщение об ошибке.
        private class LoggerMemory : Logger
        {
            public readonly List<string> Lines = new List<string>();

            public override void Log(string text) => Lines.Add(text);
            public override void Log(string text, params object[] args) =>
                Lines.Add(string.Format(text, args));
        }

        private List<ForwardAnalysisResult> CreateResults()
        {
            return new List<ForwardAnalysisResult>()
            {
                new ForwardAnalysisResult()
                {
                    BackwardFitness = 10.88,
                    ForwardFitness = 0.38,
                    BackwardProfit = 2822561,
                    ForwardProfit = 21435,
                },
                new ForwardAnalysisResult()
                {
                    BackwardFitness = 13.99,
                    ForwardFitness = 1.03,
                },
            };
        }

        [TestMethod()]
        public void SummaryReport_SurvivesFileOpenedByAnotherProgram()
        {
            //Ровно тот случай, который сорвал боевой прогон: пользователь открыл
            //отчёт в Excel, чтобы посмотреть промежуточный результат.
            var fileName = Path.GetTempFileName();
            var starter = new OptimizatorGeneticAlgorithmStarter();
            var logger = new LoggerMemory();

            try
            {
                using (new FileStream(fileName, FileMode.Open, FileAccess.Read,
                    FileShare.Read))
                {
                    starter.WriteSummaryReport(CreateResults(), fileName, logger);
                }

                Assert.IsTrue(logger.Lines.Exists(l => l.Contains("Не удалось записать отчёт")),
                    "О неудачной записи надо сообщить в журнал.");
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod()]
        public void SummaryReport_RestoresMissedRowsOnNextWrite()
        {
            //Отчёт переписывается целиком, поэтому строка, которую не удалось
            //записать, появится при следующей успешной записи.
            var fileName = Path.GetTempFileName();
            var starter = new OptimizatorGeneticAlgorithmStarter();
            var logger = new LoggerMemory();
            var results = CreateResults();

            try
            {
                using (new FileStream(fileName, FileMode.Open, FileAccess.Read,
                    FileShare.Read))
                {
                    starter.WriteSummaryReport(results, fileName, logger);
                }

                //Файл освободился — пишем следующий период.
                results.Add(new ForwardAnalysisResult() { ForwardFitness = 4.09 });
                starter.WriteSummaryReport(results, fileName, logger);

                var lines = File.ReadAllLines(fileName);
                Assert.AreEqual(4, lines.Length, "Заголовок и три периода.");
                Assert.IsTrue(lines[1].StartsWith("10,88;0,38") ||
                    lines[1].StartsWith("10.88;0.38"),
                    "Потеряна строка, записанная до сбоя: " + lines[1]);
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod()]
        public void SummaryReport_WritesUtf8WithMark()
        {
            //Без метки кодировки Excel на русской Windows читает файл как ANSI.
            var fileName = Path.GetTempFileName();
            var starter = new OptimizatorGeneticAlgorithmStarter();

            try
            {
                starter.WriteSummaryReport(CreateResults(), fileName, new LoggerMemory());

                var bytes = File.ReadAllBytes(fileName);
                Assert.IsTrue(bytes.Length > 3 && bytes[0] == 0xEF && bytes[1] == 0xBB &&
                    bytes[2] == 0xBF, "В начале файла нет метки UTF-8.");
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
