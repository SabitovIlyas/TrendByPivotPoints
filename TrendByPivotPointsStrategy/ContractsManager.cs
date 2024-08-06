using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystems
{
    public class ContractsManager
    {
        Currency currency;
        Account account;
        RiskManager globalMoneyManager;
        int shares = 1;
        public Logger Logger { get { return logger; } set { logger = value; } }
        private Logger logger = new NullLogger();
        private CurrencyConverter currencyConverter;

        public ContractsManager(RiskManager globalMoneyManager, Account account, Currency currency, CurrencyConverter currencyConverter)
        {
            this.globalMoneyManager = globalMoneyManager;
            this.account = account;
            this.currency = currency;
            this.currencyConverter = currencyConverter;
        }

        public ContractsManager(RiskManager globalMoneyManager, Account account, List<Security> securities, CurrencyConverter currencyConverter)
        {
            this.globalMoneyManager = globalMoneyManager;
            this.account = account;
            var security = securities.First();
            this.currency = security.Currency;
            this.currencyConverter = currencyConverter;
        }

        public ContractsManager(RiskManager globalMoneyManager, Account account, Currency currency, CurrencyConverter currencyConverter, int shares)
        {
            this.globalMoneyManager = globalMoneyManager;
            this.account = account;
            this.currency = currency;
            this.shares = shares;
            this.currencyConverter = currencyConverter;
        }

        public virtual int GetQntContracts(double entryPrice, double stopPrice, PositionSide position)
        {            
            logger.Log("Получаем количество контрактов для открываемой позиции...");
            var message = string.Format("Направление открываемой позиции: {0}; предполагаемая цена входа: {1}; расчётная цена выхода: {2}.",
                position, entryPrice, stopPrice);
            logger.Log(message);

            var go = 0.0;
            switch (position)
            {
                case PositionSide.Long:
                    {
                        if (stopPrice >= entryPrice)
                        {
                            logger.Log("Расчётная цена выхода больше либо равна цене входа. Сделку открывать не будем. Количество контрактов равно 0.");
                            return 0;
                        }
                        go = account.GObying;
                        
                    }
                    break;
                case PositionSide.Short:
                    {
                        if (stopPrice <= entryPrice)
                        {
                            logger.Log("Расчётная цена выхода меньше либо равна цене входа. Сделку открывать не будем. Количество контрактов равно 0.");
                            return 0; 
                        }
                        go = account.GOselling;
                    }
                    break;                
            }

                     
            var riskMoney = Math.Abs(entryPrice - stopPrice);
            logger.Log("Рискуем в одном контракте следующей суммой: " + riskMoney);
                        
            var money = globalMoneyManager.GetMoneyForDeal();
            var rate = currencyConverter.GetCurrencyRate(currency);            

            var logedMoney = money;
            money = money / rate;
            message = string.Format("В инструменте используется валюта {0}. Полученную на сделку сумму в размере {1} нужно скорректировать " +
                    "с учётом курса: {2}. Полученная сумма на сделку в валюте равна {3}.", currency, logedMoney, rate, money);
            logger.Log(message);

            var contractsByRiskMoney = (int)(money / riskMoney);            
            contractsByRiskMoney = contractsByRiskMoney / shares;
            if (contractsByRiskMoney == 0) contractsByRiskMoney = 1;

            message = string.Format("Вариант №1. Количество контрактов открываемой позиции, исходя из рискуемой суммой Equity (Estimated Balance) и рискуемой суммой в одном контракте, равно {0} " +
                "(с учётом цены контракта и количества лотов при открытии позиции {1}).", contractsByRiskMoney, shares);
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