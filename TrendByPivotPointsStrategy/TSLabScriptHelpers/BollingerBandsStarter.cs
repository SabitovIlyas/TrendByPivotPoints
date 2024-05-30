using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class BollingerBandsStarter : Starter
    {
        public override void Initialize(ISecurity[] securities, IContext ctx)
        {
            Logger.Log("Инициализация.");
            var securityFirst = securities.First();
            if (IsLaboratory(securityFirst))
                account = new AccountTsLab(securityFirst);
            else
                account = new AccountTsLabRt(securityFirst);

            var securityList = new List<Security>();

            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);

            var globalMoneyManager = new RiskManagerReal(account);
            globalMoneyManager.Logger = Logger;

            Currency currency;
            if (isUSD == 0)
                currency = Currency.Ruble;
            else
                currency = Currency.USD;

            account.Rate = rateUSD;
            var localMoneyManagerRuble = new ContractsManager(globalMoneyManager, account, currency, shares);

            tradingSystems = new List<TradingSystem>();

            double totalComission;
            AbsolutCommission absoluteComission;
            TradingSystem ts;

            ts = new TradingSystemBollingerBands(this.securityFirst, (PositionSide)positionSide);
            localMoneyManagerRuble.Logger = Logger;
            ts.Logger = Logger;
            tradingSystems.Add(ts);
            ts.Initialize(ctx);
            ts.SetParameters();

            totalComission = comission * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securities[0]);
            securities[0].Commission = CalculateCommission;

            account.Logger = logger;
            context = ContextTSLab.Create(ctx);
            account.Initialize(securityList);
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

        private double CalculateCommission(IPosition pos, double price, double shares, bool isEntry, bool isPart)
        {
            var exchangeCommission = comission;
            var brokerCommission = exchangeCommission;
            var totalCommission = exchangeCommission + brokerCommission;
            //var reserve = 0.25 * totalCommission;

            //return 0;
            return totalCommission * shares;
            //return totalCommission * shares * 0.75;
        }

        public override void Paint()
        {
            logger.Log("Отрисовка");
            var firstTradingSystem = tradingSystems.First();
            firstTradingSystem.Paint(context);
        }

        public override void Run()
        {
            logger.Log("Запуск!");
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

        public override void SetParameters(SystemParameters systemParameters)
        {
            logger.Log("Установка параметров.");
            this.systemParameters = systemParameters;
                      
            rateUSD = systemParameters.GetDouble("rateUSD");
            positionSide = systemParameters.GetInt("positionSide");
            comission = systemParameters.GetDouble("comission");            
            shares = systemParameters.GetInt("shares");
            isUSD = systemParameters.GetInt("isUSD");
        }
    }
}
