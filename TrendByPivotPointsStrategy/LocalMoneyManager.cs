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
        int shares = 1;
        private Logger logger = new NullLogger();

        public LocalMoneyManager(GlobalMoneyManager globalMoneyManager, Account account, Currency currency)
        {
            this.globalMoneyManager = globalMoneyManager;
            this.account = account;
            this.currency = currency;
        }

        public LocalMoneyManager(GlobalMoneyManager globalMoneyManager, Account account, Currency currency, int shares)
        {
            this.globalMoneyManager = globalMoneyManager;
            this.account = account;
            this.currency = currency;
            this.shares = shares;
        }

        public virtual int GetQntContracts(double enterPrice, double stopPrice, PositionSide position)
        {            
            logger.Log("Получаем количество контрактов для открываемой позиции...");

            var go = 0.0;
            switch (position)
            {
                case PositionSide.Long:
                    {
                        if (stopPrice >= enterPrice)
                            return 0;

                        go = account.GObying;                        
                        logger.Log("go = " + go.ToString());
                    }
                    break;
                case PositionSide.Short:
                    {
                        if (stopPrice <= enterPrice)
                            return 0;
                        go = account.GOselling;
                    }
                    break;
                case PositionSide.LongAndShort:                    
                    break;
            }                       
            
            var message = string.Format("Направление открываемой позиции: {0}; предполагаемая цена входа: {1}; предполагаемая цена выхода: {2}.", 
                position, enterPrice, stopPrice);
            logger.Log(message);            
            var riskMoney = Math.Abs(enterPrice - stopPrice);
            logger.Log("Рискуем в одном контракте следующей суммой: " + riskMoney);
                        
            var money = globalMoneyManager.GetMoneyForDeal();
            var contractsByRiskMoney = (int)(money / riskMoney);
            contractsByRiskMoney = contractsByRiskMoney / shares;
            logger.Log("Количество контрактов открываемой позиции, исходя из рискуемой суммой Equity и рискумой суммой в одном контракте, равно "
                + contractsByRiskMoney);


            var contractsByGO = (int)(globalMoneyManager.Equity / go); // надо вычислять это значение исходя из общего депозита            
            logger.Log("contractsByGO = " + contractsByGO.ToString());
            if (currency == Currency.USD)
                money = money / account.Rate;
                        
            logger.Log("money = " + money.ToString());
            
            

            var min =  Math.Min(contractsByRiskMoney, contractsByGO);
            if (min >= 0) return min;
            else return 0;
        }
    }
}