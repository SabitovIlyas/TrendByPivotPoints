﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using TSLab.DataSource;

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
        private List<PositionLab> positions = new List<PositionLab>();
        private List<PositionLab> activePositions = new List<PositionLab>();
        private List<Order> orders = new List<Order>();
        private List<Order> activeOrders = new List<Order>();
        private Position lastClosedLongPosition;
        private Position lastClosedShortPosition;


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

        public Position GetLastClosedLongPosition(int barNumber)
        {
            return lastClosedLongPosition;
        }

        public Position GetLastClosedShortPosition(int barNumber)
        {
            return lastClosedShortPosition;
        }

        public bool IsRealTimeActualBar(int barNumber)
        {
            return true;            
        }

        public void ResetBarNumberToLastBarNumber()
        {
        }

        public Position GetLastActiveForSignal(string signalName, int barNumber)
        {
            return activePositions.Find(p => p.SignalNameForOpenPosition == signalName);
        }        

        public void BuyIfGreater(int barNumber, int contracts, double entryPricePlanned, 
            string signalNameForOpenPosition, bool isConverted = false)
        {
            converter = new Converter(isConverted);
            var positionSide = isConverted ? PositionSide.Short : PositionSide.Long;

            if (converter.IsGreaterOrEqual(Bars[barNumber].Open, entryPricePlanned))
            {
                var order = new Order(barNumber, positionSide, Bars[barNumber].Open, contracts,
                    signalNameForOpenPosition);
                var activePosition = new PositionLab(barNumber, order, this);
                positions.Add(activePosition);
                activePositions.Add(activePosition);
                orders.Add(order);
            }
            else
            {
                activeOrder = new Order(barNumber, positionSide, entryPricePlanned, contracts,
                    signalNameForOpenPosition);
                orders.Add(activeOrder);
                activeOrders.Add(activeOrder);
            }            
        }

        public void SellIfLess(int barNumber, int contracts, double entryPricePlanned, 
            string signalNameForOpenPosition, bool isConverted = false)
        {
            BuyIfGreater(barNumber, contracts, entryPricePlanned, signalNameForOpenPosition,
                isConverted: true);
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition, 
            PositionLab position, out Order closeOrder)
        {
            closeOrder = new Order(barNumber, position.PositionSide, double.NaN, 
                position.Contracts, signalNameForClosePosition, OrderType.Market);
            orders.Add(closeOrder);
            activeOrders.Add(closeOrder);
        }

        public void CloseAtStop(int barNumber, double stopPrice, 
            string signalNameForClosePosition, PositionLab position, out Order closeOrder)
        {
            closeOrder = new Order(barNumber, position.PositionSide, stopPrice,
                position.Contracts, signalNameForClosePosition);
            orders.Add(closeOrder);
            activeOrders.Add(closeOrder);
        }

        public void Update(int barNumber)
        {
            var bar = Bars[barNumber];

            foreach (var order in activeOrders)
            {
                order.Execute(bar);
                if (order.IsExecuted)
                {
                    var position = new PositionLab(barNumber, order, this);
                    positions.Add(position);
                    activePositions.Add(position);
                }                        
            }

            foreach (var position in activePositions)            
                position.Update(barNumber, bar);

            var ordersToExcludeFromActiveOrders = new List<Order>();
            foreach (var order in activeOrders)
                if (!order.IsActive)
                    ordersToExcludeFromActiveOrders.Add(order);

            foreach(var order in ordersToExcludeFromActiveOrders)            
                activeOrders.Remove(order);

            var positionToExcludeFromActivePositions = new List<PositionLab>();
            foreach (var position in activePositions)
                if (!position.IsActive)
                    positionToExcludeFromActivePositions.Add(position);
                        
            foreach (var position in positionToExcludeFromActivePositions)
                activePositions.Remove(position);

            if (positionToExcludeFromActivePositions.Count == 0)
                return;

            var lastPosition = positionToExcludeFromActivePositions.Last();
            if (lastPosition == null)
                return;

            switch (lastPosition.PositionSide)
            {
                case PositionSide.Long:
                    {
                        lastClosedLongPosition = lastPosition;
                        break;
                    }
                case PositionSide.Short:
                    {
                        lastClosedShortPosition = lastPosition;
                        break;
                    }
            }
        }
        
        public List<Order> GetActiveOrders(int barNumber)
        {
            var activeOrders = (from order in orders
                               where order.BarNumber <= barNumber
                               select order).ToList();

            return activeOrders;
        }
    }
}