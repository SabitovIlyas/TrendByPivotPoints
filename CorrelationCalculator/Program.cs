using System;
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
        }
    }
}
