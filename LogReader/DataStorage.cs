using System;
using System.Collections.Generic;
using System.IO;

namespace LogPreparator
{
    public class DataStorage
    {
        string fileName;

        public static DataStorage Create(string fileName)
        {
            return new DataStorage(fileName);
        }
        private DataStorage(string fileName) 
        {
            this.fileName = fileName;
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

        public void SaveDataToFile(List<string> lines)
        {
            using (StreamWriter outputFile = new StreamWriter(fileName, false))
                foreach(var line in lines) 
                    outputFile.Write(line);
        }
    }
}