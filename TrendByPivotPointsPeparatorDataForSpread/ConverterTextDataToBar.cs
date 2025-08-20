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
            string line;
            var bars = new List<Bar>();
            try
            {
                string[] listStrings = new string[0];
                
                if (StringLines.Length > 0)
                {
                    listStrings = StringLines;
                }
                else
                {
                    //var p = System.IO.Directory.GetCurrentDirectory();
                    if (!System.IO.File.Exists(FullFileName))
                        throw new Exception("Файл не найден!");

                    listStrings = System.IO.File.ReadAllLines(FullFileName);

                    if (listStrings == null)
                        throw new Exception("Файл пустой!");
                }
                
                foreach (string str in listStrings)
                {
                    line = str;
                    if (!string.IsNullOrEmpty(str) &&!(str.Contains('<')||str.Contains('>')))
                    {
                        bars.Add(Convert(str));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return bars;
        }

        public Bar Convert(string line)
        {
            var culture = new CultureInfo("en-US");
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
