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

        /// <summary>Порог просадки, %; 0 — не штрафовать.</summary>
        public double MaxDrawDownPrcnt { get; set; } = 0;

        /// <summary>Порог доли выигрышных сделок, %; 0 — не штрафовать.</summary>
        public double MinWinRatePrcnt { get; set; } = 0;

        /// <summary>Жёсткость штрафов.</summary>
        public double PenaltyPower { get; set; } = 1;
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


        public void SetUpChromosomeFitnessValue(bool isCriteriaPassedNeedToCheck = true)
        {
            IsCriteriaPassedNeedToCheck = isCriteriaPassedNeedToCheck;
            var fitness = Calculate();

            chromosome.DealsCount = dealsCount;
            chromosome.DealsStatistics = tradeStatistics;

            var profit = account.Equity - account.InitDeposit;
            chromosome.Profit = profit;
            var accountLab = account as AccountLab;
            chromosome.ProfitPrcnt = accountLab.GetProfitPrcnt();
            chromosome.MaxDrawDown = accountLab.GetMaxDrawDownPrcnt();
            chromosome.RecoveryFactor = accountLab.GetRecoveryFactor();

            chromosome.FitnessValue = ApplyPenalties(fitness, chromosome.MaxDrawDown,
                tradeStatistics.WinRatePrcnt);
        }

        /// <summary>
        /// Приводит неопределённую оценку к отбраковке. Фактор восстановления — это
        /// прибыль, делённая на просадку: без сделок обе равны нулю и получается
        /// «не число», а при нулевой просадке с прибылью — бесконечность. И то и
        /// другое означает, что оценивать нечего: стратегия либо не торговала, либо
        /// сделала одну сделку без единого отката, чего на четырёх годах не бывает.
        ///
        /// Отдельно важно, что «не число» служит признаком «ещё не считали»: пока
        /// оценка могла им остаться после расчёта, такие хромосомы пересчитывались
        /// каждое поколение и путались с непосчитанными.
        /// </summary>
        public static double RejectIfNotFinite(double fitness)
        {
            if (double.IsNaN(fitness) || double.IsPositiveInfinity(fitness))
                return double.NegativeInfinity;

            return fitness;
        }

        /// <summary>
        /// Считает фитнес-функцию, ничего не записывая в хромосому — для соседних
        /// точек окрестности. Штрафы применяются те же, иначе соседей судили бы
        /// по другому правилу, чем центр.
        /// </summary>
        public double CalculateFitnessValue()
        {
            var fitness = Calculate();
            var accountLab = account as AccountLab;

            return ApplyPenalties(fitness, accountLab.GetMaxDrawDownPrcnt(),
                tradeStatistics.WinRatePrcnt);
        }

        /// <summary>
        /// Снижает оценку за нарушение порогов просадки и доли выигрышных сделок.
        /// Штраф мягкий, а не отбраковка: генетическому алгоритму нужен градиент —
        /// если бы всё нарушающее порог получало минус бесконечность, поиск ослеп бы
        /// в тот момент, когда порог не проходит вся стартовая популяция.
        /// </summary>
        public double ApplyPenalties(double fitness, double maxDrawDownPrcnt,
            double winRatePrcnt)
        {
            if (double.IsNegativeInfinity(fitness) || double.IsNaN(fitness))
                return fitness;

            var penalty = 1d;

            if (MaxDrawDownPrcnt > 0 && maxDrawDownPrcnt > MaxDrawDownPrcnt)
                penalty *= Math.Pow(MaxDrawDownPrcnt / maxDrawDownPrcnt, PenaltyPower);

            if (MinWinRatePrcnt > 0 && winRatePrcnt < MinWinRatePrcnt)
                penalty *= Math.Pow(winRatePrcnt / MinWinRatePrcnt, PenaltyPower);

            if (penalty >= 1)
                return fitness;

            //У нуля штраф вырождается, а делить на него нельзя.
            penalty = Math.Max(penalty, 1e-6);

            //Убыточную стратегию штраф должен делать хуже, а не лучше: умножение
            //на долю меньше единицы приблизило бы отрицательную оценку к нулю.
            var result = fitness >= 0 ? fitness * penalty : fitness / penalty;
            return Math.Round(result, 2);
        }

        private double Calculate()
        {
            return RejectIfNotFinite(CalculateRecoveryFactor());
        }

        private double CalculateRecoveryFactor()
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
