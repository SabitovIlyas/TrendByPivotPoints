using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class SecurityReal : Security
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
                    var securityCount = GetSecurityCount();
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
                var bar = GetBar(barNumber);
                return new Bar() { Open = bar.Open, High = bar.High, Low = bar.Low, Close = bar.Close, Date = bar.Date };
            }
        }

        private ISecurity security;
        //private int barNumber = 0;
        private int barNumber;
        private IDataBar nullDataBar = new NullDataBar();
        private FinInfo finInfo;

        public SecurityReal(ISecurity security)
        {
            this.security = security;            
            finInfo = security.FinInfo;
            barNumber = security.Bars.Count - 1; //заглушил
        }

        public double GetBarOpen(int barNumber)
        {
            var bar = GetBar(barNumber);
            return bar.Open;
        }

        public double GetBarLow(int barNumber)
        {
            var bar = GetBar(barNumber);
            return bar.Low;
        }

        public double GetBarHigh(int barNumber)
        {
            var bar = GetBar(barNumber);
            return bar.High;
        }

        public double GetBarClose(int barNumber)
        {
            var bar = GetBar(barNumber);
            return bar.Close;
        }

        public int GetSecurityCount()
        {
            var bars = GetBars();
            return bars.Count;
        }

        private IDataBar GetBar(int barNumber)
        {
            if (IsBarNumberCorrect(barNumber))
            {
                var bars = GetBars();
                return bars[barNumber];
            }
            return nullDataBar;
        }

        private bool IsBarNumberCorrect(int barNumber)
        {
            if (barNumber < 0 || barNumber > this.barNumber)
                return false;
            return true;
        }

        private IReadOnlyList<TSLab.DataSource.IDataBar> GetBars()
        {
            return security.Bars;
        }       

        public List<Bar> GetBars(int barNumber)
        {
            var bars = new List<Bar>();
            for (var i = 0; i <= barNumber; i++)            
                bars.Add(new Bar() { Open = GetBar(i).Open, High = GetBar(i).High, Low = GetBar(i).Low, Close = GetBar(i).Close, Date = GetBar(i).Date });            
            
            return bars;
        }        
    }
}