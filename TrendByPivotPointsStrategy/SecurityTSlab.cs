using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using TSLab.Script.Realtime;
using System;
using TSLab.Script.Handlers;
using TradingSystems;
using System.Linq;


namespace TradingSystems
{
    public class SecurityTSlab : Security
    {
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
        public double? StepPrice => finInfo.StepPrice; 
        public double? SellDeposit => finInfo.SellDeposit;
        public double? BuyDeposit => finInfo.BuyDeposit;
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
        
        public ISecurity security;

        private int barNumber;
        private IDataBar nullDataBar = new NullDataBar();
        private FinInfo finInfo;
        private ISecurity baseSecurity = new SecurityNull();
        private IContext context;

        public SecurityTSlab(ISecurity security)
        {
            finInfo = security.FinInfo;
            InitializeSecurity(security);
        }

        public SecurityTSlab(ISecurity compressedSecurity, ISecurity baseSecurity)
        {
            this.baseSecurity = baseSecurity;
            finInfo = baseSecurity.FinInfo;
            InitializeSecurity(compressedSecurity);
            CompareBarsBaseSecurityWithCompressedSecurity();
        }

        public SecurityTSlab(ISecurity security, IContext context)
        {
            finInfo = baseSecurity.FinInfo;
            InitializeSecurity(security);
            this.context = context;
        }

        public SecurityTSlab(ISecurity baseSecurity, List<Bar> bars)
        {
            this.baseSecurity = baseSecurity;
            finInfo = baseSecurity.FinInfo;
            //
            //
            
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
                bars.Add(new Bar() { Open = GetBarIDataBar(i).Open, High = GetBarIDataBar(i).High, Low = GetBarIDataBar(i).Low, Close = GetBarIDataBar(i).Close, Date = GetBarIDataBar(i).Date });

            return bars;
        }
        public bool IsLaboratory => isLaboratory;
        public bool IsRealTimeTrading => !isLaboratory;

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
                lastLongPositionClosed = new Position { EntryPrice = position.EntryPrice, BarNumber = position.EntryBarNum, Security = this, Profit = position.Profit() };

            if (position.EntryPrice == lastLongPositionClosed.EntryPrice && position.EntryBarNum == lastLongPositionClosed.BarNumber && position.Profit() == lastLongPositionClosed.Profit)
                return lastLongPositionClosed;

            lastLongPositionClosed = new Position { EntryPrice = position.EntryPrice, BarNumber = position.EntryBarNum, Security = this, Profit = position.Profit() };
            return lastLongPositionClosed;
        }
        public Position GetLastClosedShortPosition(int barNumber)
        {
            var positions = security.Positions;
            var position = positions.GetLastShortPositionClosed(barNumber);
            if (position == null)
                return null;
            if (lastShortPositionClosed == null)
                lastShortPositionClosed = new Position { EntryPrice = position.EntryPrice, BarNumber = position.EntryBarNum, Security = this, Profit = position.Profit() };

            if (position.EntryPrice == lastShortPositionClosed.EntryPrice && position.EntryBarNum == lastShortPositionClosed.BarNumber && position.Profit() == lastShortPositionClosed.Profit)
                return lastShortPositionClosed;

            lastShortPositionClosed = new Position { EntryPrice = position.EntryPrice, BarNumber = position.EntryBarNum, Security = this, Profit = position.Profit() };
            return lastShortPositionClosed;
        }

        private Position lastLongPositionClosed;
        private Position lastShortPositionClosed;
        public string Name => security.ToString();
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
            var bars = new ReadAndAddList<IDataBar>();
            //{
            //    new Bar() { Date = new DateTime(2021, 6, 18, 10, 0, 0), Open = 9, High = 14, Low = 6, Close = 6 },
            //    new Bar() { Date = new DateTime(2021, 6, 19, 10, 0, 0), Open = 15, High = 18, Low = 15, Close = 15 }
            //};


            var baseBarFirst = security.Bars.First();
            var date = baseBarFirst.Date.Date;
            var open = baseBarFirst.Open;
            var high = baseBarFirst.High;
            var low = baseBarFirst.Low;
            var close = baseBarFirst.Close;

            foreach(var baseBar in security.Bars)
            {

                if (date!=baseBar.Date.Date)
                {

                }    
            }

            var result = CustomSecurity.Create(bars);                  

            return result;
        }
    }
}