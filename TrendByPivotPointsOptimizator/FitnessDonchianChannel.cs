using System;
using System.Collections.Generic;
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
        private StarterDonchianTradingSystemLab system;
        private TradingSystemParameters trSysParams;

        public FitnessDonchianChannel(Security security, ChromosomeDonchianChannel chromosome, 
            Optimizator optimizator, StarterDonchianTradingSystemLab system)
        {
            this.security = security;
            this.chromosome = chromosome;
            this.optimizator = optimizator;
            this.system = system;            
            Calc();
        }

        public FitnessDonchianChannel(TradingSystemParameters trSysParams, ChromosomeDonchianChannel chromosome, 
            StarterDonchianTradingSystemLab system)
        {
            security = trSysParams.Security;
            this.chromosome = chromosome;
            this.system = system;
            Calc();
        }

        private void Calc()
        {
            ////Параметры для оптимизации: быстрая доунчиан, медленная доунчиан, период АТР,
            ////лимит открытых позиций, коэф. АТР для открытия позиции, коэф. АТР для стоп-лосса
            //optimizator.GetOptimalParametersPercent(
            //    points:null,
            //    dimension: 6, // Количество параметров  
            //    radiusNeighbourInPercent:new int[3] { 5, 5, 5 },
            //    barrier: 1.0, // Пороговое значение
            //    isCheckedPass: true); // Проверка на прохождение барьера           

            SystemRun(); // Запускаем систему на тестирование

            chromosome.FitnessPassed = CalcIsPassed();
            chromosome.FitnessValue = CalcValue();
        }

        private void SystemRun()
        {
            system.SetParameters(trSysParams.SystemParameters);
            system.Initialize();
            system.Run();
        }

        private bool CalcIsPassed()
        {
            CalcIsPassedOneSystem();

            //Нахожусь здесь! Теперь надо повторить всё это для системы, с рядом стоящими значениями параметров

            var systems = new List<StarterDonchianTradingSystemLab>();
            var sD = (int)trSysParams.SystemParameters.GetValue("slowDonchian");
            var fD = (int)trSysParams.SystemParameters.GetValue("fastDonchian");
            var atr = (int)trSysParams.SystemParameters.GetValue("atrPeriod");

            var minSd = (int)Math.Round(sD - 0.05 * sD, MidpointRounding.AwayFromZero);
            var maxSd = (int)Math.Round(sD + 0.05 * sD, MidpointRounding.AwayFromZero);

            var minFd = (int)Math.Round(fD - 0.5 * fD, MidpointRounding.AwayFromZero);
            var maxFd = (int)Math.Round(fD + 0.5 * fD, MidpointRounding.AwayFromZero);

            var minAtr = (int)Math.Round(atr - 0.05 * atr, MidpointRounding.AwayFromZero);
            var maxAtr = (int)Math.Round(atr + 0.05 * atr, MidpointRounding.AwayFromZero);


            for (int i = minSd; i <= maxSd; i++)
            {
                for (int j = minFd; j <= maxFd; j++)
                {
                    if (j > i)
                        continue;

                    for (int k = minAtr; k <= maxAtr; k++)
                    {
                        var starter = new StarterDonchianTradingSystemLab(system);

                        var param = new TradingSystemParameters(trSysParams);
                        starter.SetParameters(trSysParams.SystemParameters);
                        //Я здесь. Надо изменить исходные параметры.
                        systems.Add(starter);
                    }
                }
            }

            var matrixCreator = MatrixCreator.Create(points: null, dimension: 0, radiusNeighbour: null);

            return false;
        }

        private bool CalcIsPassedOneSystem()
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

            var nonTradingPeriods = new List<NonTradingPeriod>();

            foreach (var deal in dealsForExclude)
            {
                var n = new NonTradingPeriod();
                n.BarStart = deal.BarNumberOpenPosition - 1;
                n.BarStop = deal.BarNumberClosePosition - 1;
                nonTradingPeriods.Add(n);
            }

            system.NonTradingPeriods = nonTradingPeriods;
            SystemRun();

            recoveryFactor = CalcRecoeveryFactor();
            if (recoveryFactor < 1.0)
                return false;

            return true;
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