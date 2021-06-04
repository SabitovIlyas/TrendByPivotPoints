using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPointsStrategy
{
    public class LocalMoneyManager
    {
        Currency currency;
        Account account;
        GlobalMoneyManager globalMoneyManager;
        private Logger logger = new NullLogger();

        public LocalMoneyManager(GlobalMoneyManager globalMoneyManager, Account account, Currency currency)
        {
            this.globalMoneyManager = globalMoneyManager;
            this.account = account;
            this.currency = currency;
        }

        public virtual int GetQntContracts(double enterPrice, double stopPrice, Position position)
        {
            logger.Log("enterPrice = " + enterPrice.ToString());
            logger.Log("stopPrice = " + stopPrice.ToString());
            logger.Log("position = " + position.ToString());

            var go = 0.0;
            switch (position)
            {
                case Position.Long:
                    {
                        if (stopPrice >= enterPrice)
                            return 0;

                        go = account.GObying;                        
                        logger.Log("go = " + go.ToString());
                    }
                    break;
                case Position.Short:
                    {
                        if (stopPrice <= enterPrice)
                            return 0;
                        go = account.GOselling;
                    }
                    break;
                case Position.LongAndShort:                    
                    break;
            }            

            var money = globalMoneyManager.GetMoneyForDeal();
            logger.Log("money = " + money.ToString());
            var riskMoney = Math.Abs(enterPrice - stopPrice);            
            logger.Log("riskMoney = " + riskMoney.ToString());

            var contractsByGO = (int)(globalMoneyManager.FreeBalance / go); // надо вычислять это значение исходя из общего депозита            
            logger.Log("contractsByGO = " + contractsByGO.ToString());
            if (currency == Currency.USD)
                money = money / account.Rate;
                        
            logger.Log("money = " + money.ToString());
            
            var contractsByRiskMoney = (int)(money / riskMoney);            
            logger.Log("contractsByRiskMoney = " + contractsByRiskMoney.ToString());

            return Math.Min(contractsByRiskMoney, contractsByGO);            
        }
    }
}