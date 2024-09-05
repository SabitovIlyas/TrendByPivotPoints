using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystems
{
    public class SecurityLab : Security
    {
        public Currency Currency { get => currency; set { } }
        public int Shares { get => shares; set { } }        
        public List<Bar> Bars { get; private set; }
        public string Name { get; private set; }
        public int BarNumber { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public double? SellDeposit => 4500;
        public double? StepPrice => 1;
        public double? BuyDeposit => 4400;
        public bool IsLaboratory => throw new System.NotImplementedException();
        public bool IsRealTimeTrading => throw new System.NotImplementedException();
        public int RealTimeActualBarNumber => throw new NotImplementedException();
        public double GObuying { get; private set; }
        public double GOselling { get; private set; }
        public List<double> HighPrices { get; private set; }
        public List<double> LowPrices { get; private set; }

        private Currency currency;
        private int shares;
        private Converter converter;
        private Order activeOrder;
        private PositionLab activePosition;

        public SecurityLab(Currency currency, int shares)
        {
            this.currency = currency;
            this.shares = shares;
        }

        public SecurityLab(Currency currency, int shares,
            double GObuying, double GOselling)
        {
            this.currency = currency;
            this.shares = shares;
            this.GObuying = GObuying;
            this.GOselling = GOselling;
        }

        public SecurityLab(string securityName, Currency currency, int shares,
            double GObuying, double GOselling, List<Bar> bars)
        {
            Name = securityName;
            this.currency = currency;
            this.shares = shares;
            this.GObuying = GObuying;
            this.GOselling = GOselling;
            Bars = bars;
            Initialize();
        }

        private void Initialize()
        {
            HighPrices = new List<double>();
            LowPrices = new List<double>();
            foreach (Bar bar in Bars)
            {
                HighPrices.Add(bar.High);
                LowPrices.Add(bar.Low);
            }
        }       

        public Bar LastBar
        {
            get
            {
                return Bars?.Last();
            }
        }       

        public Bar GetBar(int barNumer)
        {
            throw new NotImplementedException();
        }

        public double GetBarClose(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public int GetBarCompressedNumberFromBarBaseNumber(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public DateTime GetBarDateTime(int barNumber)
        {
            throw new NotImplementedException();
        }

        public double GetBarHigh(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public double GetBarLow(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public double GetBarOpen(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public List<Bar> GetBars(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public List<int> GetBarsBaseFromBarCompressed(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public int GetBarsCountReal()
        {
            return Bars.Count;
        }

        public PositionTSLab GetLastClosedLongPosition(int barNumber)
        {
            //Остановился здесь
            throw new NotImplementedException();
        }

        public PositionTSLab GetLastClosedShortPosition(int barNumber)
        {
            throw new NotImplementedException();
        }

        public bool IsRealTimeActualBar(int barNumber)
        {
            throw new NotImplementedException();
        }

        public void ResetBarNumberToLastBarNumber()
        {
        }

        public PositionTSLab GetLastActiveForSignal(string signalName, int barNumber)
        {
            throw new NotImplementedException();
        }

        public void BuyIfGreater(int barNumber, int contracts, double entryPricePlanned, 
            string signalNameForOpenPosition, bool isConverted = false)
        {
            converter = new Converter(isConverted);
            var positionSide = isConverted ? PositionSide.Long : PositionSide.Short;

            if (converter.IsGreaterOrEqual(LastBar.Close, entryPricePlanned))
            {
                activePosition = new PositionLab();
            }
            else
            {
                activeOrder = new Order(barNumber, positionSide, entryPricePlanned, contracts,
                    signalNameForOpenPosition);                
            }            
        }

        public void SellIfLess(int barNumber, int contracts, double entryPricePlanned, 
            string signalNameForOpenPosition, bool isConverted = false)
        {
            BuyIfGreater(barNumber, contracts, entryPricePlanned, signalNameForOpenPosition,
                isConverted: true);
        }

        public void Update(int barNumber)
        {
            
        }        
    }
}