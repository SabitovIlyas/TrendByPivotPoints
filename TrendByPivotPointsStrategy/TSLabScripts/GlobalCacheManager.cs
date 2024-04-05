using System.IO;
using System.Xml.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Optimization;

namespace TradingSystems
{
    public class GlobalCacheManager : IExternalScriptMultiSec
    {
        public OptimProperty mode = new OptimProperty(0, 0, 1, 1);
        public OptimProperty isLoggerOn = new OptimProperty(1, 0, 1, 1);
        public void Execute(IContext context, ISecurity[] securities)
        {
            var logger = new TsLabLogger(context);

            switch ((int)mode)
            {                
                case 1:
                    {
                        logger.Log("Менеджер глобального кеша находится в режиме редактирования.");
                        break;
                    }
                default:
                    {
                        logger.Log("Менеджер глобального кеша находится в режиме чтения.");
                        logger.Log("Попробую прочитать информацию из текстового файла.");
                        //var path = System.IO.Directory.GetCurrentDirectory();
                        //logger.Log("Текущая директория: {0}.", path);
                        var folder = @"C:\Users\Ильяс\Documents\Трейдинг\Обмен между скриптами\";
                        var file = "testIO.txt";
                        var path = Path.Combine(folder, file);
                        if (!File.Exists(path))
                        {
                            logger.Log("Файл не найден!");
                            break;
                        }
                                                
                        string[] listStrings = File.ReadAllLines(path);
                        
                        if (listStrings == null)
                        {
                            logger.Log("Файл пустой!");
                            break;
                        }

                        logger.Log("Вывод содержимого файла\r\n=====================!");
                        foreach (string str in listStrings)
                            logger.Log(str);
                        logger.Log("======================!\r\nКонец файла!");

                        break;
                    }
            }
        }
    }
}