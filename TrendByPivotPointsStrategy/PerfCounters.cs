using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace TradingSystems
{
    /// <summary>
    /// Счётчики времени для поиска узких мест. Включаются переменной окружения
    /// PERF=1; выключенные стоят одну проверку булева поля. Отчёт пишется рядом
    /// с исполняемым файлом при завершении процесса.
    ///
    /// Это диагностика, а не часть торговой логики: на результат прогона счётчики
    /// не влияют и в боевых запусках выключены.
    /// </summary>
    public static class PerfCounters
    {
        public const int SecurityUpdate = 0;
        public const int MappingUpdate = 1;
        public const int GetProfitTotal = 2;
        public const int GetProfitClosed = 3;
        public const int GetProfitActive = 4;
        public const int MappingSnapshots = 5;
        public const int GetActiveOrders = 6;
        public const int MaxDrawDown = 7;
        public const int AccountUpdate = 8;
        public const int GetLastClosedPosition = 9;
        public const int TradingSystemUpdate = 10;
        private const int Count = 11;

        private static readonly string[] names =
        {
            "SecurityLab.Update (весь бар)",
            "  OrderToPositionMapping.Update",
            "  SecurityLab.GetProfit (весь)",
            "    закрытые позиции",
            "    активные позиции",
            "  снимки списков по барам",
            "GetActiveOrders (полный перебор maps)",
            "AccountLab.GetMaxDrawDownPrcnt",
            "Account.Update (весь бар)",
            "  GetLastClosedPosition",
            "TradingSystem.Update (весь бар)",
        };

        private static readonly long[] ticks = new long[Count];
        private static readonly long[] calls = new long[Count];

        public static readonly bool Enabled =
            Environment.GetEnvironmentVariable("PERF") == "1";

        static PerfCounters()
        {
            if (!Enabled)
                return;

            AppDomain.CurrentDomain.ProcessExit += (s, e) => WriteReport();
        }

        /// <summary>Засекает время до вызова Stop; при выключенных счётчиках — ноль.</summary>
        public static long Start()
        {
            return Enabled ? Stopwatch.GetTimestamp() : 0;
        }

        public static void Stop(int counter, long startedAt)
        {
            if (!Enabled)
                return;

            Interlocked.Add(ref ticks[counter], Stopwatch.GetTimestamp() - startedAt);
            Interlocked.Increment(ref calls[counter]);
        }

        public static void WriteReport()
        {
            if (!Enabled)
                return;

            var builder = new StringBuilder();
            builder.AppendLine("Счётчики времени (сумма по всем потокам):");
            builder.AppendLine();
            builder.AppendFormat("{0,-40} {1,12} {2,14}", "участок", "секунд", "вызовов");
            builder.AppendLine();

            for (var i = 0; i < Count; i++)
            {
                var seconds = (double)ticks[i] / Stopwatch.Frequency;
                builder.AppendFormat("{0,-40} {1,12:N2} {2,14:N0}", names[i], seconds, calls[i]);
                builder.AppendLine();
            }

            var fileName = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "perf_report.txt");
            System.IO.File.WriteAllText(fileName, builder.ToString(), Encoding.UTF8);
        }
    }
}
