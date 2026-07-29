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
    /// Инстанс торгует одну сторону (positionSide). Для шорта условия применяются к
    /// зеркальному RSI' = 100 − RSI, поэтому используется одна пара порогов
    /// rsiEntryLevel/rsiExitLevel на обе стороны.
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
        private double atrMultiplier;
        private bool useTrailingStop;

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
                "atrMultiplier: {6}; useTrailingStop: {7}", maPeriod, rsiEntryPeriod,
                rsiExitPeriod, atrPeriod, rsiEntryLevel, rsiExitLevel, atrMultiplier,
                useTrailingStop);
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

        //Зеркальный RSI: для лонга — сам RSI, для шорта — 100 − RSI. Благодаря этому
        //условия входа и выхода записываются одинаково для обеих сторон.
        private double GetMirroredRsi(IList<double> rsi, int barNumber)
        {
            var value = rsi[barNumber];
            if (positionSide == PositionSide.Short)
                return 100 - value;
            return value;
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
            var rsi = GetMirroredRsi(rsiEntry, barNumber);

            var isRsiExtreme = rsi < rsiEntryLevel;
            var isTrendFilterPassed = converter.IsGreater(close, sma[barNumber]);

            Log("RSI' = {0} (порог {1}); закрытие {2} {3} SMA = {4}", rsi, rsiEntryLevel,
                close, converter.Above, sma[barNumber]);

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

        private double GetStopPriceFromEntry(double entryPrice)
        {
            return converter.Minus(entryPrice, Math.Round(atrMultiplier * fixedAtr,
                digitsAfterPoint));
        }

        private void ManageOpenedPosition(string notes)
        {
            var position = GetOpenedPosition(notes);

            //Выход по RSI: пересечение порога снизу вверх (в зеркальных координатах).
            var rsiPrevious = GetMirroredRsi(rsiExit, barNumber - 1);
            var rsiCurrent = GetMirroredRsi(rsiExit, barNumber);
            var isRsiCrossedExitLevel = rsiPrevious < rsiExitLevel &&
                rsiCurrent >= rsiExitLevel;

            if (isRsiCrossedExitLevel)
            {
                Log("RSI' пересёк порог выхода {0} снизу вверх ({1} -> {2}). Закрываем " +
                    "позицию по рынку.", rsiExitLevel, rsiPrevious, rsiCurrent);
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
