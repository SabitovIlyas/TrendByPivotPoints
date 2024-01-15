using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    partial class Program
    {
        private static string path;
        private static string fileNameLong;
        private static string fileNameShort;
        private static int dimension;
        static void Main(string[] args)
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

        private static void PrintOptimalParameters(PositionSide side, int[] radiusNeighbour, int barrier)
        {
            var fullFileName = path;
            if (side == PositionSide.Long)
                fullFileName += fileNameLong;
            else if (side == PositionSide.Short)
                fullFileName += fileNameShort;

            //Console.WriteLine("\r\n {0}, dimension: 2, radiusNeighbour: new int[2] { 2, 2 }, barrier: 2\r\n", side.ToString());
            //Console.WriteLine("\r\n" + side + ", dimension: 2, radiusNeighbour: new int[2] { 2, 2 }, barrier: 2\r\n");
            Console.WriteLine("\r\n{0}, + dimension: {1}, radiusNeighbour: {2}; {3}, barrier: {4}\r\n", side, dimension, radiusNeighbour[0], radiusNeighbour[1], barrier);
            var result = GetOptimalParameters(fullFileName, dimension, radiusNeighbour, barrier);
            Console.WriteLine(result.ToString());
            Console.WriteLine("\r\n=============================================================================================\r\n");
        }

        private static string GetOptimalParameters(string fullFileName, int dimension, int[] radiusNeighbour, double barrier, bool isCheckedPass = true)
        {

            var parser = ParserPointValueFromFile.Create(fullFileName);
            var points = parser.ParseForPoints();
            var optimizator = Optimizator.Create();
            return optimizator.GetOptimalParameters(points, dimension, radiusNeighbour, barrier, isCheckedPass);
        }
    }
}