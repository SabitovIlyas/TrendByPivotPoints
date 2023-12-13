using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TrendByPivotPointsPeparatorDataForSpread;

namespace CorrelationCalculator
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Старт!");
            Console.WriteLine("Введите путь к первому файлу:");
            var fullFileName = String.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                fullFileName = openFileDialog.FileName;
            else
                return;

            var converter = ConverterTextDataToBar.Create(fullFileName);
            var barsFirstSecurity = converter.ConvertFileWithBarsToListOfBars();

            Console.WriteLine("Ок!");
            Console.WriteLine("Введите путь ко второму файлу:");

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                fullFileName = openFileDialog.FileName;
            else
                return;

            converter.FullFileName = fullFileName;
            var barsSecondSecurity = converter.ConvertFileWithBarsToListOfBars();

            var barsCorrelationPreparator = new BarsCorrelationPreparator(barsFirstSecurity, barsSecondSecurity);
            barsCorrelationPreparator.Prepare();

            var values1 = new List<double>();
            foreach (var bar in barsFirstSecurity)
                values1.Add(bar.Close);

            var values2 = new List<double>();
            foreach (var bar in barsSecondSecurity)
                values2.Add(bar.Close);

            var result = ComputeCoeff(values1.ToArray(), values2.ToArray());

            Console.WriteLine(result);
            Console.ReadLine();
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
