using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrendByPivotPointsPeparatorDataForSpread
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
            var bars1 = converter.ConvertFileWithBarsToListOfBars();

            Console.WriteLine("Ок!");
            Console.WriteLine("Введите путь ко второму файлу:");

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                fullFileName = openFileDialog.FileName;
            else
                return;

            converter.FullFileName = fullFileName;
            var bars2 = converter.ConvertFileWithBarsToListOfBars();

            var spread = Spread.Create(bars2, bars1);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                fullFileName = saveFileDialog.FileName;
            else
                return;

            StreamWriter writer = new StreamWriter(fullFileName, false);
            writer.WriteLine("<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>");
            foreach(var bar in spread.Bars)
                writer.WriteLine(bar.ToString());
            writer.Close();
            Console.WriteLine("Запись завершена");
            Console.ReadLine();
            
        }
    }
}