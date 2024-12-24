using System;

namespace TradingSystems
{
    public class OrderToPositionMap
    {
        public Order Order { get; private set; }
        public Position Position { get; set; }
        public OrderType OrderType
        {
            get
            {
                if (Order == null)
                    throw new NullReferenceException("Order == null");
                return Order.OrderType;
            }
        }
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

        public double  ExecutedPrice
        {
            get
            {
                if (Order == null)
                    throw new NullReferenceException("Order == null");
                return Order.ExecutedPrice;
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

        public int BarNumberOpenPosition
        {
            get
            {
                if (Position == null)
                    return int.MaxValue;
                return Position.BarNumberOpenPosition;
            }
        }

        public int BarNumberClosePosition
        {
            get
            {
                if (Position == null)
                    return int.MaxValue;
                return Position.BarNumberClosePosition;
            }
        }

        public string PositionOpenSignalName
        {
            get
            {
                if (Position == null)
                    return string.Empty;
                return Position.SignalNameForOpenPosition;
            }
        }
        public string PositionCloseSignalName
        {
            get
            {
                if (Position == null)
                    return string.Empty;
                return Position.SignalNameForClosePosition;
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

        public bool Execute(Bar bar, int barNumber)
        {
            return Order.Execute(bar, barNumber);
        }
    }
}