using System;
using System.Linq;
using TradingSystems;
using TrendByPivotPointsStarter;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class FitnessDonchianChannel
    {
        private Security security;
        private ChromosomeDonchianChannel chromosome;
        private Optimizator optimizator;
        private Starter system;

        public FitnessDonchianChannel(Security security, ChromosomeDonchianChannel chromosome, 
            Optimizator optimizator, Starter system)
        {
            this.security = security;
            this.chromosome = chromosome;
            this.optimizator = optimizator;
            this.system = system;
            Calc();
        }
        
        private void Calc()
        {
            //Я здесь
            //Параметры для оптимизации: быстрая доунчиан, медленная доунчиан, период АТР,
            //лимит открытых позиций, коэф. АТР для открытия позиции, коэф. АТР для стоп-лосса
            optimizator.GetOptimalParametersPercent(
                points:null,
                dimension: 6, // Количество параметров  
                radiusNeighbourInPercent:new int[3] { 5, 5, 5 },
                barrier: 1.0, // Пороговое значение
                isCheckedPass: true); // Проверка на прохождение барьера            

            chromosome.FitnessPassed = CalcIsPassed();
            chromosome.FitnessValue = CalcValue();
        }

        private bool CalcIsPassed()
        {   
            var deals = security.GetMetaDeals();
            var qtyDealsCase = deals.Count >= 30;
            if (!qtyDealsCase)
                return false;

            var recoveryFactor = CalcRecoeveryFactor();
            if (recoveryFactor < 1.0)
                return false;

            var qtyDealForExclude = (int)Math.Round(0.05 * deals.Count, MidpointRounding.AwayFromZero);

            var dealsForExclude = deals.OrderByDescending(d => d.GetProfit()).Take(qtyDealForExclude).ToList();

            //Нахожусь здесь. Исключаю сделки с наибольшей прибылью
            foreach (var deal in dealsForExclude)
            {
                var o = deal.BarNumberOpenPosition;
                var c = deal.BarNumberClosePosition;

            }

            throw new NotImplementedException();
        }

        private double CalcValue()
        {
            throw new NotImplementedException();
        }

        private double CalcRecoeveryFactor()
        {
            var profit = security.GetProfit();
            var account = system.Account;
            var drawDown = account.GetMaxDrawDown();
            
            var recoveryFactor = double.PositiveInfinity;
            if (drawDown != 0)
                recoveryFactor = profit / drawDown;

            return recoveryFactor;
        }
    }
}