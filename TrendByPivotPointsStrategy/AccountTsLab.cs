using System.Collections.Generic;
using System.Linq;
using TSLab.Script;

namespace TradingSystems
{
    public class AccountTsLab : Account
    {

        public Logger Logger { get; set; } = new NullLogger();

        public double InitDeposit
        {
            get
            {
                if (Security == null)
                    return 0;
                return Security.InitDeposit;
            }
        }

        public double Equity
        {
            get
            {
                if (Security == null)
                    return 0;
                return equity;
            }
        }        

        public double GObying => 0.45;

        public double GOselling => 0.40;

        public ISecurity Security { get; set; }

        public double FreeBalance => Equity;

        public Currency Currency => currency;
        private Currency currency;

        public AccountTsLab(List<Security> securities, Currency currency, 
            Logger logger)
        {
            this.securities = securities;
            var securityFirst = securities.First();
            var security = securityFirst as SecurityTSLab;
            Security = security.security;
            equity = Security.InitDeposit;           
            this.currency = currency;
            Logger = logger;            
        }        
    }
}