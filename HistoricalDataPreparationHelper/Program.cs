using System;

namespace HistoricalDataPreparationHelper
{
    public partial class Program
    {
        static void Main(string[] args)
        {
            var timestamp = 1739990400;
            var converter = new UnixTimeConverter();
            DateTime dt = converter.FromUnixTimeSeconds(timestamp);
            Console.WriteLine(dt);            
            Console.WriteLine(dt.ToString("yyyyMMdd,HHmmss"));
            Console.ReadLine();
        }
    }
}
