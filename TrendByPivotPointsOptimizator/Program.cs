using System;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    partial class Program
    {
        private static string path;
        private static string fileNameLong;
        private static string fileNameShort;
        private static int dimension;
        private static int useCase = 2;

        static void Main(string[] args)
        {
            Console.WriteLine("Введите номер примера использования");
            useCase = int.Parse(Console.ReadLine());
            switch (useCase)
            {
                case 1:
                    {
                        PrintOptomalParametersUseCase();
                        break;
                    }
                case 2:
                    {
                        PrintOptimalParametersPercentUseCase();
                        break;
                    }
            }
        }

        private static void PrintOptimalParameters(PositionSide side, int[] radiusNeighbour, int barrier)
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

        private static string GetOptimalParameters(string fullFileName, int dimension, int[] radiusNeighbour, double barrier, bool isCheckedPass = true)
        {
            var parser = ParserPointValueFromFile.Create(fullFileName);
            parser.Param1Str = "ВнешнийСкрипт.slowDonchian";
            parser.Param2Str = "ВнешнийСкрипт.fastDonchian";
            var points = parser.ParseForPoints();
            var optimizator = Optimizator.Create();

            switch (useCase)
            {
                case 1:
                    {
                        return optimizator.GetOptimalParameters(points, dimension, radiusNeighbour, barrier, isCheckedPass);  // TODO: не удалять комментарий, а сделать нормально
                    }
                case 2:
                    {
                        return optimizator.GetOptimalParametersPercent(points, dimension, radiusNeighbour, barrier, isCheckedPass);
                    }
            }

            return "";
        }

        private static void PrintOptomalParametersUseCase()
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
            path = "C:\\Users\\Ильяс\\Documents\\Трейдинг\\Оптимизация\\Backward\\";

            fileNameLong = "Donchian_Br_Script_Long---BR-1D-01-01-2015--31-12-2018.csv";
            //fileNameShort = "results_Short.csv";
            dimension = 2;

            Console.WriteLine("Start!");

            PrintOptimalParameters(PositionSide.Long, new int[2] { 10, 10 }, barrier: 1);

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}