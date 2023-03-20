using System;

namespace LogPreparator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start!");

            var dataStorage = DataStorage.Create("filterInclude.txt");
            var filterIncludedString = dataStorage.ReadFile();
            var filtersIncluded = filterIncludedString.Split('\t');

            dataStorage = DataStorage.Create("filterExclude.txt");
            var filterExcludedString = dataStorage.ReadFile();
            var filtersExcluded = filterExcludedString.Split('\t');

            dataStorage = DataStorage.Create("tsLab.log");
            var log = dataStorage.ReadFile();

            var reader = new LogReader(log);
            reader.SplitLogOnLines();
            reader.UseSubstringFilterIncluded(filtersIncluded);
            reader.UseSubstringFilterExcluded(filtersExcluded);

            dataStorage = DataStorage.Create("tsLabFiltered.log");
            dataStorage.SaveDataToFile(reader.Lines);

            Console.WriteLine("Stop!");
            Console.ReadLine();
        }
    }
}