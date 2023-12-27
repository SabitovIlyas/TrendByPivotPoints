using System.Collections.Generic;
using TSLab.DataSource;
using TSLab.Script;
using TSLab.Script.Handlers;
using Math = System.Math;

namespace TradingSystems
{
    class StopLoss : IStopLoss
    {
        public static StopLoss Create(string parametersCombination, Security security, PositionSide positionSide, RealTimeTrading realTimeTrading, IList<double> atr)
        {
            return new StopLoss(parametersCombination, security, positionSide, realTimeTrading, atr);
        }

        public IContext ctx { get; set; }
        public bool IsStopLossUpdateWhenBarIsClosedOnly { get; set; } = false;
        public Logger Logger { get; set; } = new NullLogger();

        private string name = "TrailingStopLoss";
        private Security security;
        private PositionSide positionSide;
        private double breakdownLong = 0;
        private double stopLossLong;
        private string stopLossDescription = "TrailingStopLoss";
        private IList<double> atr;
        private double pivotPointBreakDownSide;
        private RealTimeTrading realTimeTrading;
        private Converter convertable;
        private string signalNameForClosePosition = "";
        private double firstStopLoss;
        private IPosition le;
        private int currentBarNumber;
        private bool isStopLossActive = false;
        private double previousMaxPrice;
        private double maxPrice;            

        private StopLoss(string parametersCombination, Security security, PositionSide positionSide, RealTimeTrading realTimeTrading, IList<double> atr)
        {
            this.security = security;
            this.positionSide = positionSide;
            stopLossDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
            this.realTimeTrading = realTimeTrading;
            this.atr = atr;
            this.pivotPointBreakDownSide = pivotPointBreakDownSide;

            if (positionSide == PositionSide.Long)
            {
                signalNameForClosePosition = "LXS";
                convertable = new Converter(isConverted: false);
            }
            if (positionSide == PositionSide.Short)
            {
                signalNameForClosePosition = "SXS";
                convertable = new Converter(isConverted: true);
            }
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

        public void CreateStopLoss(double stopLoss)
        {
            stopLossLong = stopLoss;
            maxPrice = 0;
            previousMaxPrice = 0;
            isStopLossActive = false;

            var stopLossName = string.Format("StopLossTrailPivotPoints {0} {1}", security.Name, positionSide);
            Log("Сохраним stopLoss = {0} в контейнере \"{1}\".", stopLossLong, stopLossName);

            realTimeTrading.SaveObjectToContainer(stopLossName, stopLossLong);

            Log("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, stopLossName);
            try
            {
                var containerValue = (double)realTimeTrading.LoadObjectFromContainer(stopLossName);
                Logger.Log("stopLoss сохранился в контейнере. Значение в контейнере: value = {0}.", containerValue);
            }
            catch (System.Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        public void UpdateStopLossLongPosition(int barNumber, IPosition le, double stopLoss)
        {
            this.le = le;            
            currentBarNumber = barNumber;            

            Log("Обновляем стоп...");

            Log("Проверяем актуальный ли это бар.");
            if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
            {
                Log("Бар актуальный.");

                var stopLossName = string.Format("stopLoss {0} {1}", security.Name, positionSide);
                Log("Загружаем stopLoss из контейнера \"{0}\".", stopLossName);

                try
                {
                    var containerValue = (double)realTimeTrading.LoadObjectFromContainer(stopLossName);

                    Log("Загрузили контейнер.");
                    var value = containerValue;
                    Log("Значение в контейнере: value = {0}", value);
                    if (value != 0d)
                    {
                        Log("Записываем в stopLoss ненулевое значение из контейнера.");
                        stopLossLong = value;
                    }
                    else
                        Log("Значение в контейнере равно нулю! Значение из контейнера отбрасываем!");

                }
                catch (System.Exception e)
                {
                    Logger.Log(e.Message);
                }
            }
            else
                Log("Бар не актуальный.");

            if (true||convertable.IsGreater(stopLoss, stopLossLong))
            {
                Log("Да, новый стоп-лосс ({0}) выше прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossLong);
               
                stopLossLong = stopLoss;

                Log("Проверяем актуальный ли это бар.");
                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
                {
                    Log("Бар актуальный.");

                    var stopLossName = string.Format("StopLossTrailPivotPoints {0} {1}", security.Name, positionSide);
                    Log("Сохраним stopLoss = {0} в контейнере \"{1}\".", stopLossLong, stopLossName);

                    realTimeTrading.SaveObjectToContainer(stopLossName, stopLossLong);

                    Log("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, stopLossName);
                    try
                    {
                        var containerValue = (double)realTimeTrading.LoadObjectFromContainer(stopLossName);
                        Logger.Log("stopLoss сохранился в контейнере. Значение в контейнере: value = {0}.", containerValue);
                    }
                    catch (System.Exception e)
                    {
                        Logger.Log(e.Message);
                    }
                }
                else
                    Log("Бар не актуальный.");
            }
            else
            {
                Log("Нет, новый стоп-лосс ({0}) не выше прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossLong);
            }

            if (!ctx.IsOptimization && realTimeTrading.WasNewPositionOpened())
            {
                Log("Открыта новая необработанная позиция. Устанавливаем стоп-лосс на текущем баре.");
                le.CloseAtStop(barNumber, stopLossLong, atr[barNumber], signalNameForClosePosition);
                Log("Позицию обработали, поэтому сбрасываем признак того, что была открыта новая позиция");
                realTimeTrading.ResetFlagNewPositionOpened();
            }
            else
            {
                Log("Необработанной позиции нет. Устанавливаем стоп-лосс на следующем баре.");

                Log("Обновлять стоп-лосс только при закрытии бара?");
                if (IsStopLossUpdateWhenBarIsClosedOnly)
                {
                    Log("Да.");

                    Log("Устанавливаем необновлённое значение стоп-лосса.");
                    le.CloseAtStop(barNumber, stopLossLong, atr[barNumber], signalNameForClosePosition);
                }
                else
                {
                    Log("Нет.");

                    Log("Устанавливаем обновлённое значение стоп-лосса.");
                    le.CloseAtStop(barNumber, stopLossLong, atr[barNumber], signalNameForClosePosition);
                }
            }
        }
    }
}
