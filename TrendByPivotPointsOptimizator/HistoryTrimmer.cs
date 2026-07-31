using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Отрезает от истории всё, что заведомо не попадёт ни в одно окно бэктеста
    /// или форвардного теста. Позволяет скармливать оптимизатору исходный файл
    /// котировок целиком: лишние годы не занимают память и не пересортировываются
    /// заново при расчёте каждой хромосомы.
    /// </summary>
    public static class HistoryTrimmer
    {
        /// <summary>
        /// Сколько дней истории нужно оптимизатору: окно бэктеста, окно форвардного
        /// теста и сдвиг окна на каждый дополнительный форвардный период.
        /// </summary>
        public static int GetRequiredDays(Settings settings)
        {
            var periodsCount = Math.Max(settings.ForwardPeriodsCount, 1);
            return settings.BackwardDays + settings.ForwardDays +
                settings.ShiftWindowDays * (periodsCount - 1);
        }

        /// <summary>
        /// Запас в днях, на который читаем историю глубже начала самого раннего окна.
        /// Нужен, чтобы слева от границы окна гарантированно оказался хотя бы один
        /// бар: само начало окна может попасть на выходные или длинные праздники,
        /// когда торгов нет.
        /// </summary>
        public const int ExtraDaysToRead = 30;

        /// <summary>
        /// Самая ранняя дата, которая может понадобиться: начало бэктеста самого
        /// старого форвардного периода. Считается от последнего бара истории —
        /// именно от него ForwardAnalysis отсчитывает все окна.
        /// </summary>
        public static DateTime GetEarliestRequiredDate(List<Bar> bars, Settings settings)
        {
            return GetEarliestRequiredDate(bars.Max(b => b.Date), settings);
        }

        public static DateTime GetEarliestRequiredDate(DateTime lastBarDate, Settings settings)
        {
            return lastBarDate.AddDays(-(GetRequiredDays(settings) - 1));
        }

        /// <summary>
        /// С какой даты читать файл котировок: начало самого раннего окна минус запас
        /// на нерабочие дни, округлённое вниз до суток. Округление до суток
        /// обязательно: сетка сжатия баров привязана к абсолютному времени, и обрезка
        /// внутри суток дала бы неполный первый бар таймфрейма.
        /// </summary>
        public static DateTime GetReadFromDate(DateTime lastBarDate, Settings settings)
        {
            return GetEarliestRequiredDate(lastBarDate, settings)
                .AddDays(-ExtraDaysToRead).Date;
        }

        /// <summary>
        /// Возвращает бары, начиная с последнего бара, который не позже самой ранней
        /// нужной даты. Этот граничный бар остаётся намеренно: ForwardAnalysis считает
        /// историю недостаточной, если первый бар оказывается позже начала окна.
        /// Если такого бара нет, истории и так впритык — возвращаем всё как есть.
        /// </summary>
        public static List<Bar> Trim(List<Bar> bars, Settings settings)
        {
            if (bars == null || bars.Count == 0)
                return bars;

            var earliestRequiredDate = GetEarliestRequiredDate(bars, settings);

            var barsBeforeWindows = bars.Where(b => b.Date <= earliestRequiredDate).ToList();
            if (!barsBeforeWindows.Any())
                return bars;

            var firstNeededDate = barsBeforeWindows.Max(b => b.Date);
            return bars.Where(b => b.Date >= firstNeededDate).ToList();
        }
    }
}
