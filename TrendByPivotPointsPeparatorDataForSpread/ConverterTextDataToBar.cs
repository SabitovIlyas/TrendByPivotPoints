using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TradingSystems;

namespace PeparatorDataForSpreadTradingSystems
{
    public class ConverterTextDataToBar
    {
        public string FullFileName { get; set; } = string.Empty;
        public string[] StringLines { get; } = new string[0];

        //Одна культура на все строки: создавать её заново на каждый бар слишком дорого
        //для файлов с минутными данными за несколько лет.
        private static readonly CultureInfo culture = new CultureInfo("en-US");

        public static ConverterTextDataToBar Create()
        {
            return new ConverterTextDataToBar();
        }

        public static ConverterTextDataToBar Create(string fullFileName)
        {
            return new ConverterTextDataToBar(fullFileName);
        }

        public static ConverterTextDataToBar Create(string[] stringLines)
        {
            return new ConverterTextDataToBar(stringLines);
        }

        private ConverterTextDataToBar()
        {
        }

        private ConverterTextDataToBar(string fullFileName)
        {
            FullFileName = fullFileName;
        }

        private ConverterTextDataToBar(string[] stringLines)
        {
            StringLines = stringLines;
        }

        public List<Bar> ConvertFileWithBarsToListOfBars()
        {
            return ConvertFileWithBarsToListOfBars(DateTime.MinValue);
        }

        /// <summary>
        /// Разбирает только бары, начиная с указанной даты: строки старее пропускаются
        /// без разбора. Позволяет читать исходный файл котировок целиком, тратя время
        /// лишь на ту часть истории, которая действительно нужна.
        /// </summary>
        public List<Bar> ConvertFileWithBarsToListOfBars(DateTime fromDate)
        {
            var bars = new List<Bar>();
            var fromDateNumber = ToDateNumber(fromDate);

            foreach (string str in ReadLines())
            {
                if (!IsBarLine(str))
                    continue;

                if (fromDateNumber > 0 && GetDateNumber(str) < fromDateNumber)
                    continue;

                bars.Add(Convert(str));
            }

            return bars;
        }

        /// <summary>
        /// Дата последнего бара в файле. Строки не разбираются целиком, поэтому дату
        /// можно узнать заранее — до того, как решать, какую часть истории читать.
        /// Возвращает null, если баров в файле нет.
        /// </summary>
        public DateTime? GetLastBarDate()
        {
            string lastBarLine = null;
            foreach (var str in ReadLines())
            {
                if (IsBarLine(str))
                    lastBarLine = str;
            }

            if (lastBarLine == null)
                return null;

            return Convert(lastBarLine).Date;
        }

        private IEnumerable<string> ReadLines()
        {
            if (StringLines.Length > 0)
                return StringLines;

            if (!System.IO.File.Exists(FullFileName))
                throw new System.IO.FileNotFoundException("Файл не найден!", FullFileName);

            //Потоковое чтение: файл с минутными данными за годы в память не помещаем.
            return System.IO.File.ReadLines(FullFileName);
        }

        private bool IsBarLine(string line)
        {
            return !string.IsNullOrEmpty(line) &&
                !(line.Contains('<') || line.Contains('>'));
        }

        //Дата в формате файла — третье поле, число вида ГГГГММДД. Сравнивать такие
        //числа можно напрямую, не разбирая строку целиком.
        private int GetDateNumber(string line)
        {
            var firstComma = line.IndexOf(',');
            var secondComma = line.IndexOf(',', firstComma + 1);
            var thirdComma = line.IndexOf(',', secondComma + 1);

            return int.Parse(line.Substring(secondComma + 1, thirdComma - secondComma - 1));
        }

        private int ToDateNumber(DateTime date)
        {
            if (date == DateTime.MinValue)
                return 0;

            return date.Year * 10000 + date.Month * 100 + date.Day;
        }

        public Bar Convert(string line)
        {
            var data = line.Split(',');
            var ticker = data[0];
            var period = data[1];
            var dateTime = new DateTime(
                int.Parse(data[2].Substring(0, 4)),
                int.Parse(data[2].Substring(4, 2)),
                int.Parse(data[2].Substring(6, 2)),
                int.Parse(data[3].Substring(0, 2)),
                int.Parse(data[3].Substring(2, 2)),
                int.Parse(data[3].Substring(4, 2)));
            var open = double.Parse(data[4], culture);
            var high = double.Parse(data[5], culture);
            var low = double.Parse(data[6], culture);
            var close = double.Parse(data[7], culture);
            var volume = double.Parse(data[8], culture);

            return Bar.Create(dateTime, open, high, low, close, volume, ticker, period);
        }
    }
}
