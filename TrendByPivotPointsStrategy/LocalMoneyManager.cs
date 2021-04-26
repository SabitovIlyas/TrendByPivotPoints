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

        public LocalMoneyManager(GlobalMoneyManager globalMoneyManager, Account account, Currency currency)
        {
            this.globalMoneyManager = globalMoneyManager;
            this.account = account;
            this.currency = currency;
        }

        public virtual int GetQntContracts(double enterPrice, double stopPrice, Position position)
        {
            var go = 0.0;
            switch (position)
            {
                case Position.Long:
                    {
                        if (stopPrice >= enterPrice)
                            return 0;
                        go = account.GObying;

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

            var money = globalMoneyManager.GetMoney();
            var riskMoney = Math.Abs(enterPrice - stopPrice);
            var contractsByGO = (int)(money / go);

            if (currency == Currency.USD)
                money = money / account.Rate;
            var contractsByRiskMoney = (int)(money / riskMoney);            

            return Math.Min(contractsByRiskMoney, contractsByGO);
        }
    }
}
