using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystems;
using TrendByPivotPointsStarter;
using Account = TradingSystems.Account;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class FitnessDonchianChannel
    {
        public double NeighborhoodPercent { get; set; } = 0.00;//0.01
        public int DealsCountCriteria { get; set; } = 0;
        public double PrcntDealForExclude { get; set; } = 0.05;//0.05
        public bool IsCriteriaPassedNeedToCheck { get; set; } = true;

        private ChromosomeDonchianChannel chromosome;
        private StarterDonchianTradingSystemLab starter;
        private TradingSystemParameters trSysParams;
        private int dealsCount;
        private Account account;

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
            chromosome.FitnessValue = CalcIsPassed(isCriteriaPassedNeedToCheck);
            chromosome.DealsCount = dealsCount;

            var profit = account.InitDeposit - account.Equity;
            chromosome.Profit = profit;
            var acc = account as AccountLab;
            chromosome.ProfitPrcnt = acc.GetProfitPrcnt();
            chromosome.MaxDrawDown = acc.GetMaxDrawDownPrcnt();
            chromosome.RecoveryFactor = acc.GetRecoveryFactor();
        }

        private double CalcIsPassed(bool isCriteriaPassedNeedToCheck)
        {
            var averageRecoveryFactor = double.NegativeInfinity;

            var neighborhoodPercent = NeighborhoodPercent;
            if (!isCriteriaPassedNeedToCheck)
                neighborhoodPercent = 0;

            var sD = (int)trSysParams.SystemParameters.GetValue("slowDonchian");
            var fD = (int)trSysParams.SystemParameters.GetValue("fastDonchian");
            var atr = (int)trSysParams.SystemParameters.GetValue("atrPeriod");

            var minSd = (int)Math.Round(sD - neighborhoodPercent * sD, MidpointRounding.AwayFromZero);
            var maxSd = (int)Math.Round(sD + neighborhoodPercent * sD, MidpointRounding.AwayFromZero);

            var minFd = (int)Math.Round(fD - neighborhoodPercent * fD, MidpointRounding.AwayFromZero);
            var maxFd = (int)Math.Round(fD + neighborhoodPercent * fD, MidpointRounding.AwayFromZero);

            var minAtr = (int)Math.Round(atr - neighborhoodPercent * atr, MidpointRounding.AwayFromZero);
            var maxAtr = (int)Math.Round(atr + neighborhoodPercent * atr, MidpointRounding.AwayFromZero);

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
                        {
                            s.Bars = chromosome.Ticker.Bars;
                            var sl = s as SecurityLab;
                            sl.Initialize();    //не удалять! Очень важно! Связано с highiest, lowest
                        }                        
                        var param = new TradingSystemParameters(trSysParams);

                        param.SystemParameters.SetValue("slowDonchian", i);
                        param.SystemParameters.SetValue("fastDonchian", j);
                        param.SystemParameters.SetValue("atrPeriod", k);
                        SystemRun(starter, param);


                        var metaDeals = s.GetMetaDeals();
                        var bars = s.Bars;
                        dealsCount = metaDeals.Count;
                        var recoveryFactor = CheckCriteriaPassed(starter, param, starter.Account);
                        if (double.IsNegativeInfinity(recoveryFactor))
                            return averageRecoveryFactor;

                        counter++;
                        sumRecoveryFactor += recoveryFactor;

                        var c = ((int)trSysParams.SystemParameters.GetValue("slowDonchian") == i)
                            && ((int)trSysParams.SystemParameters.GetValue("fastDonchian") == j)
                            && ((int)trSysParams.SystemParameters.GetValue("atrPeriod") == k);

                        if (c)
                        {
                            sec = starter.GetSecurity();
                            account = starter.Account;
                        }
                    }
                }
            }

            if (sec != null)
                dealsCount = sec.GetMetaDeals().Count;         
            
            averageRecoveryFactor = Math.Round(sumRecoveryFactor / counter, 2);
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

            if (!IsCriteriaPassedNeedToCheck)
            {
                recoveryFactor = CalcRecoveryFactor(system.GetSecurity(), system.Account);
                return recoveryFactor;
            }

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
            var nSec = newSystem.GetSecurity();
            var acc = newSystem.Account;
            recoveryFactor = CalcRecoveryFactor(nSec, acc);
            deals = nSec.GetMetaDeals();
            return recoveryFactor;
        }

        private void SystemRun(Starter system, TradingSystemParameters parameters)
        {
            system.SetParameters(parameters.SystemParameters);
            system.Initialize();
            system.Run();
        }

        private double CalcRecoveryFactor(Security security, Account account)
        {
            var acc = (AccountLab)account;
            return acc.GetRecoveryFactor();
        }
    }
}