using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public class StarterDonchianTradingSystemTsLab : Starter
    {
        private double kAtr;
        private double limitOpenedPositions;
        IContext ctx;        

        public StarterDonchianTradingSystemTsLab(IContext ctx, ISecurity[] securities,
            Logger logger)
        {
            this.ctx = ctx;
            this.securities = new List<Security>();
            foreach (ISecurity security in securities)            
                this.securities.Add(new SecurityTSLab(security));
                        
            this.logger = logger;                   
        }

        public override void SetParameters(SystemParameters systemParameters)
        {            
            base.SetParameters(systemParameters);
        }

        public override void Initialize()
        {
            var securityTSLabFirst = securities.First() as SecurityTSLab;
            var securityFirst = securityTSLabFirst.security;
            context = new ContextTSLab(ctx, securityFirst);            
            var baseCurrency = Currency.RUB;
            if (context.IsRealTimeTrading)
                account = new AccountTsLabRt(securities, baseCurrency, logger);
            else
                account = new AccountTsLab(securities, baseCurrency, logger);

            var securityList = new List<Security>();
            this.securityFirst = new SecurityTSLab(securityFirst);
            securityList.Add(this.securityFirst);
            
            var riskValuePrcntCalc = kAtr * limitOpenedPositions;
            if (riskValuePrcntCalc > riskValuePrcnt)
                throw new System.Exception("Превышен уровень риска");
                        
            var currencyConverter = new CurrencyConverter(baseCurrency);
            currencyConverter.AddCurrencyRate(Currency.USD, rateUSD);

            ContractsManager contractsManager;
            if (contracts <= 0)
            {
                riskValuePrcnt = kAtr;
                var riskManager = new RiskManagerReal(account, logger, riskValuePrcnt);
                contractsManager = new ContractsManager(riskManager, account, currency,
                currencyConverter, shares, logger);
            }
            else
            {
                contractsManager = new ContractsManager(contracts, account, currency,
                currencyConverter, shares, logger);
            }         
            var indicators = new IndicatorsTsLab();

            tradingSystems = new List<TradingSystem>();
            var tradingSystem = new TradingSystemDonchian(securities, contractsManager,
                indicators, context, logger);

            tradingSystem.SetParameters(systemParameters);
            tradingSystem.Initialize();            
            tradingSystems.Add(tradingSystem);

            double totalComission;
            AbsolutCommission absoluteComission;
            totalComission = comission * 2;
            absoluteComission = new AbsolutCommission() { Commission = totalComission };
            absoluteComission.Execute(securityFirst);
            securityFirst.Commission = CalculateCommission;
        }        

        private double CalculateCommission(IPosition pos, double price, double shares, bool isEntry, bool isPart)
        {
            var exchangeCommission = price * comission;
            var brokerCommission = exchangeCommission;
            var totalCommission = exchangeCommission + brokerCommission;
            var reserve = 0.25 * totalCommission;

            return totalCommission + reserve;
        }      

        public override void Paint()
        {
            var firstTradingSystem = tradingSystems.First() as TradingSystemDonchian;
            firstTradingSystem.Paint(context);
        }              
    }
}