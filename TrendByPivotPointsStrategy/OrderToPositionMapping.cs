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

        public OrderToPositionMapping(List<Bar> bars, Security security)
        {
            this.bars = bars;
            this.security = security;
        }

        public void CreateOpenLimitOrder(int barNumber, int contracts, double entryPricePlanned,
            string signalNameForOpenPosition, bool isConverted)
        {
            var converter = new Converter(isConverted);
            var positionSide = isConverted ? PositionSide.Short : PositionSide.Long;

            var map = orders.Find(p => p.SignalName == signalNameForOpenPosition);
            map?.Order.Cancel(barNumber);

            var order = new Order(barNumber, positionSide, entryPricePlanned, contracts,
                  signalNameForOpenPosition);
            map = new OrderToPositionMap(order);
            orders.Add(map);
        }

        public void CreateCloseLimitOrder(int barNumber, double stopPrice,
            string signalNameForClosePosition, string notes, Position position)
        {
            var activeOrders = GetActiveOrders(barNumber);
            var order = activeOrders.Find(p => p.SignalName == signalNameForClosePosition);

            if (order != null && order.Price == stopPrice)
                return;

            var closeOrder = new Order(barNumber, position.PositionSide, stopPrice, position.Contracts,
                signalNameForClosePosition, OrderType.StopLossLimit);
            orders.Add(new OrderToPositionMap(closeOrder, position));            
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
                                   where order.BarNumberClosePosition <= barNumber
                                   select order).ToList();

            return closedPositions;
        }
    }
}