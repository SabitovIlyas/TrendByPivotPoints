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
            var logger = new LoggerSystem(context);

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
                        var path = System.IO.Directory.GetCurrentDirectory();
                        logger.Log("Текущая директория: {0}.", path);
                        
                        var file = "testIO.txt";
                        if (!System.IO.File.Exists(file))
                        {
                            logger.Log("Файл не найден!");
                            break;
                        }
                                                
                        string[] listStrings = System.IO.File.ReadAllLines(file);
                        
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