using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script.Handlers;

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
        public double GObuying { get; private set; } = double.NaN;
        public double GOselling { get; private set; } = double.NaN;
        public List<double> HighPrices { get; private set; }
        public List<double> LowPrices { get; private set; }

        private Currency currency;
        private int shares;
        private List<Order> orders = new List<Order>();
        private Dictionary<Order, Position> ordersPositions = new Dictionary<Order, Position>();
        private List<Order> activeOrders = new List<Order>();
        private OrderToPositionMapping mapping;
        private Logger logger;

        public SecurityLab(Currency currency, int shares, Logger logger)
        {
            this.currency = currency;
            this.shares = shares;
            this.logger = logger;
        }

        public SecurityLab(Currency currency, int shares,
            double GObuying, double GOselling, Logger logger)
        {
            this.currency = currency;
            this.shares = shares;
            this.GObuying = GObuying;
            this.GOselling = GOselling;
            this.logger = logger;
        }

        public SecurityLab(Currency currency, int shares, List<Bar> bars, Logger logger)
        {
            if (bars != null || bars.Count > 0)
                Name = bars.First().Ticker;
            this.currency = currency;
            this.shares = shares;
            Bars = bars;
            this.logger = logger;
            Initialize();
        }

        public SecurityLab(string securityName, Currency currency, int shares,
            double GObuying, double GOselling, List<Bar> bars, Logger logger)
        {
            Name = securityName;
            this.currency = currency;
            this.shares = shares;
            this.GObuying = GObuying;
            this.GOselling = GOselling;
            Bars = bars;
            this.logger = logger;
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
            mapping = new OrderToPositionMapping(Bars, this, logger);            
        }

        public Bar LastBar
        {
            get
            {
                return Bars?.Last();
            }
        }

        public Bar GetBar(int barNumber)
        {
            if (barNumber >= Bars.Count)
                return null;
            return Bars[barNumber];
        }        

        public int GetBarCompressedNumberFromBarBaseNumber(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public DateTime GetBarDateTime(int barNumber)
        {
            if (barNumber < Bars.Count)
                return Bars[barNumber].Date;
            return DateTime.MinValue;
        }

        public double GetBarClose(int barNumber)
        {
            if (barNumber < Bars.Count)
                return Bars[barNumber].Close;
            return double.NaN;
        }

        public double GetBarHigh(int barNumber)
        {
            if (barNumber < Bars.Count)
                return Bars[barNumber].High;
            return double.NaN;
        }

        public double GetBarLow(int barNumber)
        {
            if (barNumber < Bars.Count)
                return Bars[barNumber].Low;
            return double.NaN;
        }

        public double GetBarOpen(int barNumber)
        {
            if (barNumber < Bars.Count)
                return Bars[barNumber].Open;
            return double.NaN;
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
            return GetLastClosedPosition(barNumber, PositionSide.Long);
        }

        public Position GetLastClosedShortPosition(int barNumber)
        {
            return GetLastClosedPosition(barNumber, PositionSide.Short);
        }

        public Position GetLastClosedPosition(int barNumber, PositionSide positionSide)
        {
            var allPositions = mapping.GetPositions(barNumber);

            var positions = (from position in allPositions
                             where position.BarNumberClosePosition <= barNumber
                             && position.PositionSide == positionSide
                             select position.Position).ToList();
            if (positions.Count == 0)
                return null;

            return positions.Last();
        }

        public bool IsRealTimeActualBar(int barNumber)
        {
            return true;
        }

        public void ResetBarNumberToLastBarNumber()
        {
            BarNumber = Bars.Count - 1;
        }

        public Position GetLastActiveForSignal(string signalName, int barNumber)
        {
            var positions = mapping.GetActivePositions(barNumber);             
            var position = positions.FindLast(p => p.PositionOpenSignalName == signalName);
            if (position == null)
                return null;
            return position.Position;
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
        }
        
        public List<Order> GetActiveOrders(int barNumber)
        {
            var activeOrders = mapping.GetActiveOrders(barNumber).ToList();
            var result = (from order in activeOrders
                          select order.Order).ToList();
            return result;
        }

        public List<Order> GetOrders(int barNumber)
        {
            var orders = mapping.GetOrders(barNumber);

            var o = (from order in orders                     
                     select order.Order).ToList();

            return o;
        }

        public double GetProfit(int barNumber)
        {
            var profit = 0d;

            var closedPositionsMap = mapping.GetClosedPositions(barNumber);
            var closedPositions = (from position in closedPositionsMap
                             select position.Position).ToList();

            var uniqueClosedPositions = new List<Position>();
            foreach (var position in closedPositions)            
                if (!uniqueClosedPositions.Contains(position))
                    uniqueClosedPositions.Add(position);            
            
            foreach (var position in uniqueClosedPositions)            
                profit += position.GetProfit(barNumber);

            var activePositionsMap = mapping.GetActivePositions(barNumber);
            var activePositions = (from position in activePositionsMap
                                   select position.Position).ToList();

            var uniqueActivePositions = new List<Position>();
            foreach (var position in activePositions)
                if (!uniqueActivePositions.Contains(position))
                    uniqueActivePositions.Add(position);

            foreach (var position in uniqueActivePositions)
                profit += position.GetProfit(barNumber);

            return profit;
        }

        public List<Position> GetDeals(int barNumber)
        {
            var oP = mapping.GetActivePositions(barNumber);
            var cP = mapping.GetClosedPositions(barNumber);

            var od = (from position in oP
                      select position.Position).ToList();            

            var cd = (from position in cP
                      select position.Position).ToList();

            var result = new List<Position>();

            foreach (var position in cd)            
                if (!result.Contains(position))
                    result.Add(position);
            
            foreach (var position in od)            
                if (!result.Contains(position))
                result.Add(position);
            
            return result;
        }

        public List<Position> GetMetaDeals(int barNumber)
        {
            var deals = GetDeals(barNumber);
            List<Position> result = new List<Position>();
            Position metaDeal = null;
            var profit = 0d;
            var barNumberOpenPosition = 0;
            var barNumberClosePosition = 0;
            var positionSide = PositionSide.Null;
            var sumEntryPrice = 0d;
            var sumExitPrice = 0d;
            var contracts = 0;
            var avrEntryPrice = 0d;
            var avrExitPrice = 0d;

            foreach (var deal in deals)
            {
                if (deal.SignalNameForOpenPosition.Contains("1"))
                {
                    if (deal != deals.First())
                    {
                        avrEntryPrice = sumEntryPrice / contracts;
                        avrExitPrice = sumExitPrice / contracts;
                        metaDeal = new PositionLab(positionSide, this, profit,
                            barNumberOpenPosition, barNumberClosePosition, avrEntryPrice,
                            avrExitPrice, contracts);
                        result.Add(metaDeal);
                    }
                    
                    profit = deal.GetProfit(barNumber);
                    barNumberOpenPosition = deal.BarNumberOpenPosition;
                    barNumberClosePosition = deal.BarNumberClosePosition;
                    positionSide = deal.PositionSide;
                    contracts = deal.Contracts;
                    sumEntryPrice = deal.EntryPrice * deal.Contracts;

                    if (deal.ExitPrice != double.MinValue)
                        sumExitPrice = deal.ExitPrice * deal.Contracts;
                    else                    
                        sumExitPrice = GetBarClose(barNumber) * deal.Contracts;
                    

                    //средний вход, средний выход, количество лотов
                }
                else
                {
                    profit += deal.GetProfit(barNumber);
                    contracts += deal.Contracts;
                    sumEntryPrice += deal.EntryPrice * deal.Contracts;                    

                    if (deal.ExitPrice != double.MinValue)
                        sumExitPrice += deal.ExitPrice * deal.Contracts;
                    else
                        sumExitPrice += GetBarClose(barNumber) * deal.Contracts;
                }
            }

            avrEntryPrice = sumEntryPrice / contracts;
            avrExitPrice = sumExitPrice / contracts;

            metaDeal = new PositionLab(positionSide, this, profit, barNumberOpenPosition, 
                barNumberClosePosition, avrEntryPrice, avrExitPrice, contracts);            
            result.Add(metaDeal);

            return result;
        }
    }
}