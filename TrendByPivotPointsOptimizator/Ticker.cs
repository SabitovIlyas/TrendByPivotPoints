using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    internal class Ticker
    {        
        public List<Bar> Bars { get; private set; }
        public string Name { get; private set; }
        public Currency Currency { get; private set; }
        public int Shares { get; private set; }
        public double CommissionRate { get; private set; }
        public Logger Logger { get; private set; }        

        public Ticker(string name, Currency currency, int shares, List<Bar> bars, Logger logger, double commissionRate)
        {
            Name = name;
            Currency = currency;
            Shares = shares;
            Bars = bars;
            Logger = logger;
            CommissionRate = commissionRate;
        }
    }
}