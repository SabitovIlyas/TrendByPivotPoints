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

        /// <summary>Доля лучших особей, которые переходят в следующее поколение без
        /// изменений (элитизм). 0,2 — это 20 особей на популяции в 100. Чем больше
        /// доля, тем быстрее считается поколение (элиту не пересчитываем), но тем
        /// быстрее вырождается популяция. Всегда переносится хотя бы одна особь.</summary>
        public double EliteFraction = 0.2;

        /// <summary>Порог разнообразия популяции: ниже него оптимизация
        /// останавливается, потому что особи стали почти одинаковыми.</summary>
        public double MinDiversity = 0.1;

        /// <summary>Сколько потоков считают хромосомы поколения; 0 — по числу ядер.
        /// Результат от этого не зависит, только скорость.</summary>
        public int Threads = 0;

        //Как считается фитнес-функция

        /// <summary>Доля лучших прибыльных сделок, которые исключаются перед расчётом
        /// фитнес-функции. Пессимизация: если результат держится на паре удачных
        /// сделок, без них он развалится и хромосома получит низкую оценку.</summary>
        public double ExcludeBestDealsPrcnt = 0.05;

        /// <summary>Порог количества сделок: оценка хромосомы, совершившей меньше,
        /// снижается штрафом. На трёх сделках отличный результат получается по
        /// случайности, и оптимизатор охотно находит такие «решения».
        /// 0 — не штрафовать.</summary>
        public int MinDealsCount = 0;

        /// <summary>Порог максимальной просадки, %. Оценка хромосомы, которая
        /// проседает глубже, снижается штрафом. 0 — не штрафовать.</summary>
        public double MaxDrawDownPrcnt = 0;

        /// <summary>Порог доли выигрышных сделок, %. Оценка хромосомы, которая
        /// выигрывает реже, снижается штрафом: так задаётся характер системы —
        /// например, возврат к среднему должен выигрывать чаще, чем проигрывать.
        /// 0 — не штрафовать.</summary>
        public double MinWinRatePrcnt = 0;

        /// <summary>Жёсткость штрафов. При 1 превышение порога вдвое ополовинит
        /// оценку, при 2 — уменьшит вчетверо.</summary>
        public double PenaltyPower = 1;

        /// <summary>Сколько соседних точек проверяется вокруг хромосомы. 0 — оценивать
        /// только саму хромосому. Иначе фитнес-функция — это усреднение по центру и
        /// соседям: так находится плато устойчивых параметров, а не одиночный пик,
        /// который развалится при малейшем сдвиге.</summary>
        public int NeighbourhoodPoints = 0;

        /// <summary>Насколько далеко отстоит сосед — доля диапазона каждого гена.
        /// Сдвиг приклеивается к сетке параметра и не бывает меньше одного шага.</summary>
        public double NeighbourhoodPercent = 0.05;

        /// <summary>Брать медиану по окрестности вместо среднего: медиана устойчивее
        /// к одному провалившемуся соседу.</summary>
        public bool NeighbourhoodUseMedian = false;

        /// <summary>Сохранять состояние прогона после каждого поколения, чтобы после
        /// сбоя продолжить с последнего поколения, а не с начала.</summary>
        public bool SaveCheckpoint = true;

        /// <summary>Файл чек-поинта; пусто — рядом со сводным отчётом.</summary>
        public string CheckpointFile = string.Empty;

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
