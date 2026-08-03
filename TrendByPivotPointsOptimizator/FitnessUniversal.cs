using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using Account = TradingSystems.Account;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    /// <summary>
    /// Универсальная фитнес-функция: прогоняет стратегию на барах хромосомы и
    /// возвращает фактор восстановления, посчитанный после исключения лучших
    /// прибыльных сделок (проверка на устойчивость результата). Логика перенесена
    /// из FitnessDonchianChannel, но не зависит от конкретной стратегии.
    /// </summary>
    public class FitnessUniversal
    {
        /// <summary>Минимальное количество сделок; с меньшим хромосома отбраковывается.</summary>
        public int DealsCountCriteria { get; set; } = 0;

        /// <summary>Доля лучших прибыльных сделок, исключаемых перед расчётом.</summary>
        public double PrcntDealForExclude { get; set; } = 0.05;
        public bool IsCriteriaPassedNeedToCheck { get; set; } = true;

        private readonly SystemParameters parameters;
        private readonly ChromosomeUniversal chromosome;
        private readonly Starter starter;
        private int dealsCount;
        private Account account;
        private DealsStatistics tradeStatistics = new DealsStatistics();

        /// <summary>
        /// Бары, на которых гоняется стратегия: окно бэктеста, а для форвардного
        /// теста их подменяют на бары форвардного окна. Передаются явно, а не через
        /// общий Ticker, иначе хромосомы нельзя считать параллельно — они бы
        /// перетирали бары друг другу.
        /// </summary>
        public List<Bar> Bars { get; set; }

        /// <param name="chromosome">Хромосома, в которую сложить результат. null —
        /// когда считается соседняя точка окрестности: её результат идёт только
        /// в усреднение и ничего не перезаписывает.</param>
        public FitnessUniversal(SystemParameters parameters, ChromosomeUniversal chromosome,
            Starter starter, List<Bar> bars)
        {
            this.parameters = parameters;
            this.chromosome = chromosome;
            this.starter = starter;
            Bars = bars;

            if (chromosome != null)
                chromosome.Fitness = this;
        }

        /// <summary>
        /// Считает фитнес-функцию, ничего не записывая в хромосому — для соседних
        /// точек окрестности.
        /// </summary>
        public double CalculateFitnessValue()
        {
            return Calculate();
        }

        public void SetUpChromosomeFitnessValue(bool isCriteriaPassedNeedToCheck = true)
        {
            IsCriteriaPassedNeedToCheck = isCriteriaPassedNeedToCheck;
            chromosome.FitnessValue = Calculate();
            chromosome.DealsCount = dealsCount;
            chromosome.DealsStatistics = tradeStatistics;

            var profit = account.Equity - account.InitDeposit;
            chromosome.Profit = profit;
            var accountLab = account as AccountLab;
            chromosome.ProfitPrcnt = accountLab.GetProfitPrcnt();
            chromosome.MaxDrawDown = accountLab.GetMaxDrawDownPrcnt();
            chromosome.RecoveryFactor = accountLab.GetRecoveryFactor();
        }

        private double Calculate()
        {
            var starter = CloneStarterWithChromosomeBars();
            SystemRun(starter);

            var security = starter.GetSecurity();

            //Показатели считаем по этому прогону — он без пессимизации, то есть
            //описывает стратегию такой, какой она торгуется.
            var metaDeals = security.GetMetaDeals();
            dealsCount = metaDeals.Count;
            tradeStatistics = DealsStatistics.Calculate(metaDeals);
            account = starter.Account;

            var recoveryFactor = CheckCriteriaPassed(starter);
            if (double.IsNegativeInfinity(recoveryFactor))
                return double.NegativeInfinity;

            return Math.Round(recoveryFactor, 2);
        }

        private Starter CloneStarterWithChromosomeBars()
        {
            var clone = starter.CloneStarter();
            var security = clone.GetSecurity();
            if (security != null)
            {
                security.Bars = Bars;
                var securityLab = security as SecurityLab;
                securityLab.Initialize();   //не удалять! Пересоздаёт внутренние структуры под новые бары
            }
            return clone;
        }

        private double CheckCriteriaPassed(Starter system)
        {
            var recoveryFactor = double.NegativeInfinity;
            var security = system.GetSecurity();

            var deals = security.GetMetaDeals();
            var isQtyDealsEnough = deals.Count >= DealsCountCriteria;
            if (IsCriteriaPassedNeedToCheck && !isQtyDealsEnough)
                return recoveryFactor;

            if (!IsCriteriaPassedNeedToCheck)
                return CalcRecoveryFactor(system.Account);

            //Исключаем лучшие прибыльные сделки и пересчитываем результат: если он
            //держится не на паре удачных сделок, стратегия устойчивее.
            var dealsForExclude = deals.Where(d => d.GetProfit() > 0).ToList();
            var qtyDealForExclude = (int)Math.Ceiling(PrcntDealForExclude * dealsForExclude.Count);
            dealsForExclude = dealsForExclude.OrderByDescending(d => d.GetProfit())
                .Take(qtyDealForExclude).ToList();

            var nonTradingPeriods = new List<NonTradingPeriod>();
            foreach (var deal in dealsForExclude)
            {
                var period = new NonTradingPeriod();
                period.BarStart = deal.BarNumberOpenPosition - 1;
                period.BarStop = deal.BarNumberClosePosition - 1;
                nonTradingPeriods.Add(period);
            }

            var newSystem = CloneStarterWithChromosomeBars();
            newSystem.NonTradingPeriods = nonTradingPeriods;
            SystemRun(newSystem);

            return CalcRecoveryFactor(newSystem.Account);
        }

        private void SystemRun(Starter system)
        {
            system.SetParameters(parameters);
            system.Initialize();
            system.Run();
        }

        private double CalcRecoveryFactor(Account account)
        {
            var accountLab = (AccountLab)account;
            return accountLab.GetRecoveryFactor();
        }
    }
}
