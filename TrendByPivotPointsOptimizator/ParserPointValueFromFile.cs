using System;
using System.Collections.Generic;
using TSLab.Utils;

namespace TrendByPivotPointsOptimizator
{
    partial class Program
    {
        public class ParserPointValueFromFile
        {
            string fullFileName = string.Empty;

            public string Param1Str { get; set; } = "ВнешнийСкрипт.period";
            public string Param2Str { get; set; } = "ВнешнийСкрипт.rsiBand";

            public static ParserPointValueFromFile Create(string fullFileName)
            {
                return new ParserPointValueFromFile(fullFileName);
            }

            private ParserPointValueFromFile(string fullFileName)
            {
                this.fullFileName = fullFileName;
            }

            public List<PointValue> ParseForPoints()
            {
                var points = new List<PointValue>();
                string line;
                try
                {
                    if (!System.IO.File.Exists(fullFileName))
                        throw new Exception("Файл не найден!");

                    string[] listStrings = System.IO.File.ReadAllLines(fullFileName);

                    if (listStrings == null)
                        throw new Exception("Файл пустой!");

                    int recoveryFactorIndex = 0, param1 = 0, param2 = 0, stringNumber = 0;

                    foreach (string str in listStrings)
                    {
                        line = str;
                        if (!string.IsNullOrEmpty(str))
                        {
                            string[] split = str.Split(';');

                            if (stringNumber == 0)
                            {
                                recoveryFactorIndex = split.FindIndex(p => p.Contains("Фикс. Фактор восст."));// TODO: Переработать этот хардкод
                                param1 = split.FindIndex(p => p.Contains(Param1Str));
                                param2 = split.FindIndex(p => p.Contains(Param2Str));
                            }

                            if (stringNumber > 0)
                            {
                                var tmpStr = split[recoveryFactorIndex].Replace('.', ',');
                                var recoveryFactor = double.MinValue;

                                if (!tmpStr.ToLower().Contains("e"))
                                    recoveryFactor = double.Parse(tmpStr);

                                else if (tmpStr[0] == '+')
                                    recoveryFactor = double.MaxValue;

                                tmpStr = split[param1].Replace('.', ',');
                                var doubleTmp = double.Parse(tmpStr);
                                var period = (int)doubleTmp;

                                tmpStr = split[param2].Replace('.', ',');
                                doubleTmp = double.Parse(tmpStr);
                                var rsiBand = (int)doubleTmp;

                                points.Add(PointValue.Create(recoveryFactor, new int[2] { period, rsiBand }));
                            }
                            stringNumber++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return points;
            }
        }
    }
}