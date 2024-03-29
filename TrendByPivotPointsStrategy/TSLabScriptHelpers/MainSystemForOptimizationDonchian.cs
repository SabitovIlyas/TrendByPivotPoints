﻿using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public class MainSystemForOptimizationDonchian : PivotPointsMainSystem
    {
        private Security securityFirst;
        private IContext ctx;
        private double kAtr;
        private double limitOpenedPositions;

        public override void Initialize(ISecurity[] securities, IContext ctx)
        {
            var logger = Logger;
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountLab(securityFirst);
            else
                account = new AccountReal(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSlab(securityFirst);
            securityList.Add(this.securityFirst);

            var riskValuePrcntCalc = kAtr * limitOpenedPositions;
            if (riskValuePrcntCalc > riskValuePrcnt)
                throw new System.Exception("Превышен уровень риска");

            riskValuePrcnt = kAtr;
            var globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: this.riskValuePrcnt);
            globalMoneyManager.Logger = logger;

            Currency currency;
            if (isUSD == 0)
                currency = Currency.Ruble;
            else
                currency = Currency.USD;

            account.Rate = rateUSD;
            var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, currency, shares);

            tradingSystems = new List<TradingStrategy>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingStrategy ts;

            ts = new TradingSystemDonchian(localMoneyManagerRuble, account, this.securityFirst, (PositionSide)((int)positionSide));//si-5min            
            localMoneyManagerRuble.Logger = logger;
            ts.Logger = logger;
            tradingSystems.Add(ts);
            ts.Initialize(ctx);
            ts.SetParameters(systemParameters);

            totalComission = comission * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            securities[0].Commission = CalculateCommission;

            account.Logger = logger;
            this.ctx = ctx;
            context = ContextTSLab.Create(ctx);
            account.Initialize(securityList);
            logger.SwitchOff();
        }

        private double CalculateCommission(IPosition pos, double price, double shares, bool isEntry, bool isPart)
        {
            var exchangeCommission = price * comission;
            var brokerCommission = exchangeCommission;
            var totalCommission = exchangeCommission + brokerCommission;
            var reserve = 0.25 * totalCommission;

            return totalCommission + reserve;
        }

        public override void Run()
        {
            foreach (var tradingSystem in tradingSystems)
                tradingSystem.CalculateIndicators();

            var lastBarNumber = securityFirst.GetBarsCountReal() - 1;
            if (lastBarNumber < 1)
                return;

            for (var i = 0; i <= lastBarNumber; i++)
            {
                foreach (var tradingSystem in tradingSystems)
                {
                    tradingSystem.Update(i);
                    account.Update(i);
                }
            }
        }

        public void Paint(IContext ctx, ISecurity sec)
        {
            var firstTradingSystem = tradingSystems.First();
            firstTradingSystem.Paint(context);
        }

        private bool IsLaboratory(ISecurity security)
        {
            var realTimeSecurity = security as ISecurityRt;
            return realTimeSecurity == null;
        }

        private bool IsLastBarClosed()
        {
            return ctx.IsLastBarClosed;
        }
        private bool IsRealTimeTrading()
        {
            return securityFirst.IsRealTimeTrading;
        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            kAtr = systemParameters.GetDouble("kAtr");
            limitOpenedPositions = systemParameters.GetDouble("limitOpenedPositions");
            base.SetParameters(systemParameters);
        }
    }
}