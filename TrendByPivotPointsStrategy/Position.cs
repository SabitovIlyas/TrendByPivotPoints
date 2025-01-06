﻿namespace TradingSystems
{
    public interface Position
    {
        int BarNumberOpenPosition { get; }
        double EntryPrice { get; }
        double ExitPrice { get; }
        double Profit { get; }
        string SignalNameForOpenPosition { get; }
        int BarNumberClosePosition { get; }
        string SignalNameForClosePosition { get; }
        PositionSide PositionSide { get; }
        int Contracts { get; }

        void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition);
        void CloseAtMarket(int barNumber, double price, string signalNameForClosePosition);
        void CloseAtMarket(int barNumber, string signalNameForClosePosition);
        double GetFixedProfit();
        double GetUnfixedProfit(double barClose);        
    }
}