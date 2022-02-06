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
        public Logger Logger { get { return logger; } set { logger = value; } }
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
            var message = string.Format("Направление открываемой позиции: {0}; предполагаемая цена входа: {1}; расчётная цена выхода: {2}.",
                position, enterPrice, stopPrice);
            logger.Log(message);

            var go = 0.0;
            switch (position)
            {
                case PositionSide.Long:
                    {
                        if (stopPrice >= enterPrice)
                        {
                            logger.Log("Расчётная цена выхода больше либо равна цене входа. Сделку открывать не будем. Количество контрактов равно 0.");
                            return 0;
                        }
                        go = account.GObying;
                        
                    }
                    break;
                case PositionSide.Short:
                    {
                        if (stopPrice <= enterPrice)
                        {
                            logger.Log("Расчётная цена выхода меньше либо равна цене входа. Сделку открывать не будем. Количество контрактов равно 0.");
                            return 0; 
                        }
                        go = account.GOselling;
                    }
                    break;                
            }

                     
            var riskMoney = Math.Abs(enterPrice - stopPrice);
            logger.Log("Рискуем в одном контракте следующей суммой: " + riskMoney);
                        
            var money = globalMoneyManager.GetMoneyForDeal();

            if (currency == Currency.USD)
            {
                var moneyRuble = money;
                money = money / account.Rate;
                message = string.Format("Так как в инструменте используются USD, вместо рублей, то полученную на сделку сумму в размере {0} нужно скорректировать " +
                    "с учётом курса рубля к USD ({1}). Полученная сумма на сделку в USD равна {2}", moneyRuble, account.Rate, money);
                logger.Log(message);
            }

            var contractsByRiskMoney = (int)(money / riskMoney);            
            contractsByRiskMoney = contractsByRiskMoney / shares;
            if (contractsByRiskMoney == 0) contractsByRiskMoney = 1;

            message = string.Format("Вариант №1. Количество контрактов открываемой позиции, исходя из рискуемой суммой Equity (Estimated Balance) и рискумой суммой в одном контракте, равно {0}" +
                "(с учётом цены контракта и количества лотов при открытии позиции {1}.", contractsByRiskMoney, shares);
            logger.Log(message);
            

            logger.Log("Гарантийное обеспечение равно " + go);
            var contractsByGO = (int)(globalMoneyManager.FreeBalance / go);
            logger.Log("Вариант №2. Количество контрактов открываемой позиции, исходя из ГО и свободных средств (Free Balance) равно " + contractsByGO);

            var min = Math.Min(contractsByRiskMoney, contractsByGO);
            logger.Log("Выбираем минимальное количество контрактов из двух вариантов. Оно равно " + min);
                        
            if (min >= 0) return min;

            logger.Log("Минимальное количество контрактов меньше 0, поэтому сделку открывать не будем. Устанавливаем количество контрактов равным 0.");
            return 0;
        }
    }
}