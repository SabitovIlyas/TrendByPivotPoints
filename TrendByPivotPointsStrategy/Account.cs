using System.Collections.Generic;

namespace TradingSystems
{
    public abstract class Account
    {
        public abstract double InitDeposit { get; }
        public abstract double Equity { get; }
        public abstract double FreeBalance { get; }
        Logger Logger { get; set; }        
        protected Logger logger;
        protected double equity;
        protected List<Security> securities;
        protected Dictionary<Security, Position> lastLongPositionsClosed = new Dictionary<Security, Position>();
        protected Dictionary<Security, Position> lastShortPositionsClosed = new Dictionary<Security, Position>();        
        protected Currency currency;

        public virtual void Update(int barNumber)
        {
            foreach (var security in securities)
            {                
                if (lastLongPositionsClosed.TryGetValue(security, out Position lastLongPositionClosedPrevious))
                {
                    var lastLongPositionClosed = security.GetLastClosedLongPosition(barNumber);
                    if (lastLongPositionClosed != lastLongPositionClosedPrevious)
                    {
                        lastLongPositionsClosed.Remove(security);
                        lastLongPositionsClosed.Add(security, lastLongPositionClosed);

                        equity += lastLongPositionClosed.GetProfit(barNumber);
                        var message = string.Format("Активная длинная позиция закрылась: Номер бара = {0}; Цена открытия = {1}; Прибыль = {2}; Equity = {3}",
                            lastLongPositionClosed.BarNumberOpenPosition, lastLongPositionClosed.EntryPrice, lastLongPositionClosed.GetProfit(barNumber), equity);
                        logger.Log(message);
                    }
                }
                else
                {
                    var lastLongPositionClosed = security.GetLastClosedLongPosition(barNumber);
                    if (lastLongPositionClosed != null)
                    {
                        lastLongPositionsClosed.Add(security, lastLongPositionClosed);
                        equity += lastLongPositionClosed.GetProfit(barNumber);
                        var message = string.Format("Активная длинная позиция закрылась: Номер бара = {0}; Цена открытия = {1}; Прибыль = {2}; Equity = {3}",
                            lastLongPositionClosed.BarNumberOpenPosition, lastLongPositionClosed.EntryPrice, lastLongPositionClosed.GetProfit(barNumber), equity);
                        logger.Log(message);
                    }
                }

                if (lastShortPositionsClosed.TryGetValue(security, out Position lastShortPositionClosedPrevious))
                {
                    var lastShortPositionClosed = security.GetLastClosedShortPosition(barNumber);
                    if (lastShortPositionClosed != lastShortPositionClosedPrevious)
                    {
                        lastShortPositionsClosed.Remove(security);
                        lastShortPositionsClosed.Add(security, lastShortPositionClosed);

                        equity += lastShortPositionClosed.GetProfit(barNumber);
                        var message = string.Format("Активная короткая позиция закрылась: Номер бара = {0}; Цена открытия = {1}; Прибыль = {2}; Equity = {3}",
                            lastShortPositionClosed.BarNumberOpenPosition, lastShortPositionClosed.EntryPrice, lastShortPositionClosed.GetProfit(barNumber), equity);
                        logger.Log(message);
                    }
                }
                else
                {
                    var lastShortPositionClosed = security.GetLastClosedShortPosition(barNumber);
                    if (lastShortPositionClosed != null)
                    {
                        lastShortPositionsClosed.Add(security, lastShortPositionClosed);
                        equity += lastShortPositionClosed.GetProfit(barNumber);
                        var message = string.Format("Активная короткая позиция закрылась: Номер бара = {0}; Цена открытия = {1}; Прибыль = {2}; Equity = {3}",
                            lastShortPositionClosed.BarNumberOpenPosition, lastShortPositionClosed.EntryPrice, lastShortPositionClosed.GetProfit(barNumber), equity);
                        logger.Log(message);
                    }
                }
            }
        }        
    }
}
