using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class StopLoss
    {
        public Logger Logger { get; set; } = new NullLogger();

        private void Log(string text)
        {
            Logger.Log("{0}: {1}", tradingSystemDescription, text);
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
                Logger.Log("Это условие никогда не должно отрабатывать. Проверить и впоследствии убрать.");
                return;
            }

            var stopLoss = lastLow.Value - breakdownLong;

            textForLog = string.Format("ATR = {0}; допустимый уровень пробоя в % от ATR = {1}; допустимый уровень пробоя = {2};" +
                            "стоп-лосс = последний мимнимум {3} - допустимый уровень пробоя {2} = {4}. Новый стоп-лосс выше прежнего?", atr[barNumber], pivotPointBreakDownSide,
                            breakdownLong, lastLow.Value, stopLoss);
            Log(textForLog);

            Log("Проверяем актуальный ли это бар.");
            if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
            {
                Log("Бар актуальный.");

                var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                Log(string.Format("Загружаем stopLossLong из контейнера \"{0}\".", containerName));
                var container = ctx.LoadObject(containerName) as NotClearableContainer<double>;

                if (container != null)
                {
                    Log("Загрузили контейнер.");
                    var value = container.Content;
                    Log(string.Format("Значение в контейнере: value = {0}", value));
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
                textForLog = string.Format("Да, новый стоп-лосс ({0}) выше прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossLong);

                Log(textForLog);
                stopLossLong = stopLoss;

                Log("Проверяем актуальный ли это бар.");
                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                {
                    Log("Бар актуальный.");

                    var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
                    Log(string.Format("Сохраним stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));
                    var container = new NotClearableContainer<double>(stopLossLong);

                    ctx.StoreObject(containerName, container);
                    Log(string.Format("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));

                    container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
                    double value = 0d;
                    if (container != null)
                        value = container.Content;

                    if (value != 0d)
                        if (value == stopLossLong)
                            Logger.Log(string.Format("stopLossLong сохранился в контейнере. Значение в контейнере: value = {0}.", value));

                        else
                            Logger.Log(string.Format("stopLossLong НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value));
                }
                else                
                    Log("Бар не актуальный.");
                

            }
            else
            {
                textForLog = string.Format("Нет, новый стоп-лосс ({0}) не выше прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossLong);
                Log(textForLog);
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

        private void CreateStopLossLong()
        {
            var containerName = string.Format("stopLossLong {0} {1}", security.Name, positionSide);
            Logger.Log(string.Format("Сохраним stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));
            var container = new NotClearableContainer<double>(stopLossLong);

            ctx.StoreObject(containerName, container);
            Logger.Log(string.Format("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName));

            container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
            double value = 0d;
            if (container != null)
                value = container.Content;

            if (value != 0d)
                if (value == stopLossLong)
                    Logger.Log(string.Format("stopLossLong сохранился в контейнере. Значение в контейнере: value = {0}.", value));

                else
                    Logger.Log(string.Format("stopLossLong НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value));
        }
    }
}