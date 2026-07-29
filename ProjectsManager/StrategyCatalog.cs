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
                    //Пороги RSI зависят от стороны: у лонга и шорта свои независимые диапазоны.
                    var isShort = side == "Short";
                    return new List<ParameterRangeInfo>
                    {
                        new("maPeriod", 50, 250, 1),
                        new("rsiEntryPeriod", 5, 50, 1),
                        new("rsiExitPeriod", 5, 50, 1),
                        new("atrPeriod", 5, 50, 1),
                        new("rsiEntryLevel", isShort ? 50 : 5, isShort ? 95 : 50, 1),
                        new("rsiExitLevel", isShort ? 5 : 50, isShort ? 50 : 95, 1),
                        new("atrMultiplier", 0.5, 3.0, 0.5),
                        new("useTrailingStop", 0, 1, 1),
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
                    };
                default:
                    return new List<ParameterRangeInfo>();
            }
        }
    }
}
