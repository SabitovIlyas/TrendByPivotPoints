using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabitovCapitalConsole.Entities
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
    }
}