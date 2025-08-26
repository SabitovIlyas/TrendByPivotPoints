using System.Collections.Generic;
using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    public class Ticker
    {        
        public List<Bar> Bars { get; set; }
        public string Name { get; private set; }
        public Currency Currency { get; private set; }
        public int Shares { get; private set; }
        public double CommissionRate { get; private set; }
        public bool IsUSD { get; private set; }
        public double RateUSD { get; private set; }
        public Logger Logger { get; private set; }
        public List<Bar> InitBars { get; }

        public Ticker(string name, Currency currency, int shares, List<Bar> bars, Logger logger, 
            double commissionRate, bool isUSD, double rateUSD)
        {
            Name = name;
            Currency = currency;
            Shares = shares;
            Bars = bars;
            Logger = logger;
            CommissionRate = commissionRate;
            InitBars = bars;
            IsUSD = isUSD;
            RateUSD = rateUSD;
        }

        public void ResetBarsToInitBars()
        {
            Bars = InitBars;
        }
    }
}