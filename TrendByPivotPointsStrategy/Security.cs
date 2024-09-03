using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TSLab.Script.Handlers;
using TSLab.Script;

namespace TradingSystems
{
    public interface Security
    {
        Currency Currency { get; set; }
        double GObuying { get; }
        double GOselling { get; }
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
        PositionTSLab GetLastClosedLongPosition(int barNumber);
        PositionTSLab GetLastClosedShortPosition(int barNumber);
        bool IsRealTimeActualBar(int barNumber);
        void ResetBarNumberToLastBarNumber();

        String Name { get; }
        int RealTimeActualBarNumber { get; }
        Bar GetBar(int barNumer);
        PositionTSLab GetLastActiveForSignal(string signalName, int barNumber);
        void BuyIfGreater(int barNumber, int contracts, double price,
            string signalName, bool isConverted = false);
        void SellIfLess(int barNumber, int contracts, double price,
            string signalName, bool isConverted = false);
        List<double> HighPrices { get; }
        List<double> LowPrices { get; }
        List<Bar> Bars { get; }
    }
}