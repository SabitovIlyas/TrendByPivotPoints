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

        /// <summary>Сколько случайных особей соревнуются за право стать родителем.</summary>
        public int TournamentSize = 4;

        /// <summary>Порог разнообразия популяции: ниже него оптимизация
        /// останавливается, потому что особи стали почти одинаковыми.</summary>
        public double MinDiversity = 0.1;

        //Окна бэктеста и форвардного анализа, в днях
        public int BackwardDays = 1460;
        public int ForwardDays = 1460;
        public int ForwardPeriodsCount = 10;
        public int ShiftWindowDays = 30;

        /// <summary>Отрезать историю, которая не попадает ни в одно окно тестирования.
        /// Позволяет подавать файл котировок целиком, не обрезая его вручную.</summary>
        public bool TrimHistory = true;

        //Счёт
        public double Equity = 100000;
        public double RiskValuePrcnt = 2;

        //Пути к файлам данных: если заданы, оптимизатор не спрашивает их диалогами
        public string SecuritiesFile = string.Empty;
        public string SeedGenesFile = string.Empty;

        /// <summary>Файл журнала прогона; пустое значение — имя с датой и временем
        /// в рабочей папке оптимизатора.</summary>
        public string LogFile = string.Empty;

        //Переопределение диапазонов поиска параметров стратегии
        //(строки вида «Range:имя:мин:макс:шаг»)
        public Dictionary<string, ParameterRange> ParameterRanges =
            new Dictionary<string, ParameterRange>();
    }

    /// <summary>Диапазон поиска одного параметра: минимум, максимум, шаг.</summary>
    public class ParameterRange
    {
        public double Min;
        public double Max;
        public double Step;
    }
}
