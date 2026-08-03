using System.Text;
using System.Text.RegularExpressions;

namespace ProjectsManager
{
    /// <summary>
    /// Разбирает вывод оптимизатора и держит текущие значения прогона, чтобы их
    /// было видно на форме. Иначе их приходится выискивать в консоли, которая
    /// постоянно доливается и уезжает вниз.
    /// </summary>
    public class OptimizatorStatus
    {
        private const string Unknown = "—";

        //Вывод приходит кусками произвольной длины, поэтому хвост незавершённой
        //строки копим до следующего куска.
        private readonly StringBuilder tail = new();

        private static readonly Regex GenerationRegex = new(
            @"^Поколение (\d+): .*?= (.+?)\. Разнообразие = ([\d,\.]+)% " +
            @"\(порог остановки ([\d,\.]+)%\)\. Поколений без улучшения: (\d+) из (\d+)\.",
            RegexOptions.Compiled);

        private static readonly Regex PeriodRegex = new(@"^Период № (\d+)",
            RegexOptions.Compiled);

        private static readonly Regex PeriodsCountRegex = new(@"периодов (\d+)",
            RegexOptions.Compiled);

        private static readonly Regex ChromosomeRegex = new(
            @"^Посчитана хромосома (\d+) из (\d+)\.", RegexOptions.Compiled);

        private static readonly Regex PopulationRegex = new(
            @"^Популяция (\d+): считаем (\d+), в элите (\d+), взяли из кэша (\d+)\.",
            RegexOptions.Compiled);

        private static readonly Regex StopRegex = new(
            @"^Остановка на поколении (\d+): (.+)$", RegexOptions.Compiled);

        private static readonly Regex ResumeRegex = new(
            @"^Продолжаем прерванный прогон: этап (\S+), период (\d+), " +
            @"готовых периодов (\d+)\.", RegexOptions.Compiled);

        public string Stage { get; private set; } = Unknown;
        public string Period { get; private set; } = Unknown;
        public string PeriodsCount { get; private set; } = Unknown;
        public string Generation { get; private set; } = Unknown;
        public string Record { get; private set; } = Unknown;
        public string Diversity { get; private set; } = Unknown;
        public string MinDiversity { get; private set; } = Unknown;
        public string Idle { get; private set; } = Unknown;
        public string Patience { get; private set; } = Unknown;
        public string Progress { get; private set; } = Unknown;
        public string LastStop { get; private set; } = string.Empty;
        public bool ResumedFromCheckpoint { get; private set; }

        public void Reset()
        {
            tail.Clear();
            Stage = Period = PeriodsCount = Generation = Record = Unknown;
            Diversity = MinDiversity = Idle = Patience = Progress = Unknown;
            LastStop = string.Empty;
            ResumedFromCheckpoint = false;
        }

        /// <summary>Скармливает очередной кусок вывода. Возвращает true, если
        /// что-то изменилось и подпись на форме надо обновить.</summary>
        public bool Feed(string chunk)
        {
            if (string.IsNullOrEmpty(chunk))
                return false;

            tail.Append(chunk);
            var text = tail.ToString();
            var changed = false;
            var start = 0;

            while (true)
            {
                var end = text.IndexOf('\n', start);
                if (end < 0)
                    break;

                var line = text.Substring(start, end - start).TrimEnd('\r');
                changed |= FeedLine(line);
                start = end + 1;
            }

            tail.Clear();
            tail.Append(text.Substring(start));
            return changed;
        }

        private bool FeedLine(string line)
        {
            var match = GenerationRegex.Match(line);
            if (match.Success)
            {
                Generation = match.Groups[1].Value;
                Record = match.Groups[2].Value;
                Diversity = match.Groups[3].Value;
                MinDiversity = match.Groups[4].Value;
                Idle = match.Groups[5].Value;
                Patience = match.Groups[6].Value;
                return true;
            }

            match = ChromosomeRegex.Match(line);
            if (match.Success)
            {
                Progress = match.Groups[1].Value + " из " + match.Groups[2].Value;
                return true;
            }

            match = PopulationRegex.Match(line);
            if (match.Success)
            {
                Progress = "0 из " + match.Groups[2].Value;
                return true;
            }

            match = PeriodRegex.Match(line);
            if (match.Success)
            {
                Stage = "форвардный период";
                Period = match.Groups[1].Value;
                Generation = Record = Diversity = Idle = Unknown;
                LastStop = string.Empty;
                return true;
            }

            match = ResumeRegex.Match(line);
            if (match.Success)
            {
                ResumedFromCheckpoint = true;
                Period = match.Groups[2].Value;
                return true;
            }

            match = StopRegex.Match(line);
            if (match.Success)
            {
                LastStop = "поколение " + match.Groups[1].Value + ": " +
                    match.Groups[2].Value;
                return true;
            }

            if (line.StartsWith("Актуальная оптимизация"))
            {
                Stage = "главный бэктест";
                Period = Unknown;
                return true;
            }

            //Количество периодов сообщается один раз, в шапке настроек.
            if (line.StartsWith("Окна:"))
            {
                match = PeriodsCountRegex.Match(line);
                if (match.Success)
                {
                    PeriodsCount = match.Groups[1].Value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>Две строки для подписи на форме.</summary>
        public string ToDisplayText()
        {
            var period = Period == Unknown
                ? Stage
                : $"{Stage} {Period}" + (PeriodsCount == Unknown ? "" : $" из {PeriodsCount}");

            var first = $"Этап: {period}    Поколение: {Generation}    " +
                $"Рекорд: {Record}    Хромосом посчитано: {Progress}";

            var second = $"Разнообразие: {Diversity}% (порог {MinDiversity}%)    " +
                $"Поколений без улучшения: {Idle} из {Patience}";

            if (ResumedFromCheckpoint)
                second += "    Продолжен с чек-поинта";

            if (LastStop.Length > 0)
                second += "    Остановка: " + LastStop;

            return first + "\r\n" + second;
        }
    }
}
