using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Повторный прогон форвардных окон с уже известными генами — без генетики.
    ///
    /// Сводный отчёт оптимизатора даёт процент прибыли и просадку по каждому окну
    /// отдельно, и каждое окно там начинается с одного и того же депозита. Сквозной
    /// просадки по таким числам не увидеть: они не говорят, на какую глубину уходил
    /// счёт, который торговал все сорок кварталов подряд. Этот режим прогоняет те же
    /// окна теми же генами и склеивает кривую капитала в одну.
    ///
    /// Генетика здесь не работает: гены берутся из отчётов уже посчитанного прогона
    /// (файлы *_Period_N.csv, первая строка данных — лучшая хромосома периода).
    /// Поэтому режим считает минуты там, где сам прогон считал дни.
    /// </summary>
    public static class ForwardReplay
    {
        /// <summary>Строка склеенной кривой: бар, капитал и просадка от достигнутого максимума.</summary>
        public class CurvePoint
        {
            public DateTime Date;
            public double Equity;
            public double DrawdownPrcnt;
            public int Period;
        }

        /// <summary>Итог одного окна — для сверки с отчётом исходного прогона.</summary>
        public class WindowResult
        {
            public int Period;
            public DateTime Start;
            public DateTime End;
            public double StartEquity;
            public double EndEquity;
            public double ProfitPrcnt;
            public double MaxDrawDownPrcnt;
            public int DealsCount;
        }

        /// <summary>
        /// Читает гены лучшей хромосомы периода из отчёта *_Period_N.csv.
        /// Колонки ищутся по именам в заголовке, а не по номерам: порядок колонок
        /// задаётся описанием стратегии и может измениться.
        /// </summary>
        public static Dictionary<string, double> ReadGenes(string fileName,
            StrategyDefinition definition)
        {
            var lines = File.ReadAllLines(fileName, Encoding.UTF8);
            if (lines.Length < 2)
                throw new Exception("В файле " + fileName + " нет строки с хромосомой.");

            var header = SplitLine(lines[0]);
            var values = SplitLine(lines[1]);

            var genes = new Dictionary<string, double>();
            foreach (var descriptor in definition.Parameters)
            {
                var column = Array.FindIndex(header, h => h == descriptor.Name);
                if (column < 0)
                    throw new Exception("В файле " + fileName + " нет колонки гена " +
                        descriptor.Name + ".");

                if (column >= values.Length)
                    throw new Exception("В файле " + fileName + " строка хромосомы " +
                        "короче заголовка: нет значения гена " + descriptor.Name + ".");

                genes[descriptor.Name] = ParseNumber(values[column], descriptor.Name, fileName);
            }

            return genes;
        }

        private static string[] SplitLine(string line)
        {
            //Первая строка файла может начинаться с метки кодировки — иначе имя
            //первой колонки не совпадёт ни с одним геном.
            var cleaned = line.TrimStart('﻿');
            var parts = cleaned.Split(';');
            for (var i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();

            return parts;
        }

        /// <summary>
        /// Разбирает число из отчёта. Отчёт пишется в текущей культуре машины, где
        /// дробная часть отделена запятой, поэтому разделитель приводим к точке и
        /// читаем в инвариантной культуре — так файл читается одинаково на любой
        /// машине.
        /// </summary>
        private static double ParseNumber(string text, string geneName, string fileName)
        {
            var normalized = text.Replace(',', '.');
            double value;
            if (!double.TryParse(normalized, NumberStyles.Float,
                CultureInfo.InvariantCulture, out value))
                throw new Exception("В файле " + fileName + " значение гена " +
                    geneName + " не разобрать как число: «" + text + "».");

            return value;
        }

        /// <summary>
        /// Собирает точки склеенной кривой из капитала по барам одного окна.
        /// Просадка считается от максимума всей склеенной кривой, а не окна: в этом
        /// весь смысл склейки — падение внутри квартала меряется от вершины, которая
        /// могла быть достигнута кварталом раньше.
        /// </summary>
        public static void AppendWindow(List<CurvePoint> curve, Account account,
            List<Bar> bars, int period, ref double peak)
        {
            for (var i = 0; i < bars.Count; i++)
            {
                var equity = account.GetEquity(i);
                if (equity > peak)
                    peak = equity;

                curve.Add(new CurvePoint()
                {
                    Date = bars[i].Date,
                    Equity = equity,
                    DrawdownPrcnt = peak > 0 ? (peak - equity) / peak * 100 : 0,
                    Period = period,
                });
            }
        }

        public static double GetMaxDrawdownPrcnt(List<CurvePoint> curve)
        {
            var max = 0d;
            foreach (var point in curve)
                if (point.DrawdownPrcnt > max)
                    max = point.DrawdownPrcnt;

            return max;
        }

        public static void WriteCurve(List<CurvePoint> curve, string fileName)
        {
            var culture = CultureInfo.InvariantCulture;
            var builder = new StringBuilder();
            builder.AppendLine("Дата;Период;Капитал;Просадка, %");

            foreach (var point in curve)
                builder.Append(point.Date.ToString("dd.MM.yyyy HH:mm", culture))
                    .Append(';')
                    .Append(point.Period.ToString(culture))
                    .Append(';')
                    .Append(Math.Round(point.Equity, 2).ToString(culture))
                    .Append(';')
                    .Append(Math.Round(point.DrawdownPrcnt, 3).ToString(culture))
                    .AppendLine();

            File.WriteAllText(fileName, builder.ToString(), Encoding.UTF8);
        }

        public static void WriteWindows(List<WindowResult> windows, string fileName)
        {
            var culture = CultureInfo.InvariantCulture;
            var builder = new StringBuilder();
            builder.AppendLine("Период;Начало;Конец;Капитал на входе;Капитал на выходе;" +
                "Прибыль, %;Просадка окна, %;Сделок");

            foreach (var window in windows)
                builder.Append(window.Period.ToString(culture))
                    .Append(';')
                    .Append(window.Start.ToString("dd.MM.yyyy", culture))
                    .Append(';')
                    .Append(window.End.ToString("dd.MM.yyyy", culture))
                    .Append(';')
                    .Append(Math.Round(window.StartEquity, 2).ToString(culture))
                    .Append(';')
                    .Append(Math.Round(window.EndEquity, 2).ToString(culture))
                    .Append(';')
                    .Append(Math.Round(window.ProfitPrcnt, 2).ToString(culture))
                    .Append(';')
                    .Append(Math.Round(window.MaxDrawDownPrcnt, 2).ToString(culture))
                    .Append(';')
                    .Append(window.DealsCount.ToString(culture))
                    .AppendLine();

            File.WriteAllText(fileName, builder.ToString(), Encoding.UTF8);
        }
    }
}
