using System.Collections.Generic;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Realtime;

namespace TradingSystems
{
    public abstract class MainSystem
    {
        protected Context context;
        protected List<TradingStrategy> tradingSystems;
        protected int securityNumber;

        protected double rateUSD;
        protected int positionSide;
        protected double comission;
        protected double riskValuePrcnt;
        protected double scaleContractsPrcnt;
        protected int isUSD;

        protected int shares;
        protected Logger logger = new NullLogger();
        protected Account account;
        protected SystemParameters systemParameters;
        protected Security securityFirst;

        public  Logger Logger { get { return logger; } set { logger = value; } }

        public abstract void Initialize(ISecurity[] securities, IContext ctx);
        public abstract void Paint();
        public abstract void Run();
        public abstract void SetParameters(SystemParameters systemParameters);

        protected bool IsLaboratory(ISecurity security)
        {
            var realTimeSecurity = security as ISecurityRt;
            return realTimeSecurity == null;
        }
    }
}