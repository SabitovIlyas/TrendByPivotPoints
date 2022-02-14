using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class StopLoss
    {
        public static StopLoss Create(string parametersCombination, Security security, PositionSide positionSide)
        {
            return new StopLoss(parametersCombination, security, positionSide);
        }
        public Logger Logger { get; set; } = new NullLogger();

        public IContext ctx { get; set; }
        private string name = "StopLossUnderLow";
        private string parametersCombination = "StopLossUnderLow";
        private Security security;
        private PositionSide positionSide;
        private double breakdownLong = 0;
        private double stopLossLong;
        private string stopLossDescription = "StopLossUnderLow";

        private StopLoss(string parametersCombination, Security security, PositionSide positionSide)
        {
            this.security = security;
            this.parametersCombination = parametersCombination;
            this.positionSide = positionSide;
            stopLossDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
        }

        private void Log(string text)
        {
            Logger.Log("{0}: {1}", stopLossDescription, text);
        }

        private void Log(string text, params object[] args)
        {
            text = string.Format(text, args);
            Log(text);
        }

        public void UpdateStopLossLongPosition(int barNumber, List<Indicator> lows, Indicator lastLow, IPosition le)
        {
            Log("Обновляем стоп...");

            if (lows.Count == 0)
            {
                Log("Это условие никогда не должно отрабатывать. Проверить и впоследствии убрать.");
                return;
            }

            var stopLoss = lastLow.Value - breakdownLong;

            Log("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                            "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Новый стоп-лосс выше прежнего?", atr[barNumber], pivotPointBreakDownSide,
                            breakdownLong, lastLow.Value, stopLoss);            

            Log("Проверяем актуальный ли это бар.");
            if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
            {
                Log("Бар актуальный.");

                var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                Log("Загружаем stopLossLong из контейнера \"{0}\".", containerName);
                var container = ctx.LoadObject(containerName) as NotClearableContainer<double>;

                if (container != null)
                {
                    Log("Загрузили контейнер.");
                    var value = container.Content;
                    Log("Значение в контейнере: value = {0}", value);
                    if (value != 0d)
                    {
                        Log("Записываем в stopLossLong ненулевое значение из контейнера.");
                        stopLossLong = value;
                    }
                    else
                        Log("Значение в контейнере равно нулю! Значение из контейнера отбрасываем!");                    
                }
                else                
                    Log("Не удалось загрузить контейнер.");                
            }
            else            
                Log("Бар не актуальный.");            

            if (stopLoss > stopLossLong)
            {
                Log("Да, новый стоп-лосс ({0}) выше прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossLong);
                
                stopLossLong = stopLoss;

                Log("Проверяем актуальный ли это бар.");
                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                {
                    Log("Бар актуальный.");

                    var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                    Log("Сохраним stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName);
                    var container = new NotClearableContainer<double>(stopLossLong);

                    ctx.StoreObject(containerName, container);
                    Log("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName);

                    container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
                    double value = 0d;
                    if (container != null)
                        value = container.Content;

                    if (value != 0d)
                        if (value == stopLossLong)
                            Logger.Log("stopLossLong сохранился в контейнере. Значение в контейнере: value = {0}.", value);

                        else
                            Logger.Log("stopLossLong НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value);
                }
                else                
                    Log("Бар не актуальный.");
                

            }
            else
            {
                Log("Нет, новый стоп-лосс ({0}) не выше прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossLong);                
            }

            if (WasNewPositionOpened())
            {
                Log("Открыта новая позиция. Устанавливаем стоп-лосс на текущем баре.");
                le.CloseAtStop(barNumber, stopLossLong, atr[barNumber], "LXS");
            }
            else
            {
                Log("Новая позиция не открыта. Устанавливаем стоп-лосс на следующем баре.");
                le.CloseAtStop(barNumber + 1, stopLossLong, atr[barNumber], "LXS");
            }
        }

        public void CreateStopLossLong()
        {
            var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
            Log("Сохраним stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName);
            var container = new NotClearableContainer<double>(stopLossLong);

            ctx.StoreObject(containerName, container);
            Log(string.Format("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName);

            container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
            double value = 0d;
            if (container != null)
                value = container.Content;

            if (value != 0d)
                if (value == stopLossLong)
                    Log("stopLossLong сохранился в контейнере. Значение в контейнере: value = {0}.", value);

                else
                    Log("stopLossLong НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value);
        }
    }
}