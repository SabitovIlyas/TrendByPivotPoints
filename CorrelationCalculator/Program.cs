using System;
using System.Collections.Generic;
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

            for (var i = 0; i < openFileDialog.FileNames.Length; i++)
            {
                var fullFileName = openFileDialog.FileNames[i];
                var converter = ConverterTextDataToBar.Create(fullFileName);
                var barsBase = converter.ConvertFileWithBarsToListOfBars();

                var securityBase = CustomSecurity.Create(barsBase);
                var securityTsLab = new SecurityTSlab(securityBase);
                var securityCompressed = securityTsLab.CompressLessIntervalTo1DayInterval();
                listCompressedSecurity.Add(securityCompressed);

                var s = listCompressedSecurity[0];
                Console.WriteLine("{0} {1} {2}", s.Symbol, s.Interval, s.IntervalBase);
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

        private static void OpenFileDialog()
        {

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
}
