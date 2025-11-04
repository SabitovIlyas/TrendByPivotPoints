using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class OrderToPositionMapping
    {
        private List<OrderToPositionMap> orders = new List<OrderToPositionMap>();
        private List<Bar> bars;
        public Security security;
        private Logger logger;

        public OrderToPositionMapping(List<Bar> bars, Security security, Logger logger)
        {
            this.bars = bars;
            this.security = security;
            this.logger = logger;
        }

        public void CreateOpenLimitOrder(int barNumber, int contracts, double entryPricePlanned,
            string signalNameForOpenPosition, bool isConverted)
        {
            Log("Создаём лимитный ордер для открытия позиции. {0} = {1}; {2} = {3};", nameof(barNumber), barNumber, 
                nameof(entryPricePlanned), entryPricePlanned);
            var converter = new Converter(isConverted);
            var positionSide = isConverted ? PositionSide.Short : PositionSide.Long;

            var maps = orders.FindAll(p => p.SignalName == signalNameForOpenPosition);

            foreach (var m in  maps)
                m.Order.Cancel(barNumber);

            var order = new Order(barNumber, positionSide, entryPricePlanned, contracts,
                  signalNameForOpenPosition);
            var map = new OrderToPositionMap(order);
            orders.Add(map);        
        }

        protected void Log(string text, params object[] args)
        {
            text = string.Format(text, args);
            Log(text);
        }

        protected void Log(string text)
        {
            logger.Log("{0}: {1}", nameof(security), text);
        }        

        public void CreateCloseLimitOrder(int barNumber, double stopPrice,
            string signalNameForClosePosition, string notes, Position position)
        {
            var activeOrders = GetActiveOrders(barNumber);
            var order = activeOrders.Find(p => p.SignalName == signalNameForClosePosition + notes);

            if (order != null && order.Price == stopPrice)
                return;

            var closeOrder = new Order(barNumber, position.PositionSide, stopPrice, position.Contracts,
                signalNameForClosePosition + notes, OrderType.StopLossLimit);
            orders.Add(new OrderToPositionMap(closeOrder, position));
            
            if (order != null)
                order.Order.Cancel(barNumber);
        }

        public void CreateOpenMarketOrder(int barNumber, int contracts, double entryPricePlanned,
            string signalNameForOpenPosition, bool isConverted)
        {
        }

        public void CreateCloseMarketOrder(int barNumber, double stopPrice,
        string signalNameForClosePosition, string notes, Position position)
        {

        }

        public void Update(int barNumber)
        {
            try
            {
                var bar = bars[barNumber];
                var activeOrders = GetActiveOrders(barNumber);

                foreach (var order in activeOrders)
                {
                    if (order.Execute(bar, barNumber))
                    {
                        if (order.OrderType == OrderType.Limit)
                        {
                            var position = new PositionLab(barNumber, order.Order, security);
                            order.Position = position;
                        }
                        else if (order.OrderType == OrderType.StopLossLimit)
                        {
                            var position = order.Position;
                            position.CloseAtStop(barNumber, order.ExecutedPrice, order.SignalName);
                        }
                        else if (order.OrderType == OrderType.Market)
                        {

                        }
                        else if (order.OrderType == OrderType.StopLossMarket)
                        {
                            var position = order.Position;
                            position.CloseAtMarket(barNumber, order.ExecutedPrice, order.SignalName);
                        }
                    }
                }
            }
            catch
            {
                
            }            
        }

        public List<OrderToPositionMap> GetActiveOrders(int barNumber)
        {
            var activeOrders = (from order in orders
                                where order.BarNumber <= barNumber
                                && barNumber < order.BarNumberSinceOrderIsNotActive
                                select order).ToList();

            return activeOrders;
        }

        public List<OrderToPositionMap> GetActivePositions(int barNumber)
        {
            var activePositions = (from order in orders
                                where order.BarNumberOpenPosition <= barNumber
                                && barNumber < order.BarNumberClosePosition
                                select order).ToList();

            return activePositions;
        }

        public List<OrderToPositionMap> GetOrders(int barNumber)
        {
            var activeOrders = (from order in orders
                                where order.BarNumber <= barNumber                                
                                select order).ToList();

            return activeOrders;
        }

        public List<OrderToPositionMap> GetPositions(int barNumber)
        {
            var positions = (from order in orders
                                   where order.BarNumberOpenPosition <= barNumber                                   
                                   select order).ToList();

            return positions;
        }

        public List<OrderToPositionMap> GetClosedPositions(int barNumber)
        {
            var closedPositions = (from order in orders
                                   where order.BarNumberClosePosition <= barNumber &&
                                   order.BarNumberClosePosition < int.MaxValue
                                   select order).ToList();

            return closedPositions;
        }
    }
}