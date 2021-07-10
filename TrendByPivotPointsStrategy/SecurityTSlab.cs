using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using TSLab.Script.Realtime;
using System;

namespace TrendByPivotPointsStrategy
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
                    var securityCount = GetBarsCount();
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

        public ISecurity security;
        //private int barNumber = 0; //заглушил
        private int barNumber;
        private IDataBar nullDataBar = new NullDataBar();
        private FinInfo finInfo;
        private ISecurity baseSecurity = new SecurityNull();

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

        private void InitializeSecurity(ISecurity security)
        {
            this.security = security;            
            barNumber = security.Bars.Count - 1; //заглушил
            DefineIsLaboratory();
        }

        private List<List<int>> barsBaseSecurityInBarsCompressedSecurity = new List<List<int>>();
        private void CompareBarsBaseSecurityWithCompressedSecurity()
        {
            var lastBarNumber = GetBarsCount() - 1;
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
            for (var i=0;i<barsBaseSecurityInBarsCompressedSecurity.Count;i++)
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

        public DateTime GetBarDateTime(int barNumber)
        {
            var bar = GetBar(barNumber);
            return bar.Date;
        }

        public int GetBarsCount()
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

        private IReadOnlyList<IDataBar> GetBars()
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
        public bool IsLaboratory => isLaboratory;
        public bool IsRealTimeTrading => !isLaboratory;

        private bool isLaboratory;
        private void DefineIsLaboratory()
        {
            var realTimeSecurity = security as ISecurityRt;
            isLaboratory = realTimeSecurity == null;
        }
    }
}