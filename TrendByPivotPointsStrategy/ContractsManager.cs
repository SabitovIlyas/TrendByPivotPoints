using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystems
{
    public class ContractsManager
    {
        Currency currency;
        Account account;
        RiskManager riskManager;
        int shares = 1;

        private Logger logger;
        private CurrencyConverter currencyConverter;

        public ContractsManager(RiskManager riskManager, Account account, 
            Currency currency, CurrencyConverter currencyConverter, Logger logger)
        {
            Initialize(riskManager, account, currencyConverter, logger);
            this.currency = currency;
        }

        public ContractsManager(RiskManager riskManager, Account account, 
            List<Security> securities, CurrencyConverter currencyConverter, Logger logger)
        {
            Initialize(riskManager, account, currencyConverter, logger);
            var security = securities.First();
            currency = security.Currency;            
        }

        public ContractsManager(RiskManager riskManager, Account account, 
            Currency currency, CurrencyConverter currencyConverter, int shares, 
            Logger logger)
        {
            Initialize(riskManager, account, currencyConverter, logger);            
            this.currency = currency;
            this.shares = shares;
        }

        private void Initialize(RiskManager riskManager, Account account, 
            CurrencyConverter currencyConverter, Logger logger)
        {
            this.riskManager = riskManager;
            this.account = account;
            this.currencyConverter = currencyConverter;
            this.logger = logger;
        }

        public virtual int GetQntContracts(Security security, double entryPrice, double stopPrice, PositionSide positionSide)
        {            
            logger.Log("Получаем количество контрактов для открываемой позиции...");
            var message = string.Format("Направление открываемой позиции: {0}; предполагаемая цена входа: {1}; расчётная цена выхода: {2}.",
                positionSide, entryPrice, stopPrice);
            logger.Log(message);

            var go = 0.0;
            switch (positionSide)
            {
                case PositionSide.Long:
                    {
                        if (stopPrice >= entryPrice)
                        {
                            logger.Log("Расчётная цена выхода больше либо равна цене входа. Сделку открывать не будем. Количество контрактов равно 0.");
                            return 0;
                        }                        
                        go = security.GObuying;                        
                    }
                    break;
                case PositionSide.Short:
                    {
                        if (stopPrice <= entryPrice)
                        {
                            logger.Log("Расчётная цена выхода меньше либо равна цене входа. Сделку открывать не будем. Количество контрактов равно 0.");
                            return 0; 
                        }
                        go = security.GOselling;
                    }
                    break;                
            }

                     
            var riskMoney = Math.Abs(entryPrice - stopPrice);
            logger.Log("Рискуем в одном контракте следующей суммой: " + riskMoney);
                        
            var money = riskManager.GetMoneyForDeal();
            var rate = currencyConverter.GetCurrencyRate(currency);            

            var loggedMoney = money;
            money = money / rate;
            message = string.Format("В инструменте используется валюта {0}. Полученную на сделку сумму в размере {1} нужно скорректировать " +
                    "с учётом курса: {2}. Полученная сумма на сделку в валюте равна {3}.", currency, loggedMoney, rate, money);
            logger.Log(message);

            var contractsByRiskMoney = (int)(money / riskMoney);            
            contractsByRiskMoney = contractsByRiskMoney / shares;
            if (contractsByRiskMoney == 0) contractsByRiskMoney = 1;

            message = string.Format("Вариант №1. Количество контрактов открываемой позиции, исходя из рискуемой суммой Equity (Estimated Balance) и рискуемой суммой в одном контракте, равно {0} " +
                "(с учётом цены контракта и количества лотов при открытии позиции {1}).", contractsByRiskMoney, shares);
            logger.Log(message);
            

            logger.Log("Гарантийное обеспечение равно " + go);
            var contractsByGO = (int)(riskManager.FreeBalance / go);
            logger.Log("Вариант №2. Количество контрактов открываемой позиции, исходя из ГО и свободных средств (Free Balance) равно " + contractsByGO);

            var min = Math.Min(contractsByRiskMoney, contractsByGO);
            logger.Log("Выбираем минимальное количество контрактов из двух вариантов. Оно равно " + min);
                        
            if (min >= 0) return min;

            logger.Log("Минимальное количество контрактов меньше 0, поэтому сделку открывать не будем. Устанавливаем количество контрактов равным 0.");
            return 0;
        }
    }
}