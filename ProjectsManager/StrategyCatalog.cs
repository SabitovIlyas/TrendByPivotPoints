namespace ProjectsManager
{
    /// <summary>Диапазон поиска одного параметра стратегии.</summary>
    public record ParameterRangeInfo(string Name, double Min, double Max, double Step);

    /// <summary>
    /// Каталог стратегий для GUI: имена и диапазоны параметров по умолчанию.
    /// Значения должны совпадать с описаниями стратегий в TrendByPivotPointsOptimizator
    /// (DonchianStrategyDefinition, MeanReversionStrategyDefinition) — файл настроек
    /// переопределяет их строками Range, поэтому расхождение влияет только на
    /// значения, показанные в таблице по умолчанию.
    /// </summary>
    public static class StrategyCatalog
    {
        public static readonly string[] Strategies = { "MeanReversion", "DonchianUniversal" };
        public static readonly string[] Sides = { "Long", "Short" };
        public static readonly string[] TimeFrames = { "01min", "05min", "15min", "30min", "60min", "1d" };

        public static List<ParameterRangeInfo> GetDefaultParameters(string strategy, string side)
        {
            switch (strategy)
            {
                case "MeanReversion":
                    //Пороги RSI ищутся в общем диапазоне для обеих сторон: их взаимный
                    //порядок наводит Repair в MeanReversionStrategyDefinition.
                    return new List<ParameterRangeInfo>
                    {
                        new("maPeriod", 50, 250, 1),
                        new("rsiEntryPeriod", 5, 50, 1),
                        new("rsiExitPeriod", 5, 50, 1),
                        new("atrPeriod", 5, 50, 1),
                        new("rsiEntryLevel", 5, 95, 1),
                        new("rsiExitLevel", 5, 95, 1),
                        new("atrMultiplier", 0.5, 3.0, 0.5),
                        new("useTrailingStop", 0, 1, 1),
                        //0 — уровень, 1 — разворот, 2 — вход в зону экстремума.
                        new("rsiEntryMode", 0, 2, 1),
                        //0 — выключен, 1 — цель достигнута, 2 — движение угасло, 3 — уровень.
                        new("rsiExitMode", 0, 3, 1),
                    };
                case "DonchianUniversal":
                    return new List<ParameterRangeInfo>
                    {
                        new("fastDonchian", 10, 100, 1),
                        new("slowDonchian", 10, 200, 1),
                        new("atrPeriod", 2, 24, 1),
                        new("limitOpenedPositions", 1, 4, 1),
                        new("kAtrForOpenPosition", 0.5, 3.0, 0.5),
                        new("kAtrForStopLoss", 0.5, 3.0, 0.5),
                        //0 — не закрывать по времени.
                        new("maxBarsInPosition", 0, 800, 10),
                        //0 — выход только по ATR-стопу, без канального.
                        new("useChannelExit", 0, 1, 1),
                    };
                default:
                    return new List<ParameterRangeInfo>();
            }
        }
    }
}
