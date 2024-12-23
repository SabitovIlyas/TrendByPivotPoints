﻿using System;
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
        public bool IsRealTimeTrading => false;
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
        private Dictionary<Order, Position> ordersPositions = new Dictionary<Order, Position>();
        private List<Order> activeOrders = new List<Order>();
        private Position lastClosedLongPosition;
        private Position lastClosedShortPosition;
        private OrderToPositionMapping mapping;

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

        public SecurityLab(Currency currency, int shares, List<Bar> bars)
        {
            if (bars != null || bars.Count > 0)
                Name = bars.First().Ticker;
            this.currency = currency;
            this.shares = shares;
            Bars = bars;
            Initialize();
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
            mapping = new OrderToPositionMapping(Bars);
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
            var position = positions.FindLast(p => p.SignalNameForOpenPosition == signalName);
            if (position == null)
                return null;
            if (position.BarNumberOpenPosition > barNumber)
                return null;


            return activePositions.Find(p => p.SignalNameForOpenPosition == signalName);
        }

        public void BuyIfGreater(int barNumber, int contracts, double entryPricePlanned,
            string signalNameForOpenPosition, bool isConverted = false)
        {
            mapping.CreateOpenLimitOrder(barNumber, contracts, entryPricePlanned, 
                signalNameForOpenPosition, isConverted);
        }

        public void SellIfLess(int barNumber, int contracts, double entryPricePlanned,
            string signalNameForOpenPosition, bool isConverted = false)
        {
            BuyIfGreater(barNumber, contracts, entryPricePlanned, signalNameForOpenPosition,
                isConverted: true);
        }

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition,
            PositionLab position, out Order closeOrder)//r
        {
            closeOrder = new Order(barNumber, position.PositionSide, double.NaN,
                position.Contracts, signalNameForClosePosition, OrderType.Market);
            orders.Add(closeOrder);
            ordersPositions.Add(closeOrder, position);
            activeOrders.Add(closeOrder);
        }
       
        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition, 
            string notes, Position position)
        {
            mapping.CreateCloseLimitOrder(barNumber, stopPrice, signalNameForClosePosition, notes, 
                position);
        }

        public void Update(int barNumber)
        {            
            mapping.Update(barNumber);

            var bar = Bars[barNumber];
            var aO = orders.FindAll(p => p.IsActive);
            foreach (var order in aO)
            {
                order.Execute(bar);
                if (order.IsExecuted)
                {
                    if (order.OrderType != OrderType.StopLoss)
                    {
                        var position = new PositionLab(barNumber, order, this);
                        positions.Add(position);
                        ordersPositions.Add(order, position);

                        activePositions.Add(position);
                    }
                    else
                    {
                        //Нахожусь здесь

                        //ordersPositions.TryGetValue(order);
                        //var pos = (PositionLab)order;
                        
                        //order.SignalName
                    }
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
            var activeOrders = mapping.GetActiveOrders(barNumber).ToList();
            var result = (from order in activeOrders
                          select order.Order).ToList();
            return result;
        }

        public List<Order> GetOrdersBeforeBarOpened(int barNumber)
        {
            var o = (from order in orders
                                where order.BarNumber <= barNumber
                                select order).ToList();

            return o;
        }
    }
}