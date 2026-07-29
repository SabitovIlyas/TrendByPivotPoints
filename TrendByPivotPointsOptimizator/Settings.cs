using System.Collections.Generic;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Настройки оптимизатора, читаются из файла настроек (строки вида «Ключ:Значение»).
    /// Все значения имеют разумные умолчания — прежние зашитые в код константы.
    /// </summary>
    public class Settings
    {
        public List<PositionSide> Sides;
        public List<Interval> TimeFrames;

        /// <summary>Имя стратегии для универсального оптимизатора
        /// (например, MeanReversion или DonchianUniversal). Пустое — старый путь Дончиана.</summary>
        public string Strategy = string.Empty;

        /// <summary>Сид генератора случайных чисел; null — случайный запуск.</summary>
        public int? Seed = null;

        //Параметры генетического алгоритма
        public int PopulationSize = 100;
        public int Generations = 300;
        public double CrossoverRate = 0.85;
        public double MutationRate = 0.10;
        public int Patience = 50;

        //Окна бэктеста и форвардного анализа, в днях
        public int BackwardDays = 1460;
        public int ForwardDays = 1460;
        public int ForwardPeriodsCount = 10;
        public int ShiftWindowDays = 30;

        //Счёт
        public double Equity = 100000;
        public double RiskValuePrcnt = 2;
    }
}
