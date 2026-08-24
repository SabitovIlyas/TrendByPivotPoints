using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TSLab.DataSource;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;

namespace TradingSystems
{
    /// <summary>
    /// Стратегия возврата к среднему: вход по экстремуму RSI с фильтром тренда по SMA,
    /// выход по обратному пересечению RSI либо по ATR-стопу. Все сигналы считаются по
    /// закрытому бару, ордера исполняются на открытии следующего бара.
    /// Робот торгует только одну сторону (positionSide); у каждой стороны свои,
    /// никак не связанные пороги rsiEntryLevel/rsiExitLevel:
    /// лонг — вход при RSI ниже уровня, выход при пересечении уровня снизу вверх;
    /// шорт — вход при RSI выше уровня, выход при пересечении уровня сверху вниз.
    /// </summary>
    public class TradingSystemMeanReversion : TradingSystem
    {
        private IList<double> sma;
        private IList<double> rsiEntry;
        private IList<double> rsiExit;
        private IList<double> atr;

        private int maPeriod;
        private int rsiEntryPeriod;
        private int rsiExitPeriod;
        private int atrPeriod;
        private int rsiEntryLevel;
        private int rsiExitLevel;
        private int rsiEntryMode;
        private int rsiExitMode;
        private double atrMultiplier;
        private bool useTrailingStop;

        /// <summary>Вход: RSI по одну сторону от уровня (прежнее поведение).</summary>
        public const int EntryModeLevel = 0;

        /// <summary>Вход: RSI уходит от экстремума — лонг пересекает уровень снизу
        /// вверх, шорт сверху вниз. Разворот подтверждён.</summary>
        public const int EntryModeReversal = 1;

        /// <summary>Вход: RSI входит в зону экстремума — лонг пересекает уровень
        /// сверху вниз, шорт снизу вверх.</summary>
        public const int EntryModeEnterZone = 2;

        /// <summary>Выход по RSI выключен: закрывает только стоп.</summary>
        public const int ExitModeOff = 0;

        /// <summary>Выход: RSI дошёл до цели — лонг пересекает уровень снизу вверх,
        /// шорт сверху вниз (прежнее поведение).</summary>
        public const int ExitModeTargetReached = 1;

        /// <summary>Выход: движение угасло — лонг пересекает уровень сверху вниз,
        /// шорт снизу вверх.</summary>
        public const int ExitModeFaded = 2;

        /// <summary>Выход: RSI просто по нужную сторону уровня, без пересечения.
        /// Единственный режим, которому безразлично состояние на входе.</summary>
        public const int ExitModeLevel = 3;

        private double fixedAtr;
        private double trailingStopPrice;
        private int digitsAfterPoint = 0;

        public TradingSystemMeanReversion(List<Security> securities,
            ContractsManager contractsManager, Indicators indicators, Context context,
            Logger logger, List<NonTradingPeriod> nonTradingPeriods = null) :
            base(securities, contractsManager, indicators, context, logger, nonTradingPeriods)
        {
            name = "MeanReversion";
            digitsAfterPoint = CountDecimalPlaces(security.Bars.Last().Close);
        }

        public void SetParameters(SystemParameters systemParameters)
        {
            maPeriod = (int)systemParameters.GetValue("maPeriod");
            rsiEntryPeriod = (int)systemParameters.GetValue("rsiEntryPeriod");
            rsiExitPeriod = (int)systemParameters.GetValue("rsiExitPeriod");
            atrPeriod = (int)systemParameters.GetValue("atrPeriod");
            rsiEntryLevel = (int)systemParameters.GetValue("rsiEntryLevel");
            rsiExitLevel = (int)systemParameters.GetValue("rsiExitLevel");
            atrMultiplier = (double)systemParameters.GetValue("atrMultiplier");
            useTrailingStop = (int)systemParameters.GetValue("useTrailingStop") == 1;

            //Режимы появились позже уровней: без них работаем как раньше.
            rsiEntryMode = GetIntOrDefault(systemParameters, "rsiEntryMode", EntryModeLevel);
            rsiExitMode = GetIntOrDefault(systemParameters, "rsiExitMode", ExitModeTargetReached);

            //Одновременно открыта только одна позиция, пирамидирование не используется.
            limitOpenedPositions = 1;

            var pSide = (int)systemParameters.GetValue("positionSide");
            if (pSide == 0)
                positionSide = PositionSide.Long;
            else if (pSide == 1)
                positionSide = PositionSide.Short;
            else
                positionSide = PositionSide.Null;

            parametersCombination = string.Format("maPeriod: {0}; rsiEntryPeriod: {1}; " +
                "rsiExitPeriod: {2}; atrPeriod: {3}; rsiEntryLevel: {4}; rsiExitLevel: {5}; " +
                "atrMultiplier: {6}; useTrailingStop: {7}; rsiEntryMode: {8}; " +
                "rsiExitMode: {9}", maPeriod, rsiEntryPeriod,
                rsiExitPeriod, atrPeriod, rsiEntryLevel, rsiExitLevel, atrMultiplier,
                useTrailingStop, rsiEntryMode, rsiExitMode);
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name,
                parametersCombination, security.Name, positionSide);
        }

        public override void CalculateIndicators()
        {
            nonTradingPeriod = Math.Max(Math.Max(maPeriod, atrPeriod),
                Math.Max(rsiEntryPeriod, rsiExitPeriod));

            var closes = security.Bars.Select(b => b.Close).ToList();
            sma = Series.SMA(closes, maPeriod);
            rsiEntry = Series.RSI(closes, rsiEntryPeriod);
            rsiExit = Series.RSI(closes, rsiExitPeriod);

            var candles = ConvertBarsForUsingInTsLabIndicators();
            atr = Series.AverageTrueRange(candles, atrPeriod);
        }

        protected override void CheckPositionOpenLongCase(int positionNumber)
        {
            var notes = " Вход №" + (positionNumber + 1);

            if (!IsPositionOpen(notes))
                CheckEntry(notes);
            else
                ManageOpenedPosition(notes);
        }

        private void CheckEntry(string notes)
        {
            Log("бар № {0}. Позиция не открыта. Проверяем условия входа.", barNumber);

            var close = security.GetBarClose(barNumber);
            var rsi = rsiEntry[barNumber];

            var rsiPrevious = rsiEntry[barNumber - 1];
            var isRsiExtreme = IsEntrySignal(rsiPrevious, rsi);

            //Фильтр тренда: лонг — закрытие выше SMA, шорт — ниже (через converter).
            var isTrendFilterPassed = converter.IsGreater(close, sma[barNumber]);

            Log("RSI = {0} (предыдущий {1}, порог {2}, режим входа {3}); закрытие {4} {5} " +
                "SMA = {6}", rsi, rsiPrevious, rsiEntryLevel, rsiEntryMode, close,
                converter.Above, sma[barNumber]);

            if (!isRsiExtreme || !isTrendFilterPassed)
                return;

            //ATR фиксируется на баре сигнала — стоп не пересчитывается, если только
            //не включён трейлинг.
            fixedAtr = Math.Round(atr[barNumber], digitsAfterPoint);
            var stopPrice = GetStopPriceFromEntry(close);
            trailingStopPrice = stopPrice;

            Log("Сигнал на вход. Фиксированный ATR = {0}. Стоп-цена = {1}.", fixedAtr,
                stopPrice);

            var contracts = contractsManager.GetQntContracts(security, close, stopPrice,
                positionSide);
            if (contracts <= 0)
                return;

            if (positionSide == PositionSide.Long)
                security.BuyAtMarket(barNumber + 1, contracts,
                    signalNameForOpenPosition + notes);
            if (positionSide == PositionSide.Short)
                security.SellAtMarket(barNumber + 1, contracts,
                    signalNameForOpenPosition + notes);
        }

        private static int GetIntOrDefault(SystemParameters systemParameters, string key,
            int defaultValue)
        {
            return systemParameters.TryGetValue(key, out object value)
                ? (int)value : defaultValue;
        }

        /// <summary>
        /// Сигнал входа по RSI. Режимы описаны зеркально: то, что для лонга «вверх»,
        /// для шорта «вниз», поэтому у обеих сторон один и тот же смысл режима.
        /// </summary>
        private bool IsEntrySignal(double previous, double current)
        {
            var isLong = positionSide == PositionSide.Long;

            switch (rsiEntryMode)
            {
                case EntryModeReversal:
                    return isLong
                        ? previous <= rsiEntryLevel && current > rsiEntryLevel
                        : previous >= rsiEntryLevel && current < rsiEntryLevel;

                case EntryModeEnterZone:
                    return isLong
                        ? previous >= rsiEntryLevel && current < rsiEntryLevel
                        : previous <= rsiEntryLevel && current > rsiEntryLevel;

                default:
                    return isLong ? current < rsiEntryLevel : current > rsiEntryLevel;
            }
        }

        /// <summary>
        /// Сигнал выхода по RSI. Режим ExitModeOff оставляет позицию стопу: это не
        /// поломка, а осознанный вариант — на золоте в лонг оптимизатор шестнадцать
        /// раз подряд задирал порог выхода в недостижимую зону, добиваясь того же
        /// самого окольным путём.
        /// </summary>
        private bool IsExitSignal(double previous, double current)
        {
            var isLong = positionSide == PositionSide.Long;

            switch (rsiExitMode)
            {
                case ExitModeOff:
                    return false;

                case ExitModeFaded:
                    return isLong
                        ? previous > rsiExitLevel && current <= rsiExitLevel
                        : previous < rsiExitLevel && current >= rsiExitLevel;

                case ExitModeLevel:
                    return isLong ? current >= rsiExitLevel : current <= rsiExitLevel;

                default:
                    return isLong
                        ? previous < rsiExitLevel && current >= rsiExitLevel
                        : previous > rsiExitLevel && current <= rsiExitLevel;
            }
        }

        private double GetStopPriceFromEntry(double entryPrice)
        {
            return converter.Minus(entryPrice, Math.Round(atrMultiplier * fixedAtr,
                digitsAfterPoint));
        }

        private void ManageOpenedPosition(string notes)
        {
            var position = GetOpenedPosition(notes);

            var rsiPrevious = rsiExit[barNumber - 1];
            var rsiCurrent = rsiExit[barNumber];
            var isRsiCrossedExitLevel = IsExitSignal(rsiPrevious, rsiCurrent);

            if (isRsiCrossedExitLevel)
            {
                Log("Сигнал выхода по RSI: порог {0}, режим {1} ({2} -> {3}). Закрываем " +
                    "позицию по рынку.", rsiExitLevel, rsiExitMode, rsiPrevious, rsiCurrent);
                security.CloseAtMarket(barNumber + 1, signalNameForClosePosition,
                    " Выход №1 RSI", position);
                return;
            }

            var stopPrice = GetStopPriceFromEntry(position.EntryPrice);
            if (useTrailingStop)
            {
                var close = security.GetBarClose(barNumber);
                var candidate = converter.Minus(close, Math.Round(atrMultiplier *
                    atr[barNumber], digitsAfterPoint));
                trailingStopPrice = converter.Maximum(trailingStopPrice, candidate);
                stopPrice = trailingStopPrice;
            }

            Log("Обновляем стоп-заявку. Стоп-цена = {0}.", stopPrice);
            security.CloseAtStop(barNumber + 1, stopPrice, signalNameForClosePosition,
                " Выход №1", position);
        }

        private IReadOnlyList<IDataBar> ConvertBarsForUsingInTsLabIndicators()
        {
            var candles = new ReadAndAddList<IDataBar>();
            foreach (var bar in security.Bars)
                candles.Add(bar);
            return candles;
        }

        private int CountDecimalPlaces(double value)
        {
            string strValue = value.ToString();
            var currentCulture = CultureInfo.CurrentCulture;
            char decimalSeparator = currentCulture.NumberFormat.NumberDecimalSeparator[0];

            int pointIndex = strValue.IndexOf(decimalSeparator);

            if (pointIndex != -1)
                return strValue.Length - pointIndex - 1;

            return 0;
        }

        public override void CheckPositionCloseCase(Position position,
            string signalNameForClosePosition, out bool isPositionClosing)
        {
            throw new NotImplementedException();
        }
    }
}
