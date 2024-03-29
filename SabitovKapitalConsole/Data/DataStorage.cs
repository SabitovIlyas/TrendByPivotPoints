﻿using SabitovCapitalConsole.Entities;

namespace SabitovCapitalConsole.Data
{
    public class DataStorage
    {
        Portfolio portfolio;
        string fileName;

        public static DataStorage Create(string fileName)
        {
            return new DataStorage(fileName);
        }
        private DataStorage(string fileName) 
        {
            this.fileName = fileName;
        }

        public void SaveDataToFile(Portfolio portfolio)
        {
            using (StreamWriter outputFile = new StreamWriter(fileName))
                outputFile.Write(portfolio);
        }

        public void SaveDataToFile()
        {
            SaveDataToFile(portfolio);            
        }

        public string ReadFile()
        {
            var result = string.Empty;
            try
            {
                using (var sr = new StreamReader(fileName))
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
            var serializedPortfolio = ReadFile();
            var portfolioSerializator = PortfolioSerializator.Create(serializedPortfolio);
            portfolio = (Portfolio)portfolioSerializator.Deserialize();
            return portfolio;
        }
    }
}