using System;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public enum OrderType
    {
        Market, Limit, StopLossLimit, StopLossMarket
    }

    public class Order
    {
        public int BarNumber { get; private set; }
        public PositionSide PositionSide { get; private set; }
        public double Price { get; private set; }
        public int Contracts { get; private set; }
        public string SignalName { get; private set; }
        public bool IsActive { get; private set; } = true;
        private Converter converter;
        public double ExecutedPrice { get; private set; } = double.NaN;
        public bool IsExecuted { get; private set; } = false;
        public OrderType OrderType { get; private set; }
        public int BarNumberSinceOrderIsNotActive { get; private set; } = int.MaxValue;
        private Position position;

        public Order(int barNumber, PositionSide positionSide, double price, int contracts,
            string signalName, OrderType orderType = OrderType.Limit)
        {
            BarNumber = barNumber;
            PositionSide = positionSide;
            Price = price;
            Contracts = contracts;
            SignalName = signalName;
            OrderType = orderType;
            converter = new Converter(isConverted: positionSide == PositionSide.Short);
        }

        public Order(int barNumber, double price, string signalName, Position position,
            OrderType orderType = OrderType.Limit)
        {
            BarNumber = barNumber;
            PositionSide = position.PositionSide;
            Price = price;
            Contracts = position.Contracts;
            SignalName = signalName;
            OrderType = orderType;
            converter = new Converter(isConverted: PositionSide == PositionSide.Short);

            this.position = position;
        }

        public bool Execute(Bar bar, int barNumber)
        {
            if (PositionSide == PositionSide.Null) return false;

            if (converter.IsGreaterOrEqual(bar.Open, Price) && OrderType != OrderType.StopLossLimit)
            {
                ExecutedPrice = bar.Open;
                BarNumberSinceOrderIsNotActive = barNumber;
                return true;
            }
            else if (converter.IsGreaterOrEqual(converter.GetBarHigh(bar), Price) &&
                OrderType != OrderType.StopLossLimit)
            {
                ExecutedPrice = Price;
                BarNumberSinceOrderIsNotActive = barNumber;
                return true;
            }
            //else if (converter.IsLessOrEqual(bar.Open, Price) && (OrderType != OrderType.StopLossLimit))
            //{
            //    ExecutedPrice = bar.Open;
            //    BarNumberSinceOrderIsNotActive = barNumber;
            //    return true;
            //}
            //else if (converter.IsLessOrEqual(converter.GetBarLow(bar), Price) &&
            //    (OrderType == OrderType.StopLossLimit))
            //{
            //    ExecutedPrice = Price;
            //    BarNumberSinceOrderIsNotActive = barNumber;
            //    return true;
            //}
            else if (converter.IsLessOrEqual(converter.GetBarLow(bar), Price) &&
                (OrderType == OrderType.StopLossLimit))
            {
                ExecutedPrice = Price;
                BarNumberSinceOrderIsNotActive = barNumber;
                return true;
            }
            else if (OrderType == OrderType.Market || OrderType == OrderType.StopLossMarket)
            {
                ExecutedPrice = bar.Open;
                BarNumberSinceOrderIsNotActive = barNumber;
                return true;
            }

            return false;
        }

        public void Cancel(int barNumber)
        {
            IsActive = false;
            BarNumberSinceOrderIsNotActive = barNumber;
        }
    }
}