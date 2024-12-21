using System.Collections.Generic;

namespace TradingSystems
{
    public class OrderToPositionMapping
    {
        private List<Order> orders = new List<Order>();

        public void CreateOpenLimitOrder(int barNumber, int contracts, double entryPricePlanned,
            string signalNameForOpenPosition, bool isConverted)
        {
            var converter = new Converter(isConverted);
            var positionSide = isConverted ? PositionSide.Short : PositionSide.Long;

            var order = orders.Find(p => p.SignalName == signalNameForOpenPosition);
            order?.Cancel(barNumber);

            order = new Order(barNumber, positionSide, entryPricePlanned, contracts,
                    signalNameForOpenPosition);
            orders.Add(order);
        }

        public void CreateCloseLimitOrder(int barNumber, double stopPrice,
            string signalNameForClosePosition, string notes, Position position)
        {

        }

        public void CreateOpenMarketOrder(int barNumber, int contracts, double entryPricePlanned,
            string signalNameForOpenPosition, bool isConverted)
        {
        }

        public void CreateCloseMarketOrder(int barNumber, double stopPrice,
        string signalNameForClosePosition, string notes, Position position)
        {

        }

        public Order FindOrder(string signalName)
        {
            return orders.Find(p => p.SignalName == signalName);
        }
    }
}