using System;
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
                        logger.Log("Менеджер глобального кеша находится в режиме редактирования");
                        break;
                    }
                default:
                    {
                        logger.Log("Менеджер глобального кеша находится в режиме чтения");
                        break;
                    }
            }
        }
    }
}