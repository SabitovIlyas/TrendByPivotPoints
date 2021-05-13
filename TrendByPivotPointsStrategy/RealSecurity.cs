using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class RealSecurity
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

        private ISecurity security;
        private int barNumber = 0;        
        private IDataBar nullDataBar = new NullDataBar();
        private FinInfo finInfo;
        
        public RealSecurity(ISecurity security)
        {
            this.security = security;
            finInfo = security.FinInfo;
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

        private int GetSecurityCount()
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
            if (barNumber < 0 || barNumber >= this.barNumber)
                return false;
            return true;
        }

        private IReadOnlyList<TSLab.DataSource.IDataBar> GetBars()
        {
            return security.Bars;
        }
    }
}