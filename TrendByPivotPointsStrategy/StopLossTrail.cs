//using System.Collections.Generic;
//using TSLab.DataSource;
//using TSLab.Script;
//using TSLab.Script.Handlers;
//using Math = System.Math;

//namespace TrendByPivotPointsStrategy
//{
//    class StopLossTrail : IStopLoss
//    {
//        public static StopLossTrail Create(string parametersCombination, Security security, PositionSide positionSide, IList<double> atr, double pivotPointBreakDownSide, RealTimeTrading realTimeTrading)
//        {
//            return new StopLossTrail(parametersCombination, security, positionSide, atr, pivotPointBreakDownSide, realTimeTrading);
//        }

//        public IContext ctx { get; set; }
//        public bool IsStopLossUpdateWhenBarIsClosedOnly { get; set; } = true;
//        public Logger Logger { get; set; } = new NullLogger();

//        private string name = "TrailingStopLoss";
//        private Security security;
//        private PositionSide positionSide;
//        private double breakdownLong = 0;
//        private double stopLossLong;
//        private string stopLossDescription = "TrailingStopLoss";
//        private IList<double> atr;
//        private double pivotPointBreakDownSide;
//        private RealTimeTrading realTimeTrading;
//        private Converter convertable;
//        private string signalNameForClosePosition = "";
//        private double firstStopLoss;
//        private IPosition le;

//        private StopLossTrail(string parametersCombination, Security security, PositionSide positionSide, IList<double> atr, double pivotPointBreakDownSide, RealTimeTrading realTimeTrading)
//        {
//            this.security = security;            
//            this.positionSide = positionSide;
//            stopLossDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);
//            this.realTimeTrading = realTimeTrading;
//            this.atr = atr;
//            this.pivotPointBreakDownSide = pivotPointBreakDownSide;

//            if (positionSide == PositionSide.Long)
//            {
//                signalNameForClosePosition = "LXS";
//                convertable = new Converter(isConverted: false);
//            }
//            if (positionSide == PositionSide.Short)
//            {
//                signalNameForClosePosition = "SXS";
//                convertable = new Converter(isConverted: true);
//            }
//        }

//        private void Log(string text)
//        {
//            Logger.Log("{0}: {1}", stopLossDescription, text);
//        }

//        private void Log(string text, params object[] args)
//        {
//            text = string.Format(text, args);
//            Log(text);
//        }

//        public void CreateStopLoss(double stopLoss, double breakdown)
//        {
//            firstStopLoss = stopLoss;
//            stopLossLong = stopLoss;
//            this.breakdownLong = breakdown;
//            var containerName = string.Format("stopLoss {0} {1}", security.Name, positionSide);
//            Log("Сохраним stopLoss = {0} в контейнере \"{1}\".", stopLoss, containerName);
//            var container = new NotClearableContainer<double>(stopLoss);

//            ctx.StoreObject(containerName, container);
//            Log("Проверим, сохранился ли stopLoss = {0} в контейнере \"{1}\".", stopLoss, containerName);

//            container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
//            double value = 0d;
//            if (container != null)
//                value = container.Content;

//            if (value != 0d)
//                if (value == stopLoss)
//                    Log("stopLoss сохранился в контейнере. Значение в контейнере: value = {0}.", value);

//                else
//                    Log("stopLoss НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value);
//        }

//        private bool Check()
//        {
//            var pathPrice = Math.Abs(le.EntryPrice - firstStopLoss);
//        }


//        public void UpdateStopLossLongPosition(int barNumber, double maxPrice, IPosition le)
//        {
//            this.le = le;
//            var previousStopLoss = stopLossLong;
//            Log("Обновляем стоп...");      

//            //le.EntryPrice

            

//            //var stopLoss = convertable.Minus(lastLow.Value, breakdownLong);                        

//            Log("Проверяем актуальный ли это бар.");
//            if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
//            {
//                Log("Бар актуальный.");

//                var containerName = string.Format("stopLoss {0} {1}", security.Name, positionSide);
//                Log("Загружаем stopLoss из контейнера \"{0}\".", containerName);
//                var container = ctx.LoadObject(containerName) as NotClearableContainer<double>;

//                if (container != null)
//                {
//                    Log("Загрузили контейнер.");
//                    var value = container.Content;
//                    Log("Значение в контейнере: value = {0}", value);
//                    if (value != 0d)
//                    {
//                        Log("Записываем в stopLoss ненулевое значение из контейнера.");
//                        stopLossLong = value;
//                    }
//                    else
//                        Log("Значение в контейнере равно нулю! Значение из контейнера отбрасываем!");
//                }
//                else
//                    Log("Не удалось загрузить контейнер.");
//            }
//            else
//                Log("Бар не актуальный.");

//            if (convertable.IsGreater(stopLoss, stopLossLong))
//            {
//                Log("Да, новый стоп-лосс ({0}) выше прежнего ({1}). Обновляем стоп-лосс.", stopLoss, stopLossLong);
//                previousStopLoss = stopLossLong;
//                stopLossLong = stopLoss;

//                Log("Проверяем актуальный ли это бар.");
//                if (security.IsRealTimeActualBar(barNumber) || (security.RealTimeActualBarNumber == (barNumber + 1)))
//                {
//                    Log("Бар актуальный.");

//                    var containerName = string.Format("stopLoss {0} {1}", security.Name, positionSide);
//                    Log("Сохраним stopLoss = {0} в контейнере \"{1}\".", stopLossLong, containerName);
//                    var container = new NotClearableContainer<double>(stopLossLong);

//                    ctx.StoreObject(containerName, container);
//                    Log("Проверим, сохранился ли stopLossLong = {0} в контейнере \"{1}\".", stopLossLong, containerName);

//                    container = ctx.LoadObject(containerName) as NotClearableContainer<double>;
//                    double value = 0d;
//                    if (container != null)
//                        value = container.Content;

//                    if (value != 0d)
//                        if (value == stopLossLong)
//                            Logger.Log("stopLoss сохранился в контейнере. Значение в контейнере: value = {0}.", value);

//                        else
//                            Logger.Log("stopLoss НЕ сохранился в контейнере! Значение в контейнере: value = {0}.", value);
//                }
//                else
//                    Log("Бар не актуальный.");


//            }
//            else
//            {
//                Log("Нет, новый стоп-лосс ({0}) не выше прежнего ({1}). Стоп-лосс оставляем прежним.", stopLoss, stopLossLong);
//            }

//            if (realTimeTrading.WasNewPositionOpened())
//            {
//                Log("Открыта новая необработанная позиция. Устанавливаем стоп-лосс на текущем баре.");
//                le.CloseAtStop(barNumber, stopLossLong, atr[barNumber], signalNameForClosePosition);
//                Log("Позицию обработали, поэтому сбрасываем признак того, что была открыта новая позиция");
//                realTimeTrading.ResetFlagNewPositionOpened();
//            }
//            else
//            {
//                Log("Необработанной позиции нет. Устанавливаем стоп-лосс на следующем баре.");

//                Log("Обновлять стоп-лосс только при закрытии бара?");
//                if (IsStopLossUpdateWhenBarIsClosedOnly)
//                {
//                    Log("Да.");

//                    Log("Устанавливаем необновлённое значение стоп-лосса.");
//                    le.CloseAtStop(barNumber, previousStopLoss, atr[barNumber], signalNameForClosePosition);
//                }
//                else
//                {
//                    Log("Нет.");

//                    Log("Устанавливаем обновлённое значение стоп-лосса.");
//                    le.CloseAtStop(barNumber, stopLossLong, atr[barNumber], signalNameForClosePosition);
//                }
//            }
//        }
//    }
//}
