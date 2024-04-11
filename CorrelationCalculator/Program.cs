using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using PeparatorDataForSpreadTradingSystems;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Options;
using TSLab.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace CorrelationCalculator
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Старт!");
            Console.WriteLine("Сжимаем бары в дневные!");
            
            OpenFileDialog openFileDialog = new OpenFileDialog();            
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Выберите файлы с историческими данными";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
                        
            Console.WriteLine("Ок!\n");
            var listCompressedSecurity = new List<ISecurity>();
            var securityInfos = new List<SecurityInfo>();
            var averageDaysPerMonth = 365.25 / 12;

            var startTime = DateTime.Now;

            for (var i = 0; i < openFileDialog.FileNames.Length; i++)
            {
                var fullFileName = openFileDialog.FileNames[i];
                var converter = ConverterTextDataToBar.Create(fullFileName);
                var barsBase = converter.ConvertFileWithBarsToListOfBars();

                var securityBase = CustomSecurity.Create(barsBase);
                var securityTsLab = new TSLabSecurity(securityBase);
                var securityCompressed = securityTsLab.CompressLessIntervalTo1DayInterval();
                listCompressedSecurity.Add(securityCompressed);

                var t = securityCompressed.Bars.Last().Date - securityCompressed.Bars.First().Date;
                var months = t.TotalDays / averageDaysPerMonth;

                securityInfos.Add(new SecurityInfo()
                {
                    Security = securityCompressed,
                    Symbol = securityCompressed.Symbol,
                    Interval = securityCompressed.Interval.ToString() + securityCompressed.IntervalBase.ToString(),
                    startDate = securityCompressed.Bars.First().Date,
                    endDate = securityCompressed.Bars.Last().Date,
                    months = (int)months
                });
                Console.WriteLine("{0} / {1})\tСжато {2}", i + 1, openFileDialog.FileNames.Length, securityInfos[i].Symbol);
            }
            Console.WriteLine();

            var orderedSecurityInfos = (from securityInfo in securityInfos
                                       orderby securityInfo.months
                                       select securityInfo).ToList();

            for (var i = 0; i < orderedSecurityInfos.Count; i++)
            {
                var s = orderedSecurityInfos[i];
                Console.WriteLine("{0} / {1})\t{2}", i + 1, orderedSecurityInfos.Count, orderedSecurityInfos[i].Print());
            }
            
            var endTime = DateTime.Now;

            Console.WriteLine("Прошло {0} секунд.", (int)(endTime - startTime).TotalSeconds);

            Console.Write("Количество месяцев для форвардного тестирования: "); 
            var forward = int.Parse(Console.ReadLine());

            Console.Write("Количество месяцев для бэктеста: ");
            var backward = int.Parse(Console.ReadLine());
            Console.Write("Количество периодов для анализа тестирования: ");
            var periodsForTesting = int.Parse(Console.ReadLine());

            var totalMonthsForAnalysis = periodsForTesting * forward + backward;
            Console.WriteLine("Необходимое количество месяцев для тестирования: {0}\n", totalMonthsForAnalysis);

            var selectedSecurityInfos = (from securityInfo in orderedSecurityInfos
                                         where securityInfo.months >= totalMonthsForAnalysis
                                         select securityInfo).ToList();

            for (var i = 0; i < selectedSecurityInfos.Count; i++)
            {
                var s = selectedSecurityInfos[i];
                Console.WriteLine("{0} / {1})\t{2}", i + 1, selectedSecurityInfos.Count, selectedSecurityInfos[i].Print());
            }

            var maxEndDate = selectedSecurityInfos.Max(securityInfo => securityInfo.endDate);

            Console.WriteLine("Последняя дата: {0}\n", maxEndDate);

            selectedSecurityInfos = (from securityInfo in selectedSecurityInfos
                                     where securityInfo.endDate == maxEndDate
                                     select securityInfo).ToList();

            for (var i = 0; i < selectedSecurityInfos.Count; i++)
            {
                var s = selectedSecurityInfos[i];
                Console.WriteLine("{0} / {1})\t{2}", i + 1, selectedSecurityInfos.Count, selectedSecurityInfos[i].Print());
            }

            Console.Write("Количество периодов для анализа корреляции: ");
            var periodsForCorrelationAnalysis = int.Parse(Console.ReadLine());
            Console.Write("Размер периода для анализа корреляции: ");
            var periodCorrelation = int.Parse(Console.ReadLine());

            if (selectedSecurityInfos.Count == 0)
                return;

            var matrix = new List<CorrMatrixElement>();

            var startDate = selectedSecurityInfos.First().endDate + new TimeSpan(1, 0, 0, 0);
            for (var i = 0; i < periodsForCorrelationAnalysis; i++)
            {
                var endDate = startDate - new TimeSpan(1, 0, 0, 0);
                var prep = new BarsCorrelationPreparator();
                startDate =  prep.GetStartDateTime(endDate, periodCorrelation);

                foreach (var sec1 in selectedSecurityInfos)
                {   
                    var bars1 = (from bar in sec1.Security.Bars
                                 where bar.Date >= startDate && bar.Date <= endDate
                                 select bar).ToList();

                    var bars1list = new List<Bar>();
                    foreach (var bar in bars1)
                        bars1list.Add(new Bar() { Ticker = sec1.Symbol, Period = sec1.Interval, Date = bar.Date, Open = bar.Open, High = bar.High, Low = bar.Low, Close = bar.Close, Volume = bar.Volume, DigitsAfterPoint = -1 });

                    foreach (var sec2 in selectedSecurityInfos)
                    {
                        var bars2 = (from bar in sec2.Security.Bars
                                     where bar.Date >= startDate && bar.Date <= endDate
                                     select bar).ToList();

                        var bars2list = new List<Bar>();
                        foreach (var bar in bars2)
                            bars2list.Add(new Bar() { Ticker = sec2.Symbol, Period = sec2.Interval, Date = bar.Date, Open = bar.Open, High = bar.High, Low = bar.Low, Close = bar.Close, Volume = bar.Volume, DigitsAfterPoint = -1 });

                        var barsCorrelationPreparator = new BarsCorrelationPreparator(bars1list, bars2list);
                        barsCorrelationPreparator.Prepare();

                        var values1 = new List<double>();
                        foreach (var bar in bars1list)
                            values1.Add(bar.Close);

                        var values2 = new List<double>();
                        foreach (var bar in bars2list)
                            values2.Add(bar.Close);

                        var result = barsCorrelationPreparator.ComputeCoeff(values1.ToArray(), values2.ToArray());
                        matrix.Add(new CorrMatrixElement() { startDate = startDate, endDate = endDate, corrCoef = result, Symbol1 = sec1.Symbol, Symbol2 = sec2.Symbol, Interval = sec1.Interval });
                    }
                }
            }            
                                    
            PrintMatrixToCSV(matrix);
            PrintMatrixToCSV1(matrix);
            Console.WriteLine("Стоп!");
            Console.ReadLine();
            return;
        }  
        
        private static void PrintMatrixToCSV(List<CorrMatrixElement> matrix)
        {
            if (matrix.Count == 0)
                return;

            var fbw = new FolderBrowserDialog();            
            
            if (fbw.ShowDialog() != DialogResult.OK)
                return;

            var maxEndDate = matrix.Max(element => element.endDate);                       

            var symbols = (from element in matrix
                           where element.endDate == maxEndDate
                           select element.Symbol1).ToList();

            symbols = symbols.Distinct().ToList();

            var endDates = (from element in matrix
                            where element.Symbol1 == matrix.First().Symbol1
                            select element.endDate).ToList();

            endDates = endDates.Distinct().ToList();

            foreach (var endDate in endDates)
            {
                var separator = ";";
                var body = String.Empty;

                var header = separator;
                foreach (var symbol in symbols)
                    header += symbol + separator;
                
                header = header.TrimEnd(separator.First());                                

                foreach (var symbol1 in symbols)
                {
                    body += '\n' + symbol1 + separator;

                    foreach (var symbol2 in symbols)
                    {
                        var el = (from element in matrix
                                  where element.endDate == endDate && 
                                  element.Symbol1 == symbol1 && 
                                  element.Symbol2 == symbol2
                                  select Math.Round(element.corrCoef * 100)).First();
                        
                        body += el + separator;
                    }                        
                }

                var startDate = (from element in matrix
                          where element.endDate == endDate
                          select element.startDate).First();
                var path = fbw.SelectedPath;
                StreamWriter sw = new StreamWriter(path + "\\" + startDate.ToShortDateString() + " -- " + endDate.ToShortDateString() + ".csv");
                sw.WriteLine(header + body);             
                sw.Close();
            }
        }

        private static void PrintMatrixToCSV1(List<CorrMatrixElement> matrix)
        {
            if (matrix.Count == 0)
                return;

            var fbw = new FolderBrowserDialog();

            if (fbw.ShowDialog() != DialogResult.OK)
                return;

            var maxEndDate = matrix.Max(element => element.endDate);

            var symbols = (from element in matrix
                           where element.endDate == maxEndDate
                           select element.Symbol1).ToList();

            symbols = symbols.Distinct().ToList();

            var endDates = (from element in matrix
                            where element.Symbol1 == matrix.First().Symbol1
                            select element.endDate).ToList();

            endDates = endDates.Distinct().ToList();

            var separator = ";";
            var body = String.Empty;

            var header = separator;
            foreach (var symbol in symbols)
                header += symbol + separator;

            header = header.TrimEnd(separator.First());

            foreach (var symbol1 in symbols)
            {
                body += '\n' + symbol1 + separator;

                foreach (var symbol2 in symbols)
                {
                    var els = (from element in matrix
                              where element.Symbol1 == symbol1 &&
                              element.Symbol2 == symbol2
                              select Math.Round(element.corrCoef * 100)).ToList();

                    var max = els.Max();
                    var min = els.Min();

                    var r = Math.Max(Math.Abs(max), Math.Abs(min));
                    r = els.Find(p => Math.Abs(p) == Math.Abs(r));

                    body += r + separator;
                }
            }            

            var path = fbw.SelectedPath;
            var endDate = matrix.Max(element => element.endDate);
            var startDate = matrix.Min(element => element.startDate);

            StreamWriter sw = new StreamWriter(path + "\\" + startDate.ToShortDateString() + " -- " + endDate.ToShortDateString() + "_pessimistic.csv");
            sw.WriteLine(header + body);
            sw.Close();
        }
    }

    struct SecurityInfo
    {
        public ISecurity Security;
        public string Symbol;
        public string Interval;
        public DateTime startDate;
        public DateTime endDate;
        public int months;

        public string Print()
        {
            return string.Format("{0} {1} {2} {3} {4}", Symbol, Interval, startDate.Date.ToShortDateString(), endDate.Date.ToShortDateString(), months);
        }
    }

    struct CorrMatrixElement
    {        
        public string Symbol1;
        public string Symbol2;
        public string Interval;
        public DateTime startDate;
        public DateTime endDate;        
        public double corrCoef;        
    }
}