using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using PeparatorDataForSpreadTradingSystems;
using TradingSystems;
using TSLab.Script;

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

            for (var i = 0; i < openFileDialog.FileNames.Length; i++)
            {
                var fullFileName = openFileDialog.FileNames[i];
                var converter = ConverterTextDataToBar.Create(fullFileName);
                var barsBase = converter.ConvertFileWithBarsToListOfBars();

                var securityBase = CustomSecurity.Create(barsBase);
                var securityTsLab = new SecurityTSlab(securityBase);
                var securityCompressed = securityTsLab.CompressLessIntervalTo1DayInterval();
                listCompressedSecurity.Add(securityCompressed);

                var t = securityCompressed.Bars.Last().Date - securityCompressed.Bars.First().Date;
                var months = t.TotalDays / 29.3d;

                securityInfos.Add(new SecurityInfo()
                {
                    Security = securityCompressed,
                    Symbol = securityCompressed.Symbol,
                    Interval = securityCompressed.Interval.ToString() + securityCompressed.IntervalBase.ToString(),
                    startDate = securityCompressed.Bars.First().Date,
                    endDate = securityCompressed.Bars.Last().Date,
                    months = (int)months
                });             
            }

            //for (var i = 0; i < listCompressedSecurity.Count; i++)
            //{
            //    var s = listCompressedSecurity[i];
            //    Console.WriteLine("{0} {1} {2}", s.Symbol, s.Interval, s.IntervalBase);
            //}            

            var orderedSecurityInfos = (from securityInfo in securityInfos
                                       orderby securityInfo.months
                                       select securityInfo).ToList();

            for (var i = 0; i < orderedSecurityInfos.Count; i++)
            {
                var s = orderedSecurityInfos[i];
                Console.WriteLine(orderedSecurityInfos[i].Print());
            }

            Console.WriteLine("Стоп!");
            Console.ReadLine();
            return;

            //var barsCorrelationPreparator = new BarsCorrelationPreparator(barsFirstSecurity, barsSecondSecurity);
            //barsCorrelationPreparator.Prepare();

            //var values1 = new List<double>();
            //foreach (var bar in barsFirstSecurity)
            //    values1.Add(bar.Close);

            //var values2 = new List<double>();
            //foreach (var bar in barsSecondSecurity)
            //    values2.Add(bar.Close);

            //var result = ComputeCoeff(values1.ToArray(), values2.ToArray());

            //Console.WriteLine("Корреляция = " + (int)(result * 100) + "%");
            //Console.ReadLine();
        }

        public static double ComputeCoeff(double[] values1, double[] values2)
        {
            if (values1.Length != values2.Length)
                throw new ArgumentException("values must be the same length");

            var avg1 = values1.Average();
            var avg2 = values2.Average();

            var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
            var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

            var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

            return result;
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
            return string.Format("{0} {1} {2} {3} {4}", Symbol, Interval, startDate.Date, endDate.Date, months);
        }
    }
}
