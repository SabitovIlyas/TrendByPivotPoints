using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TradingSystems;
using TrendByPivotPointsStarter;
using TSLab.DataSource;
using Account = TradingSystems.Account;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class FitnessDonchianChannel
    {
        public double NeighborhoodPercent { get; set; } = 0.01;
        public int DealsCountCriteria { get; set; } = 30;        
        public double PrcntDealForExclude { get; set; } = 0.05;
        public bool IsCriteriaPassedNeedToCheck { get; set; } = true;

        private ChromosomeDonchianChannel chromosome;
        private StarterDonchianTradingSystemLab starter;
        private TradingSystemParameters trSysParams;
        private int dealsCount;

        public FitnessDonchianChannel(TradingSystemParameters trSysParams, ChromosomeDonchianChannel chromosome, 
            StarterDonchianTradingSystemLab starter)
        {
            this.trSysParams = trSysParams;
            this.chromosome = chromosome;
            this.starter = starter;
            chromosome.FitnessDonchianChannel = this;
        }
        
        public void SetUpChromosomeFitnessValue(bool isCriteriaPassedNeedToCheck = true)
        {
            IsCriteriaPassedNeedToCheck = isCriteriaPassedNeedToCheck;
            chromosome.FitnessValue = CalcIsPassed();
            chromosome.DealsCount = dealsCount;
        }

        private double CalcIsPassed()
        {
            var averageRecoveryFactor = double.NegativeInfinity;

            var sD = (int)trSysParams.SystemParameters.GetValue("slowDonchian");
            var fD = (int)trSysParams.SystemParameters.GetValue("fastDonchian");
            var atr = (int)trSysParams.SystemParameters.GetValue("atrPeriod");

            var minSd = (int)Math.Round(sD - NeighborhoodPercent * sD, MidpointRounding.AwayFromZero);
            var maxSd = (int)Math.Round(sD + NeighborhoodPercent * sD, MidpointRounding.AwayFromZero);

            var minFd = (int)Math.Round(fD - NeighborhoodPercent * fD, MidpointRounding.AwayFromZero);
            var maxFd = (int)Math.Round(fD + NeighborhoodPercent * fD, MidpointRounding.AwayFromZero);

            var minAtr = (int)Math.Round(atr - NeighborhoodPercent * atr, MidpointRounding.AwayFromZero);
            var maxAtr = (int)Math.Round(atr + NeighborhoodPercent * atr, MidpointRounding.AwayFromZero);

            var counter = 0;
            var sumRecoveryFactor = 0d;
            Security sec = null;

            for (int i = minSd; i <= maxSd; i++)
            {
                for (int j = minFd; j <= maxFd; j++)
                {
                    if (j > i)
                        continue;

                    for (int k = minAtr; k <= maxAtr; k++)
                    {
                        var starter = this.starter.GetClone();
                        var s = starter.GetSecurity();
                        if (s != null)
                            starter.GetSecurity().Bars = chromosome.Ticker.Bars;
                        var param = new TradingSystemParameters(trSysParams);

                        param.SystemParameters.SetValue("slowDonchian", i);
                        param.SystemParameters.SetValue("fastDonchian", j);
                        param.SystemParameters.SetValue("atrPeriod", k);                        
                        SystemRun(starter, param);

                        dealsCount = starter.GetSecurity().GetMetaDeals().Count;
                        var recoveryFactor = CheckCriteriaPassed(starter, param, starter.Account);
                        if (double.IsNegativeInfinity(recoveryFactor))
                            return averageRecoveryFactor;

                        counter++;
                        sumRecoveryFactor += recoveryFactor;

                        var c = ((int)trSysParams.SystemParameters.GetValue("slowDonchian") == i)
                            && ((int)trSysParams.SystemParameters.GetValue("fastDonchian") == j)
                            && ((int)trSysParams.SystemParameters.GetValue("atrPeriod") == k);

                        if (c)
                            sec = starter.GetSecurity();
                    }
                }
            }

            if (sec != null)
                dealsCount = sec.GetMetaDeals().Count;

            averageRecoveryFactor = sumRecoveryFactor / counter;
            return averageRecoveryFactor;
        }

        private double CheckCriteriaPassed(StarterDonchianTradingSystemLab system, TradingSystemParameters parameters, Account account)
        {
            var recoveryFactor = double.NegativeInfinity;
            var security = system.GetSecurity();

            var deals = security.GetMetaDeals();
            //dealsCount = deals.Count;
            
            var isQtyDealsEnough = deals.Count >= DealsCountCriteria;
            if (IsCriteriaPassedNeedToCheck && !isQtyDealsEnough)
                return recoveryFactor;

            var qtyDealForExclude = (int)Math.Round(PrcntDealForExclude * deals.Count, MidpointRounding.AwayFromZero);
            var dealsForExclude = deals.OrderByDescending(d => d.GetProfit()).Take(qtyDealForExclude).ToList();

            var nonTradingPeriods = new List<NonTradingPeriod>();

            foreach (var deal in dealsForExclude)
            {
                var n = new NonTradingPeriod();
                n.BarStart = deal.BarNumberOpenPosition - 1;
                n.BarStop = deal.BarNumberClosePosition - 1;
                nonTradingPeriods.Add(n);
            }
            var newSystem = system.GetClone();
            newSystem.NonTradingPeriods = nonTradingPeriods;

            SystemRun(newSystem, parameters);
            recoveryFactor = CalcRecoeveryFactor(security, account);

            return recoveryFactor;
        }

        private void SystemRun(Starter system, TradingSystemParameters parameters)
        {
            system.SetParameters(parameters.SystemParameters);
            system.Initialize();
            system.Run();
        }

        private double CalcRecoeveryFactor(Security security, Account account)
        {
            var profit = security.GetProfit();            
            var drawDown = account.GetMaxDrawDownPrcnt();
            
            var recoveryFactor = double.PositiveInfinity;
            if (drawDown != 0)
                recoveryFactor = profit / drawDown;

            return recoveryFactor;
        }
    }
}