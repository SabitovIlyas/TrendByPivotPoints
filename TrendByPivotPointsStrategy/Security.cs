using System;
using System.Collections.Generic;

namespace TradingSystems
{
    public interface Security
    {
        Currency Currency { get; set; }
        int Shares { get; set; }
        double? SellDeposit { get; }
        double? StepPrice { get; }
        double? BuyDeposit { get; }
        Bar LastBar { get; }
        List<Bar> GetBars(int barNumber);
        double GetBarHigh(int barNumber);
        double GetBarLow(int barNumber);
        double GetBarOpen(int barNumber);
        double GetBarClose(int barNumber);
        DateTime GetBarDateTime(int barNumber);
        int GetBarsCountReal();
        List<int> GetBarsBaseFromBarCompressed(int barNumber);
        bool IsLaboratory { get; }
        bool IsRealTimeTrading { get; }
        int GetBarCompressedNumberFromBarBaseNumber(int barNumber);
        Position GetLastClosedLongPosition(int barNumber);
        Position GetLastClosedShortPosition(int barNumber);
        bool IsRealTimeActualBar(int barNumber);
        void ResetBarNumberToLastBarNumber();

        String Name { get; }
        int RealTimeActualBarNumber { get; }
        Bar GetBar(int barNumer);
    }
}