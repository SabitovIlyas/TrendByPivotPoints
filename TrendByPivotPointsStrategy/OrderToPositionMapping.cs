using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Utils;

namespace TradingSystems
{
    public class OrderToPositionMapping
    {
        private List<OrderToPositionMap> maps = new List<OrderToPositionMap>();
        public List<OrderToPositionMap> activeOrders = new List<OrderToPositionMap>();

        private List<Bar> bars;
        public Security security;
        private Logger logger;
        private int barNumber;

        private HashSet<OrderToPositionMap> activePositions = new HashSet<OrderToPositionMap>();
        private HashSet<OrderToPositionMap> closedPositions = new HashSet<OrderToPositionMap>();
        private HashSet<OrderToPositionMap> positions = new HashSet<OrderToPositionMap>();

        private List<List<OrderToPositionMap>> activePositionsPerBarNumber = new
            List<List<OrderToPositionMap>>();
        private List<List<OrderToPositionMap>> closedPositionsPerBarNumber = new
            List<List<OrderToPositionMap>>();
        private List<List<OrderToPositionMap>> positionsPerBarNumber = new
            List<List<OrderToPositionMap>>();
        //private List<List<OrderToPositionMap>> ordersPerBarNumber = new
            //List<List<OrderToPositionMap>>();
        //private List<List<OrderToPositionMap>> activeOrdersPerBarNumber = new
        //    List<List<OrderToPositionMap>>();


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

            //var maps = activeOrders.FindAll(p => p.SignalName == signalNameForOpenPosition);
            //foreach (var m in maps)
            //    m.Order.Cancel(barNumber);

            var o = activeOrders.Find(p => p.SignalName == signalNameForOpenPosition);
            if (o != null)
            {
                o.Order.Cancel(barNumber);
                activeOrders.Remove(o);
            }

            var order = new Order(barNumber, positionSide, entryPricePlanned, contracts,
                  signalNameForOpenPosition);
            var map = new OrderToPositionMap(order);
            maps.Add(map);
            activeOrders.Add(map);
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
            maps.Add(new OrderToPositionMap(closeOrder, position));
            
            if (order != null)
                order.Order.Cancel(barNumber);
            activeOrders.Remove(order);
        }

        public void CreateOpenMarketOrder(int barNumber, int contracts,
            string signalNameForOpenPosition, bool isConverted)
        {
            Log("Создаём рыночный ордер для открытия позиции. {0} = {1};", nameof(barNumber),
                barNumber);
            var positionSide = isConverted ? PositionSide.Short : PositionSide.Long;

            var o = activeOrders.Find(p => p.SignalName == signalNameForOpenPosition);
            if (o != null)
            {
                o.Order.Cancel(barNumber);
                activeOrders.Remove(o);
            }

            var order = new Order(barNumber, positionSide, double.NaN, contracts,
                signalNameForOpenPosition, OrderType.Market);
            var map = new OrderToPositionMap(order);
            maps.Add(map);
            activeOrders.Add(map);
        }

        public void CreateCloseMarketOrder(int barNumber,
            string signalNameForClosePosition, string notes, Position position)
        {
            var activeOrders = GetActiveOrders(barNumber);
            var order = activeOrders.Find(p => p.SignalName == signalNameForClosePosition + notes);
            if (order != null)
                return;

            //Отменяем прочие закрывающие ордера этой позиции (например, стоп-лосс),
            //иначе позиция может закрыться дважды.
            foreach (var o in activeOrders)
                if (o.Position == position)
                    o.Order.Cancel(barNumber);

            var closeOrder = new Order(barNumber, position.PositionSide, double.NaN,
                position.Contracts, signalNameForClosePosition + notes, OrderType.StopLossMarket);
            maps.Add(new OrderToPositionMap(closeOrder, position));
        }

        public void Update(int barNumber)//скорее всего, мне придётся реализовать работу всех связанных классов таким образом, что номер бара должен обновлять классы только вперёд. Нужен какой-то внутренний индекс, что ли. Это ускорит работу многих методов.
        {
            try
            {
                this.barNumber = barNumber;
                var bar = bars[barNumber];
                var activeOrders = GetActiveOrders(barNumber);                

                foreach (var order in activeOrders)
                {
                    if (order.Execute(bar, barNumber))
                    {
                        if (order.OrderType == OrderType.Limit || order.OrderType == OrderType.Market)
                        {
                            var position = new PositionLab(barNumber, order.Order, security);
                            order.Position = position;

                            if (!positions.Contains(order))
                                positions.Add(order);
                            if (!activePositions.Contains(order))
                                activePositions.Add(order);
                        }
                        else if (order.OrderType == OrderType.StopLossLimit)
                        {
                            var position = order.Position;
                            position.CloseAtStop(barNumber, order.ExecutedPrice, order.SignalName);
                            if (!closedPositions.Contains(order))
                            {
                                closedPositions.Add(order);
                                var o = activePositions.Find(p => p.Position == order.Position);
                                activePositions.Remove(o);
                            }
                        }
                        else if (order.OrderType == OrderType.StopLossMarket)
                        {
                            var position = order.Position;
                            position.CloseAtMarket(barNumber, order.ExecutedPrice, order.SignalName);
                            if (!closedPositions.Contains(order))
                            {
                                closedPositions.Add(order);
                                var o = activePositions.Find(p => p.Position == order.Position);
                                activePositions.Remove(o);
                            }
                        }
                    }
                }



                var cPos = new List<OrderToPositionMap>();
                foreach (var p in closedPositions)                
                    cPos.Add(p);                
                closedPositionsPerBarNumber.Add(cPos);

                var aPos = new List<OrderToPositionMap>();
                foreach (var p in activePositions)
                    aPos.Add(p);
                activePositionsPerBarNumber.Add(aPos);

                var pos = new List<OrderToPositionMap>();
                foreach (var p in positions)
                    pos.Add(p);
                positionsPerBarNumber.Add(pos);

                //var orders = new List<OrderToPositionMap>();
                //foreach (var m in maps)
                //    orders.Add(m);
                //ordersPerBarNumber.Add(orders);

                //var aO = new List<OrderToPositionMap>();
                //foreach (var o in activeOrders)
                //    aO.Add(o);
                //activeOrdersPerBarNumber.Add(aO);
            }
            catch
            {                
            }            
        }       

        public List<OrderToPositionMap> GetActiveOrders(int barNumber)
        {
            //if (barNumber == this.barNumber)
            //    return activeOrders;
            //else
            //    return activeOrdersPerBarNumber[barNumber];

            var activeOrders = (from order in maps
                                where order.BarNumber <= barNumber
                                && barNumber < order.BarNumberSinceOrderIsNotActive
                                select order).ToList();
            return activeOrders;
        }      

        public List<OrderToPositionMap> GetActivePositions(int barNumber)//!
        {
            if (barNumber == this.barNumber)
                return activePositions.ToList();
            else
                return activePositionsPerBarNumber[barNumber];            
        }       

        public List<OrderToPositionMap> GetOrders(int barNumber)//!
        {
            if (barNumber == this.barNumber)
                return maps;
            else
                throw new Exception("Не реализовал, так как комп не вывозит");
        }        

        public List<OrderToPositionMap> GetPositions(int barNumber)//!
        {
            if (barNumber == this.barNumber)
                return positions.ToList();
            else
                return positionsPerBarNumber[barNumber];
        }       

        public List<OrderToPositionMap> GetClosedPositions(int barNumber)//!
        {
            if (barNumber == this.barNumber)
                return closedPositions.ToList();
            else
                return closedPositionsPerBarNumber[barNumber];            
        }
    }
}