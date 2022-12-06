using System;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class MainSystemScalper1
    {
        private IContext ctx;
        private double kAtr;
        private double limitOpenedPositions;

        public override Logger Logger { get { return logger; } set { logger = value; } }

        public void Initialize(ISecurity[] securities, IContext ctx)
        {
            //var logger = Logger;
            //var securityFirst = securities.First();
            //if (IsLaboratory(securityFirst))
            //    account = new AccountLab(securityFirst);
            //else
            //    account = new AccountReal(securityFirst);

            //var securityList = new List<Security>();

            //this.securityFirst = new SecurityTSlab(securityFirst);
            //securityList.Add(this.securityFirst);

            //var globalMoneyManager = new GlobalMoneyManagerReal(account);
            //globalMoneyManager.Logger = logger;

            //Currency currency;
            //if (isUSD == 0)
            //    currency = Currency.Ruble;
            //else
            //    currency = Currency.USD;

            //account.Rate = rateUSD;
            //var localMoneyManagerRuble = new LocalMoneyManager(globalMoneyManager, account, currency, shares);

            //tradingSystems = new List<TradingStrategy>();

            //double totalComission;
            //AbsolutCommission absoluteComission;
            //TradingStrategy ts;

            //ts = new TradingSystemScalper(localMoneyManagerRuble, this.securityFirst, (PositionSide)((int)positionSide));//si-5min            
            //localMoneyManagerRuble.Logger = logger;
            //ts.Logger = logger;
            //tradingSystems.Add(ts);
            //ts.Initialize(ctx);
            //ts.SetParameters(systemParameters);

            //totalComission = comission * 2;
            //absoluteComission = new AbsolutCommission() { Commission = totalComission };
            //absoluteComission.Execute(securities[0]);
            //securities[0].Commission = CalculateCommission;

            //account.Logger = logger;
            //this.ctx = ctx;
            //context = ContextTSLab.Create(ctx);
            //account.Initialize(securityList);
            //logger.SwitchOff();
        }

        //private double CalculateCommission(IPosition pos, double price, double shares, bool isEntry, bool isPart)
        //{
        //    var exchangeCommission = price * comission;
        //    var brokerCommission = exchangeCommission;
        //    var totalCommission = exchangeCommission + brokerCommission;
        //    var reserve = 0.25 * totalCommission;

        //    return totalCommission + reserve;
        //}

        public void Paint()
        {
            //var firstTradingSystem = tradingSystems.First();
            //firstTradingSystem.Paint(context);
        }

        public void Run()
        {
            //foreach (var tradingSystem in tradingSystems)
            //    tradingSystem.CalculateIndicators();

            //var lastBarNumber = securityFirst.GetBarsCountReal() - 1;
            //if (lastBarNumber < 1)
            //    return;

            //for (var i = 0; i <= lastBarNumber; i++)
            //{
            //    foreach (var tradingSystem in tradingSystems)
            //    {
            //        tradingSystem.Update(i);
            //        account.Update(i);
            //    }
            //}
        }

        public void SetParameters(SystemParameters systemParameters)
        {
            //this.systemParameters = systemParameters;

            //rateUSD = systemParameters.GetDouble("rateUSD");
            //positionSide = systemParameters.GetInt("positionSide");
            //comission = systemParameters.GetDouble("comission");
            //shares = systemParameters.GetInt("shares");
            //isUSD = systemParameters.GetInt("isUSD");
        }
    }
}
