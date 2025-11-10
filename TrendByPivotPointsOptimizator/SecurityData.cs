using TradingSystems;

namespace TrendByPivotPointsOptimizator
{
    public struct SecurityData
    {
        public string Name;
        public Currency Currency;
        public double Shares;
        public double CommissionRate;
        public bool IsUSD;
        public double RateUSD;
    }
}