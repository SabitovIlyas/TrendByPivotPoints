using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public class AccountTsLabRt : Account
    {
        public ISecurity Security { get => sec;}
        public Currency Currency => currency;
        public double GObying => sec.FinInfo.BuyDeposit ?? double.MaxValue;

        public double GOselling => sec.FinInfo.SellDeposit ?? double.MaxValue;

        Logger logger = new LoggerNull();


        ISecurity sec;
        Currency currency;

        public override double InitDeposit => sec.InitDeposit;

        public override double Equity 
        {
            get
            {
                var rtSec = sec as ISecurityRt;
                if (rtSec != null)
                {
                    logger.Log("Получаем Equity: ISecurityRt.EstimatedBalance = {0}", rtSec.EstimatedBalance);
                    return rtSec.EstimatedBalance;
                }

                logger.Log("rtSec == null");
                return 0;
            } 
        }

        public Logger Logger
        {
            get
            {
                return logger;
            }

            set
            {
                logger = value;
            }
        }

        public override double FreeBalance
        {
            get
            {
                var rtSec = sec as ISecurityRt;
                if (rtSec != null)
                    return rtSec.CurrencyBalance;
                return 0;
            }
        }
        public AccountTsLabRt(List<Security> securities, Currency currency, Logger logger)
        {
            this.securities = securities;
            var securityFirst = securities.First();
            var security = securityFirst as SecurityTSLab;
            sec = security.security;
            this.currency = currency;
            this.logger = logger;
        }

        public void Initialize(List<Security> securities)
        {            
        }
    }
}
