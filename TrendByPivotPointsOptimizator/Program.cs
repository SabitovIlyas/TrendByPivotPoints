using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrendByPivotPointsOptimizator
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var path = "C:\\Users\\1\\Dropbox\\Трейдинг\\";
            
            var fileName = "results_Long.csv";
            var result = GetOptimalParameters(path + fileName, dimension: 2, radiusNeighbour: new int[2] { 1, 5 }, barrier: 0, isCheckedPass: true);
            Console.WriteLine(result.ToString());

            Console.WriteLine("\r\n=============================================================================================\r\n");

            //fileName = "results_Short.csv";
            //result = GetOptimalParameters(path + fileName, dimension: 2, radiusNeighbour: new int[2] { 1, 1 }, barrier: 2, isCheckedPass: true);
            //Console.WriteLine(result.ToString());

            //Console.WriteLine("\r\n=============================================================================================\r\n");


            //fileName = "results_Long.csv";
            //result = GetOptimalParameters(path + fileName, dimension: 2, radiusNeighbour: new int[2] { 1, 5 }, barrier: 1, isCheckedPass: true);
            //Console.WriteLine(result.ToString());

            //Console.WriteLine("\r\n=============================================================================================\r\n");

            //fileName = "results_Short.csv";
            //result = GetOptimalParameters(path + fileName, dimension: 2, radiusNeighbour: new int[2] { 1, 1 }, barrier: 1, isCheckedPass: true);
            //Console.WriteLine(result.ToString());

            //Console.WriteLine("\r\n=============================================================================================\r\n");

            Console.WriteLine("Finished!");
            Console.ReadLine();            
        }

        private static string GetOptimalParameters(string fullFileName, int dimension, int[] radiusNeighbour, double barrier, bool isCheckedPass)
        {
            var parser = ParserPointValueFromFile.Create(fullFileName);
            var points = parser.ParseForPoints();
            var optimizator = Optimizator.Create();
            return optimizator.GetOptimalParameters(points, dimension, radiusNeighbour, barrier, isCheckedPass);
        }
    }
}