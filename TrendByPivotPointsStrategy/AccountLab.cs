using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class AccountLab : Account
    {
        private ISecurity sec;
        private double equity;
        Logger logger = new NullLogger();

        public Logger Logger
        {
            get
            {
                return logger;
            }

            set
            {
                logger = value;
            }
        }        

        public double InitDeposit
        {
            get
            {
                if (sec == null)
                    return 0;
                return sec.InitDeposit;
            }
        }

        public double Equity
        {
            get
            {
                if (sec == null)
                    return 0;
                return equity;
            }
        }        

        public double GObying => 0.45;

        public double GOselling => 0.40;

        public double Rate { get { return rate; } set { rate = value; } }
        private double rate;

        public ISecurity Security
        {
            get
            {
                return sec;
            }
            set
            {
                sec = value;
            }
        }

        public double FreeBalance => Equity;

        public AccountLab(ISecurity sec)
        {
            this.sec = sec;
            equity = sec.InitDeposit;            
        }

        public void Initialize(List<Security> securities)
        {
            this.securities = securities;
            var securityFirst = securities[0];
            var security = securityFirst as TSLabSecurity;
            sec = security.security;
            equity = sec.InitDeposit;
            lastLongPositionsClosed = new Dictionary<Security, Position>();
            lastShortPositionsClosed = new Dictionary<Security,Position>();
        }

        public void Update(int barNumber)
        {
            foreach(var security in securities)
            {
                if (lastLongPositionsClosed.TryGetValue(security, out Position lastLongPositionClosedPrevious))
                {
                    var lastLongPositionClosed = security.GetLastClosedLongPosition(barNumber);
                    if (lastLongPositionClosed != lastLongPositionClosedPrevious)
                    {
                        lastLongPositionsClosed.Remove(security);
                        lastLongPositionsClosed.Add(security, lastLongPositionClosed);

                        equity += lastLongPositionClosed.Profit;
                        var message = string.Format("Активная длинная позиция закрылась: Номер бара = {0}; Цена открытия = {1}; Прибыль = {2}; Equity = {3}",
                            lastLongPositionClosed.BarNumber, lastLongPositionClosed.EntryPrice, lastLongPositionClosed.Profit, equity);
                        logger.Log(message);
                    }
                }
                else
                {
                    var lastLongPositionClosed = security.GetLastClosedLongPosition(barNumber);
                    if (lastLongPositionClosed != null)
                    {
                        lastLongPositionsClosed.Add(security, lastLongPositionClosed);
                        equity += lastLongPositionClosed.Profit;
                        var message = string.Format("Активная длинная позиция закрылась: Номер бара = {0}; Цена открытия = {1}; Прибыль = {2}; Equity = {3}",
                            lastLongPositionClosed.BarNumber, lastLongPositionClosed.EntryPrice, lastLongPositionClosed.Profit, equity);
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

                        equity += lastShortPositionClosed.Profit;
                        var message = string.Format("Активная короткая позиция закрылась: Номер бара = {0}; Цена открытия = {1}; Прибыль = {2}; Equity = {3}",
                            lastShortPositionClosed.BarNumber, lastShortPositionClosed.EntryPrice, lastShortPositionClosed.Profit, equity);
                        logger.Log(message);
                    }
                }
                else
                {
                    var lastShortPositionClosed = security.GetLastClosedShortPosition(barNumber);
                    if (lastShortPositionClosed != null)
                    {
                        lastShortPositionsClosed.Add(security, lastShortPositionClosed);
                        equity += lastShortPositionClosed.Profit;
                        var message = string.Format("Активная короткая позиция закрылась: Номер бара = {0}; Цена открытия = {1}; Прибыль = {2}; Equity = {3}",
                            lastShortPositionClosed.BarNumber, lastShortPositionClosed.EntryPrice, lastShortPositionClosed.Profit, equity);
                        logger.Log(message);
                    }
                }
            }
        }
        
        private List<Security> securities;
        private Dictionary<Security, Position> lastLongPositionsClosed;
        private Dictionary<Security, Position> lastShortPositionsClosed;
    }
}