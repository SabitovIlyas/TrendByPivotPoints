using System;
using System.Collections.Generic;

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
        Position GetLastClosedLongPosition(int barNumber);
        Position GetLastClosedShortPosition(int barNumber);
        bool IsRealTimeActualBar(int barNumber);
        void ResetBarNumberToLastBarNumber();

        String Name { get; }
        int RealTimeActualBarNumber { get; }
        Bar GetBar(int barNumer);
        Position GetLastActiveForSignal(string signalName, int barNumber);
        void BuyIfGreater(int barNumber, int contracts, double price,
            string signalName, bool isConverted = false);
        void SellIfLess(int barNumber, int contracts, double price,
            string signalName, bool isConverted = false);        
        void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition, string notes, Position position);
        void CancelAllOrders(int barNumber);
        double GetProfit(int barNumber);
        double GetProfit();
        List<Order> GetOrders(int barNumber);
        List<Order> GetActiveOrders(int barNumber);
        List<Position> GetDeals(int barNumber);
        List<Position> GetMetaDeals(int barNumber);
        List<Position> GetDeals();
        List<Position> GetMetaDeals();

        List<double> HighPrices { get; }
        List<double> LowPrices { get; }
        List<Bar> Bars { get; }
        double CommissionRate { get; }
    }
}