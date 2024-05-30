using System;
using System.Collections.Generic;
using TSLab.Script;

namespace TradingSystems
{
    public class SecurityLab : Security
    {
        public Currency Currency { get => currency; set { } }
        public int Shares { get => shares; set { } }
                
        private Currency currency;
        private int shares;

        public SecurityLab(Currency currency, int shares)
        {            
            this.currency = currency;
            this.shares = shares;
        }

        public int BarNumber { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public double? SellDeposit => 4500;

        public double? StepPrice => 1;

        public double? BuyDeposit => 4400;

        public Bar LastBar => throw new System.NotImplementedException();

        public bool IsLaboratory => throw new System.NotImplementedException();

        public bool IsRealTimeTrading => throw new System.NotImplementedException();

        public string Name => throw new NotImplementedException();

        public int RealTimeActualBarNumber => throw new NotImplementedException();
 
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
            throw new System.NotImplementedException();
        }

        public PositionTSLab GetLastClosedLongPosition(int barNumber)
        {
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

        public void BuyIfGreater(int barNumber, int contracts, double entryPricePlanned, string signalNameForOpenPosition, bool isConverted = false)
        {
            throw new NotImplementedException();
        }
    }
}