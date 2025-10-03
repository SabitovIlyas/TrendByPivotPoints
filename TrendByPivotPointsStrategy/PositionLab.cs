using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TradingSystems
{
    public class PositionLab : Position
    {
        public int BarNumberOpenPosition { get; private set; } = int.MaxValue;
        public double EntryPrice { get; private set; } = double.MinValue;
        public double ExitPrice { get; private set; } = double.MinValue;
        public PositionSide PositionSide => openOrder.PositionSide;
        public int Contracts { get; private set; } = 0;
        public string SignalNameForOpenPosition => openOrder.SignalName;
        public string SignalNameForClosePosition { get; private set; }
        public Security Security { get; private set; }
        public int BarNumberClosePosition { get; set; } = int.MaxValue;   

        private Order openOrder;
        private Converter converter;
        private double profit = double.MinValue;        

        public PositionLab(int barNumber, Order openOrder, Security security)
        {
            BarNumberOpenPosition = barNumber;
            this.openOrder = openOrder;
            Security = security;
            converter = new Converter(isConverted: PositionSide != PositionSide.Long);
            EntryPrice = openOrder.Price;
            Contracts = openOrder.Contracts;
        }

        public PositionLab(int barNumber, PositionSide positionSide, Security security)
        {
            BarNumberOpenPosition = barNumber;            
            Security = security;
            converter = new Converter(isConverted: positionSide != PositionSide.Long);            
        }

        public PositionLab(PositionSide positionSide, Security security, double profit, 
            int barNumberOpenPosition, int barNumberClosePosition, double averageEntryPrice,
            double averageExitPrice, int contracts) : this(barNumberOpenPosition, positionSide, 
                security)
        {

            BarNumberClosePosition = barNumberClosePosition;
            this.profit = profit;
            EntryPrice = averageEntryPrice;
            ExitPrice = averageExitPrice;
            Contracts = contracts;
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            BarNumberClosePosition = barNumber;
            ExitPrice = stopPrice;
            SignalNameForClosePosition = signalNameForClosePosition;
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            var bar = Security.GetBar(barNumber);
            CloseAtMarket(barNumber, bar.Open, signalNameForClosePosition);
        }

        public void CloseAtMarket(int barNumber, double price, string signalNameForClosePosition)
        {
            BarNumberClosePosition = barNumber;
            ExitPrice = price;
            SignalNameForClosePosition = signalNameForClosePosition;
        }

        public double GetProfit(int barNumber)
        {
            if (profit != double.MinValue)
                return profit;

            if (barNumber >= BarNumberClosePosition)
            {
                return GetFixedProfit();
            }
            else if (barNumber >= BarNumberOpenPosition)
            {
                return GetUnfixedProfit(barNumber);
            }
            return 0;
        }

        public double GetFixedProfit()
        {            
            return GetProfit(ExitPrice);
        }

        public double GetUnfixedProfit(int barNumber)
        {
            var barClose = Security.GetBarClose(barNumber);            
            return GetProfit(barClose);
        }

        private double GetProfit(double price)
        {
            var profit = converter.Difference(price, EntryPrice);
            var commissionEnter = GetTotalCommision(EntryPrice);
            var commissionExit = GetTotalCommision(price);
            var result = (profit - commissionEnter - commissionExit) * Contracts *
                Security.Shares;

            var d = CountDecimalPlaces(price);
            return Math.Round(result,d);
        }

        private double GetTotalCommision(double price)
        {
            var exchangeCommission = price * Security.CommissionRate;// 0,01980%
            var brokerCommission = exchangeCommission;
            var totalCommission = exchangeCommission + brokerCommission;
            var reserve = 0.25 * totalCommission;

            return totalCommission + reserve;            
        }

        public double GetProfit()
        {
            return GetProfit(Security.Bars.Count - 1);
        }

        private int CountDecimalPlaces(double value)
        {
            string strValue = value.ToString();
            var currentCulture = CultureInfo.CurrentCulture;
            char decimalSeparator = currentCulture.NumberFormat.NumberDecimalSeparator[0];

            int pointIndex = strValue.IndexOf(decimalSeparator);

            if (pointIndex != -1)
                return strValue.Length - pointIndex - 1;

            return 0;
        }
    }
}