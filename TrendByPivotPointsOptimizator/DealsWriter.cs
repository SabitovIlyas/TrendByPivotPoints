using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Выгружает сделки прогона: даты и цены входа-выхода, объём, прибыль, а также
    /// размах бара входа и выхода.
    ///
    /// Размах нужен не для красоты. Пробойная стратегия входит стоп-заявкой, и
    /// проскальзывание на ней тем больше, чем быстрее движение. Если крупные
    /// прибыльные сделки систематически открываются на барах повышенного размаха,
    /// то оценивать издержки средним проскальзыванием нельзя — оно ляжет тяжелее
    /// именно на те сделки, которыми стратегия живёт.
    /// </summary>
    public static class DealsWriter
    {
        public static void Write(ChromosomeUniversal chromosome, string fullFileName,
            Logger logger)
        {
            if (string.IsNullOrEmpty(fullFileName))
                return;

            var fitness = chromosome != null ? chromosome.Fitness : null;
            var deals = fitness != null ? fitness.Deals : null;
            var bars = fitness != null ? fitness.Bars : null;

            if (deals == null || bars == null || bars.Count == 0)
            {
                logger.Log("Сделки выгрузить не удалось: прогон хромосомы не выполнен " +
                    "либо бары не заданы.");
                return;
            }

            try
            {
                var culture = CultureInfo.InvariantCulture;
                var builder = new StringBuilder();
                builder.AppendLine("Дата входа;Дата выхода;Цена входа;Цена выхода;" +
                    "Контрактов;Прибыль;Баров в сделке;Размах бара входа;" +
                    "Размах бара выхода;Средний размах окна");

                var averageRange = GetAverageRange(bars);

                foreach (var deal in deals)
                {
                    var open = deal.BarNumberOpenPosition;
                    var close = deal.BarNumberClosePosition;

                    if (open < 0 || open >= bars.Count)
                        continue;

                    var closeBar = close >= 0 && close < bars.Count ? close : bars.Count - 1;

                    builder
                        .Append(bars[open].Date.ToString("dd.MM.yyyy HH:mm", culture)).Append(';')
                        .Append(bars[closeBar].Date.ToString("dd.MM.yyyy HH:mm", culture)).Append(';')
                        .Append(Round(deal.EntryPrice)).Append(';')
                        .Append(Round(deal.ExitPrice)).Append(';')
                        .Append(deal.Contracts.ToString(culture)).Append(';')
                        .Append(Round(deal.GetProfit())).Append(';')
                        .Append((closeBar - open).ToString(culture)).Append(';')
                        .Append(Round(bars[open].High - bars[open].Low)).Append(';')
                        .Append(Round(bars[closeBar].High - bars[closeBar].Low)).Append(';')
                        .Append(Round(averageRange))
                        .AppendLine();
                }

                var directory = Path.GetDirectoryName(fullFileName);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(fullFileName, builder.ToString(), Encoding.UTF8);
                logger.Log("Сделки: {0} ({1} шт.)", fullFileName, deals.Count);
            }
            catch (Exception e)
            {
                //Отчёт — не повод ронять прогон.
                logger.Log("Не удалось записать сделки «{0}»: {1}", fullFileName, e.Message);
            }
        }

        private static double GetAverageRange(List<Bar> bars)
        {
            var sum = 0d;
            foreach (var bar in bars)
                sum += bar.High - bar.Low;

            return bars.Count > 0 ? sum / bars.Count : 0;
        }

        private static string Round(double value)
        {
            return Math.Round(value, 4).ToString(CultureInfo.InvariantCulture);
        }
    }
}
