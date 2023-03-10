using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Data
{
    public class DataStorage
    {
        public static DataStorage Create()
        {
            return new DataStorage();
        }
        private DataStorage() { }

        public void SaveDataToFile(Portfolio portfolio)
        {
            using (StreamWriter outputFile = new StreamWriter("PortfolioDataBase.txt"))
                outputFile.Write(portfolio);
        }

        public string ReadFile()
        {
            var result = string.Empty;
            try
            {
                using (var sr = new StreamReader("PortfolioDataBase.txt"))
                    result = sr.ReadToEnd();
            }

            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public Portfolio LoadDataFromFile()
        {
            var lines = GetLinesWithData();
            foreach (var line in lines)
            {
            }
            return null;
        }

        private List<string> GetLinesWithData()
        {
            var fileContent = ReadFile();
            var lines = fileContent.Split("\r\n");
            var linesWithData = new List<string>();
            foreach (var line in lines)
            {
                if (line == string.Empty)
                    continue;

                linesWithData.Add(line);
            }

            return linesWithData;
        }
    }
}