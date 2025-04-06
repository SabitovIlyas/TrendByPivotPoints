using System;
using System.IO;
using System.Windows.Forms;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    partial class Program
    {
        private static string path;
        private static string fileNameLong;
        private static string fileNameShort;
        private static int dimension;
        private static int useCase = 3;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Введите номер примера использования");
            useCase = int.Parse(Console.ReadLine());
            switch (useCase)
            {
                case 1:
                    {
                        PrintOptimalParametersUseCase();
                        break;
                    }
                case 2:
                    {
                        PrintOptimalParametersPercentUseCase();
                        break;
                    }
                case 3:
                    {
                        Optimize();
                        break;
                    }
            }
        }

        private static void Optimize()
        {
            //var os = new OptimizatorStarter();
            var os = new OptimizatorGeneticAlgorithmStarter();
            os.Start();
        }        

        private static void PrintOptimalParameters(PositionSide side, int[] radiusNeighbour, int barrier)
        {
            if (useCase == 1)
            {
                var fullFileName = path;
                if (side == PositionSide.Long)
                    fullFileName += fileNameLong;
                else if (side == PositionSide.Short)
                    fullFileName += fileNameShort;

                Console.WriteLine("\r\n{0}, + dimension: {1}, radiusNeighbour: {2}; {3}, barrier: {4}\r\n", side, dimension, radiusNeighbour[0], radiusNeighbour[1], barrier);
                var result = GetOptimalParameters(fullFileName, dimension, radiusNeighbour, barrier);
                Console.WriteLine(result.ToString());
                Console.WriteLine("\r\n=============================================================================================\r\n");
            }            
            
            if (useCase == 2)
            {
                var fullFileName = path;
                var result = GetOptimalParameters(fullFileName, dimension, radiusNeighbour, barrier);
                StreamWriter sw = new StreamWriter(fullFileName.Trim(".csv".ToCharArray()) + "_result.csv");
                sw.WriteLine(result);
                sw.Close();
            }
        }

        private static string GetOptimalParameters(string fullFileName, int dimension, int[] radiusNeighbour, double barrier, bool isCheckedPass = true)
        {
            var parser = ParserPointValueFromFile.Create(fullFileName);            

            switch (useCase)
            {
                case 1:
                    {
                        parser.Param1Str = "ВнешнийСкрипт.slowDonchian";    //тут другие были параметры
                        parser.Param2Str = "ВнешнийСкрипт.fastDonchian";
                        var points = parser.ParseForPoints();
                        var optimizator = Optimizator.Create();
                        return optimizator.GetOptimalParameters(points, dimension, radiusNeighbour, barrier, isCheckedPass);  // TODO: не удалять комментарий, а сделать нормально
                    }
                case 2:
                    {
                        parser.Param1Str = "ВнешнийСкрипт.slowDonchian";
                        parser.Param2Str = "ВнешнийСкрипт.fastDonchian";
                        var points = parser.ParseForPoints();
                        var optimizator = Optimizator.Create();
                        return optimizator.GetOptimalParametersPercentNew(points, dimension, radiusNeighbour, barrier, isCheckedPass);
                    }
            }

            return "";
        }

        private static void PrintOptimalParametersUseCase()
        {
            path = "C:\\Users\\1\\Dropbox\\Трейдинг\\";

            fileNameLong = "results_Long.csv";
            fileNameShort = "results_Short.csv";
            dimension = 2;

            Console.WriteLine("Start!");

            PrintOptimalParameters(PositionSide.Long, new int[2] { 1, 1 }, barrier: 1);
            PrintOptimalParameters(PositionSide.Long, new int[2] { 1, 1 }, barrier: 2);
            PrintOptimalParameters(PositionSide.Long, new int[2] { 1, 1 }, barrier: 3);

            PrintOptimalParameters(PositionSide.Long, new int[2] { 2, 2 }, barrier: 1);
            PrintOptimalParameters(PositionSide.Long, new int[2] { 2, 2 }, barrier: 2);
            PrintOptimalParameters(PositionSide.Long, new int[2] { 2, 2 }, barrier: 3);

            PrintOptimalParameters(PositionSide.Short, new int[2] { 1, 1 }, barrier: 1);
            PrintOptimalParameters(PositionSide.Short, new int[2] { 1, 1 }, barrier: 2);
            PrintOptimalParameters(PositionSide.Short, new int[2] { 1, 1 }, barrier: 3);

            PrintOptimalParameters(PositionSide.Short, new int[2] { 2, 2 }, barrier: 1);
            PrintOptimalParameters(PositionSide.Short, new int[2] { 2, 2 }, barrier: 2);
            PrintOptimalParameters(PositionSide.Short, new int[2] { 2, 2 }, barrier: 3);

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

        private static void PrintOptimalParametersPercentUseCase()               
        {
            Console.WriteLine("Start!");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Выберите файлы с историческими данными";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            //path = "C:\\Users\\sabitovii\\Documents\\Трейдинг\\Оптимизация\\Backward\\";

            //fileNameLong = "Donchian_Br_Script_Long---BR-1D-01-01-2015--31-12-2018.csv";
            //fileNameShort = "results_Short.csv";
            dimension = 2;

            for (var i = 0; i < openFileDialog.FileNames.Length; i++)
            {
                path = openFileDialog.FileNames[i];
                PrintOptimalParameters(PositionSide.Long, new int[2] { 5, 5 }, barrier: 1); //29; 24
            }               

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}