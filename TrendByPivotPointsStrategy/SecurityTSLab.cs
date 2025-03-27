using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using TSLab.Script.Realtime;
using System;
using TSLab.Script.Handlers;
using System.Linq;

namespace TradingSystems
{
    public class SecurityTSLab : Security
    {
        public string Name => security.ToString();
        public Currency Currency { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Shares { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double GObuying => goBuying;
        public double GOselling => goSelling;
        public double[] HighPrices => highPrices;

        public double[] LowPrices => lowPrices;

        public List<Bar> Bars => bars;
        public double? StepPrice => finInfo.StepPrice;
        public double? SellDeposit => finInfo.SellDeposit;
        public double? BuyDeposit => finInfo.BuyDeposit;
        public ISecurity security;
        public int BarNumber
        {
            get
            {
                return barNumber;
            }

            set
            {
                if (value < 0)
                    barNumber = 0;
                else
                {
                    var securityCount = GetBarsCountReal();
                    if (securityCount == 0)
                        barNumber = 0;
                    else
                    {
                        if (value >= securityCount)
                            barNumber = securityCount - 1;
                        else
                            barNumber = value;
                    }
                }
            }
        }

        public Bar LastBar
        {
            get
            {
                var bar = GetBarIDataBar(barNumber);
                return new Bar() { Open = bar.Open, High = bar.High, Low = bar.Low, Close = bar.Close, Date = bar.Date };
            }
        }

        public int RealTimeActualBarNumber
        {
            get
            {
                if (IsRealTimeTrading)
                {
                    var bars = GetBarsReal();
                    if (bars != null && bars.Count > 0)
                        return bars.Count - 1;
                    return 0;
                }
                return barNumber;
            }
        }

        private PositionTSLab lastLongPositionClosed;
        private PositionTSLab lastShortPositionClosed;
        private int barNumber;
        private IDataBar nullDataBar = new NullDataBar();
        private FinInfo finInfo;
        private ISecurity baseSecurity = new SecurityNull();
        private IContext context;
        private List<Bar> bars = new List<Bar>();
        private double goBuying;
        private double goSelling;
        private double[] highPrices;
        private double[] lowPrices;

        public SecurityTSLab(ISecurity security)
        {
            finInfo = security.FinInfo;
            InitializeSecurity(security);
        }

        public SecurityTSLab(ISecurity compressedSecurity, ISecurity baseSecurity)
        {
            this.baseSecurity = baseSecurity;
            finInfo = baseSecurity.FinInfo;
            InitializeSecurity(compressedSecurity);
            CompareBarsBaseSecurityWithCompressedSecurity();
        }

        public SecurityTSLab(ISecurity security, IContext context)
        {
            finInfo = baseSecurity.FinInfo;
            InitializeSecurity(security);
            this.context = context;
        }

        public SecurityTSLab(ISecurity baseSecurity, List<Bar> bars)
        {
            this.baseSecurity = baseSecurity;
            finInfo = baseSecurity.FinInfo;
            InitializeSecurity(baseSecurity);
            CompareBarsBaseSecurityWithCompressedSecurity();
        }

        public bool IsRealTimeActualBar(int barNumber)
        {
            if (IsRealTimeTrading)
            {
                var bars = GetBarsReal();
                if (bars != null)
                {
                    var lastBar = bars.Count - 1;
                    if (lastBar == barNumber)
                        return true;
                }
                return false;
            }
            return true;
        }

        public bool IsRealTimeActualBarNew(int barNumber)
        {
            if (context.IsLastBarClosed)
                return IsRealTimeActualBar(barNumber);
            else
                return IsRealTimeActualBar(barNumber - 1);
        }

        private void InitializeSecurity(ISecurity security)
        {
            this.security = security;
            var bars = security.Bars;
            barNumber = bars.Count - 1;
            DefineIsLaboratory();
            ConvertBars();
            goBuying = security.FinInfo.BuyDeposit ?? (double.NaN);
            goSelling = security.FinInfo.SellDeposit ?? (double.NaN);

            highPrices = new double[security.Bars.Count];            
            lowPrices = new double[security.Bars.Count];

            var i = 0;
            foreach (Bar bar in security.Bars)
            {
                HighPrices[i] = bar.High;
                LowPrices[i] = bar.Low;
                i++;
            }
        }        

        private void ConvertBars()
        {
            bars = new List<Bar>();
            foreach (var bar in security.Bars)
            {
                var newBar = Bar.Create(bar.Date, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
                bars.Add(newBar);
            }
        }

        private List<List<int>> barsBaseSecurityInBarsCompressedSecurity = new List<List<int>>();
        private void CompareBarsBaseSecurityWithCompressedSecurity()
        {
            var lastBarNumber = GetBarsCountReal() - 1;
            var securityBaseCount = GetSecurityBaseCount();
            List<int> barsInCompressedBar;

            for (var i = 0; i < lastBarNumber; i++)
            {
                barsInCompressedBar = new List<int>();

                for (var j = 0; j < securityBaseCount; j++)
                {
                    if (baseSecurity.Bars[j].Date >= security.Bars[i].Date && baseSecurity.Bars[j].Date < security.Bars[i + 1].Date)
                        barsInCompressedBar.Add(j);
                }

                barsBaseSecurityInBarsCompressedSecurity.Add(barsInCompressedBar);
            }

            barsInCompressedBar = new List<int>();
            for (var j = 0; j < securityBaseCount; j++)
            {
                if (baseSecurity.Bars[j].Date >= security.Bars[lastBarNumber].Date)
                    barsInCompressedBar.Add(j);
            }

            barsBaseSecurityInBarsCompressedSecurity.Add(barsInCompressedBar);
        }

        public List<int> GetBarsBaseFromBarCompressed(int barNumber)
        {
            return barsBaseSecurityInBarsCompressedSecurity[barNumber];
        }

        public int GetBarCompressedNumberFromBarBaseNumber(int barNumber)
        {
            List<int> bars;
            for (var i = 0; i < barsBaseSecurityInBarsCompressedSecurity.Count; i++)
            {
                bars = barsBaseSecurityInBarsCompressedSecurity[i];
                for (var j = 0; j < bars.Count; j++)
                    if (bars[j] == barNumber)
                        return i;
            }

            throw new Exception("Номеру базового бара не соответствует ни один сжатый бар");
        }

        private int GetSecurityBaseCount()
        {
            var bars = baseSecurity.Bars;
            return bars.Count;
        }

        public int GetBarNumberCompressed()
        {
            return 0;
        }

        public double GetBarOpen(int barNumber)
        {
            var bar = GetBarIDataBar(barNumber);
            return bar.Open;
        }

        public double GetBarLow(int barNumber)
        {
            var bar = GetBarIDataBar(barNumber);
            return bar.Low;
        }

        public double GetBarHigh(int barNumber)
        {
            var bar = GetBarIDataBar(barNumber);
            return bar.High;
        }

        public double GetBarClose(int barNumber)
        {
            var bar = GetBarIDataBar(barNumber);
            return bar.Close;
        }

        public DateTime GetBarDateTime(int barNumber)
        {
            var bar = GetBarIDataBar(barNumber);
            return bar.Date;
        }

        public int GetBarsCountReal()
        {
            var bars = GetBarsReal();
            return bars.Count;
        }

        private IDataBar GetBarIDataBar(int barNumber)
        {
            if (IsBarNumberCorrect(barNumber))
            {
                var bars = GetBarsReal();
                if (bars.Count == 0)
                    throw new Exception("Баров нет");
                return bars[barNumber];
            }
            return nullDataBar;
        }

        private bool IsBarNumberCorrect(int barNumber)
        {
            var bars = GetBarsReal();

            if (barNumber < 0 || barNumber > this.barNumber)
                return false;
            return true;
        }

        private IReadOnlyList<IDataBar> GetBarsReal()
        {
            return security.Bars;
        }

        public void ResetBarNumberToLastBarNumber()
        {
            var barsCountReal = GetBarsCountReal();
            barNumber = barsCountReal - 1;
        }

        public List<Bar> GetBars(int barNumber)
        {
            var bars = new List<Bar>();
            for (var i = 0; i <= barNumber; i++)
            {
                var bar = GetBarIDataBar(i);
                bars.Add(new Bar() { Open = bar.Open, High = bar.High, Low = bar.Low, Close = bar.Close, Date = bar.Date, Volume = bar.Volume });
            }

            return bars;
        }
        public bool IsLaboratory => isLaboratory;
        public bool IsRealTimeTrading => !isLaboratory;

        public double CommissionRate => throw new NotImplementedException();

        private bool isLaboratory;
        private void DefineIsLaboratory()
        {
            var realTimeSecurity = security as ISecurityRt;
            isLaboratory = realTimeSecurity == null;
        }
        public Position GetLastClosedLongPosition(int barNumber)
        {
            var positions = security.Positions;
            var position = positions.GetLastLongPositionClosed(barNumber);
            if (position == null)
                return null;
            if (lastLongPositionClosed == null)
                lastLongPositionClosed = new PositionTSLab(position, barNumber);

            if (position.EntryPrice == lastLongPositionClosed.EntryPrice && position.EntryBarNum == lastLongPositionClosed.BarNumberOpenPosition && position.Profit() == lastLongPositionClosed.Profit)
                return lastLongPositionClosed;

            lastLongPositionClosed = new PositionTSLab(position, barNumber);
            return lastLongPositionClosed;
        }
        public Position GetLastClosedShortPosition(int barNumber)
        {
            var positions = security.Positions;
            var position = positions.GetLastShortPositionClosed(barNumber);
            if (position == null)
                return null;
            if (lastShortPositionClosed == null)
                lastShortPositionClosed = new PositionTSLab(position, barNumber);

            if (position.EntryPrice == lastShortPositionClosed.EntryPrice && position.EntryBarNum == lastShortPositionClosed.BarNumberOpenPosition && position.Profit() == lastShortPositionClosed.Profit)
                return lastShortPositionClosed;

            lastShortPositionClosed = new PositionTSLab(position, barNumber );
            return lastShortPositionClosed;
        }

        public Bar GetBar(int barNumber)
        {
            if (IsBarNumberCorrect(barNumber))
            {
                var bars = GetBars(barNumber);
                if (bars.Count == 0)
                    throw new Exception("Баров нет");
                return bars[barNumber];
            }
            return new Bar();
        }

        public ISecurity CompressLessIntervalTo1DayInterval()
        {
            var bars = new List<Bar>();
            FillBarParams(security.Bars.First(), out var date, out var open, out var high, out var low, out var close, out var volume);//, out var ticker, out var period);

            for (int i = 1; i < security.Bars.Count; i++)
            {
                IDataBar baseBar = security.Bars[i];

                if (date.Date != baseBar.Date.Date)
                {
                    bars.Add(new Bar() { Date = date, Open = open, High = high, Low = low, Close = close, Volume = volume, Ticker = security.Symbol, Period = "1D" });
                    FillBarParams(security.Bars[i], out date, out open, out high, out low, out close, out volume);
                }
                else
                {
                    high = Math.Max(high, baseBar.High);
                    low = Math.Min(low, baseBar.Low);
                    close = baseBar.Close;
                    volume += baseBar.Volume;
                }
            }

            bars.Add(new Bar() { Date = date, Open = open, High = high, Low = low, Close = close, Volume = volume, Ticker = security.Symbol, Period = "1D" });

            var result = CustomSecurity.Create(bars);
            return result;
        }

        private void FillBarParams(IDataBar bar, out DateTime date, out double open, out double high, out double low, out double close, out double volume)//, out string ticker, out string period)
        {
            date = new DateTime(bar.Date.Year, bar.Date.Month, bar.Date.Day, 10, 0, 0);
            open = bar.Open;
            high = bar.High;
            low = bar.Low;
            close = bar.Close;
            volume = bar.Volume;
        }

        public Position GetLastActiveForSignal(string signalName, int barNumber)
        {
            var positions = security.Positions;
            IPosition position = null;
            if (positions == null || positions.Count() == 0)
                return null;

            position = positions.GetLastActiveForSignal(signalName, barNumber);
            if  (position == null)
                return null;

            return new PositionTSLab(position, barNumber);
        }

        public void BuyIfGreater(int barNumber, int contracts, double price, string signalNameForOpenPosition, bool isConverted = false)
        {
            var positions = security.Positions;

            if (!isConverted)
                positions.BuyIfGreater(barNumber, contracts, price, signalNameForOpenPosition);
            else
                positions.SellIfLess(barNumber, contracts, price, signalNameForOpenPosition);
        }

        public void SellIfLess(int barNumber, int contracts, double price, string signalNameForOpenPosition, bool isConverted = false)
        {
            var positions = security.Positions;

            if (!isConverted)
                positions.SellIfLess(barNumber, contracts, price, signalNameForOpenPosition);
            else
                positions.BuyIfGreater(barNumber, contracts, price, signalNameForOpenPosition);
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition, PositionLab positionLab, out Order closeOrder)
        {
            throw new NotImplementedException();
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition, PositionLab positionLab, out Order closeOrder)
        {
            throw new NotImplementedException();
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition, string notes, Position position)
        {
            position.CloseAtStop(barNumber, stopPrice, signalNameForClosePosition);
        }

        public void CancelAllOrders(int barNumber)
        {            
        }

        public double GetProfit(int barNumber)
        {
            throw new NotImplementedException();
        }

        public double GetProfit()
        {
            throw new NotImplementedException();
        }

        public List<Order> GetOrders(int barNumber)
        {
            throw new NotImplementedException();
        }

        public List<Order> GetActiveOrders(int barNumber)
        {
            throw new NotImplementedException();
        }

        public List<Position> GetDeals(int barNumber)
        {
            throw new NotImplementedException();
        }

        public List<Position> GetMetaDeals(int barNumber)
        {
            throw new NotImplementedException();
        }

        public List<Position> GetDeals()
        {
            throw new NotImplementedException();
        }

        public List<Position> GetMetaDeals()
        {
            throw new NotImplementedException();
        }
    }
}