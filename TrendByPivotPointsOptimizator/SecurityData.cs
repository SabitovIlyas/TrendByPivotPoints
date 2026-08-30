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

        /// <summary>Проскальзывание на сторону, в рублях. Необязательное седьмое
        /// поле файла инструментов; без него — ноль, как было раньше.</summary>
        public double SlippagePerSide;
    }
}