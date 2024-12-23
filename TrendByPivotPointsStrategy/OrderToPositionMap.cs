using System;

namespace TradingSystems
{
    public class OrderToPositionMap
    {
        public Order Order { get; private set; }
        public Position Position { get; set; }
        public string SignalName
        {
            get
            {
                if (Order == null)
                    throw new NullReferenceException("Order == null");
                return Order.SignalName;
            }
        }
        public double Price
        {
            get
            {
                if (Order == null)
                    throw new NullReferenceException("Order == null");
                return Order.Price;
            }
        }
        public int BarNumber
        {
            get
            {
                if (Order == null)
                    throw new NullReferenceException("Order == null");
                return Order.BarNumber;
            }
        }

        public int BarNumberSinceOrderIsNotActive
        {
            get
            {
                if (Order == null)
                    throw new NullReferenceException("Order == null");
                return Order.BarNumberSinceOrderIsNotActive;
            }
        }

        public OrderToPositionMap(Order order)
        {
            Order = order;
        }

        public OrderToPositionMap(Order order, Position position)
        {
            Order = order;
            Position = position;
        }

        public void Execute(Bar bar)
        {
            Order.Execute(bar);
        }
    }
}