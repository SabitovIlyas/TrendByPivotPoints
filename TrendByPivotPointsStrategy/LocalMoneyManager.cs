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
            TrendByPivotPointsStrategy.ctx.Log("enterPrice = " + enterPrice.ToString());
            TrendByPivotPointsStrategy.ctx.Log("stopPrice = " + stopPrice.ToString());
            TrendByPivotPointsStrategy.ctx.Log("position = " + position.ToString());


            var go = 0.0;
            switch (position)
            {
                case Position.Long:
                    {
                        if (stopPrice >= enterPrice)
                            return 0;

                        go = account.GObying;
                        TrendByPivotPointsStrategy.ctx.Log("go = " + go.ToString());

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

            TrendByPivotPointsStrategy.ctx.Log("money = " + money.ToString());

            var riskMoney = Math.Abs(enterPrice - stopPrice);

            TrendByPivotPointsStrategy.ctx.Log("riskMoney = " + riskMoney.ToString());

            //var contractsByGO = (int)(money / go); // надо вычислять это значение исходя из общего депозита
            var contractsByGO = 1000000;

            TrendByPivotPointsStrategy.ctx.Log("contractsByGO = " + contractsByGO.ToString());

            if (currency == Currency.USD)
                money = money / account.Rate;

            TrendByPivotPointsStrategy.ctx.Log("money = " + money.ToString());

            var contractsByRiskMoney = (int)(money / riskMoney);

            TrendByPivotPointsStrategy.ctx.Log("contractsByRiskMoney = " + contractsByRiskMoney.ToString());

            return Math.Min(contractsByRiskMoney, contractsByGO);
        }
    }
}
