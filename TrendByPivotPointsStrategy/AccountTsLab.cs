using System.Collections.Generic;
using System.Linq;
using TSLab.Script;

namespace TradingSystems
{
    public class AccountTsLab : Account
    {
        public override double InitDeposit
        {
            get
            {
                if (Security == null)
                    return 0;
                return Security.InitDeposit;
            }
        }

        public override double Equity
        {
            get
            {
                if (Security == null)
                    return 0;
                return equity;
            }
        }        

        public ISecurity Security { get; set; }
        public override double FreeBalance => Equity;

        public AccountTsLab(List<Security> securities, Currency currency, 
            Logger logger)
        {
            this.securities = securities;
            var securityFirst = securities.First();
            var security = securityFirst as SecurityTSLab;
            Security = security.security;
            equity = Security.InitDeposit;           
            this.currency = currency;
            this.logger = logger;            
        }

        public override double GetMaxDrawDown()
        {
            throw new System.NotImplementedException();
        }
    }
}