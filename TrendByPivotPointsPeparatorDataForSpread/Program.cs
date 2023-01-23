using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPointsPeparatorDataForSpread
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Старт!");
            var converter = ConverterTextDataToBar.Create(
                @"C:\Users\1\Downloads\SPFB.BR-2.23_230101_230131.txt");
            var bars1 = converter.ConvertFileWithBarsToListOfBars();
            converter.FullFileName = @"C:\Users\1\Downloads\SPFB.BR-3.23_230101_230131.txt";
            var bars2 = converter.ConvertFileWithBarsToListOfBars();

            var spread = Spread.Create(bars2, bars1);

            var fullFileName = @"C:\Users\1\Downloads\SPFB.BR-2.23-3.23_230101_230131.txt";
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
