using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Выгружает кривую капитала прогона: капитал на каждом баре и просадку от
    /// достигнутого максимума.
    ///
    /// Сводный отчёт даёт проценты по отдельным окнам, и каждое окно начинается
    /// с одного и того же депозита — сквозной просадки по ним не увидеть. Кривая
    /// нужна там, где стратегия работает непрерывно.
    /// </summary>
    public static class EquityCurveWriter
    {
        public static void Write(ChromosomeUniversal chromosome, string fullFileName,
            Logger logger)
        {
            if (string.IsNullOrEmpty(fullFileName))
                return;

            var fitness = chromosome != null ? chromosome.Fitness : null;
            var account = fitness != null ? fitness.Account : null;
            var bars = fitness != null ? fitness.Bars : null;

            if (account == null || bars == null || bars.Count == 0)
            {
                logger.Log("Кривую капитала выгрузить не удалось: прогон хромосомы " +
                    "не выполнен либо бары не заданы.");
                return;
            }

            try
            {
                var culture = CultureInfo.InvariantCulture;
                var builder = new StringBuilder();
                builder.AppendLine("Дата;Капитал;Просадка, %");

                var peak = double.MinValue;

                for (var i = 0; i < bars.Count; i++)
                {
                    var equity = account.GetEquity(i);
                    if (equity > peak)
                        peak = equity;

                    var drawdown = peak > 0 ? (peak - equity) / peak * 100 : 0;

                    builder.Append(bars[i].Date.ToString("dd.MM.yyyy HH:mm", culture))
                        .Append(';')
                        .Append(Math.Round(equity, 2).ToString(culture))
                        .Append(';')
                        .Append(Math.Round(drawdown, 3).ToString(culture))
                        .AppendLine();
                }

                var directory = Path.GetDirectoryName(fullFileName);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(fullFileName, builder.ToString(), Encoding.UTF8);
                logger.Log("Кривая капитала: {0} ({1} баров)", fullFileName, bars.Count);
            }
            catch (Exception e)
            {
                //Отчёт — не повод ронять прогон.
                logger.Log("Не удалось записать кривую капитала «{0}»: {1}",
                    fullFileName, e.Message);
            }
        }
    }
}
