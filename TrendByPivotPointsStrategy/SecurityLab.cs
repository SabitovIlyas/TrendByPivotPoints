using System;
using System.Collections.Generic;
using TSLab.Script;
using TradingSystems;

namespace TradingSystems
{
    /// <summary>
    /// Я не пользуюсь этим классом. Но им надо пользоваться! Возрождаю этот класс.
    /// </summary>
    public class SecurityLab : Security
    {
        private ISecurity sec;
        //TODO: Удалить эту реализацию Security. Но сперва рефакторить SecurityTSlab.
        public SecurityLab()
        {            
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

        public Position GetLastClosedLongPosition(int barNumber)
        {
            throw new NotImplementedException();
        }

        public Position GetLastClosedShortPosition(int barNumber)
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
    }
}