﻿using System;
using System.IO;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class GlobalCacheManagerTest : IExternalScriptMultiSec
    {       
        public void Execute(IContext context, ISecurity[] securities)
        {
            var logger = new LoggerSystem(context);

            logger.Log("Попробую записать информацию в текстовый файла.");
            //var path = System.IO.Directory.GetCurrentDirectory();
            //logger.Log("Текущая директория: {0}.", path);
            var folder = @"C:\Users\Ильяс\Documents\Трейдинг\Обмен между скриптами\";
            var file = "testIO.txt";
            var path = Path.Combine(folder, file);
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(DateTime.Now + ": 1-ая тестовая строка");
            sw.WriteLine(DateTime.Now + ": 2-ая тестовая строка");
            sw.Close();
        }
    }
}