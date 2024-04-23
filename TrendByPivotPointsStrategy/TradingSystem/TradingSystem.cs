using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public abstract class TradingSystem
    {
        IContext Ctx { get; set; }
        protected List<Security> securities { get; set; }

        protected Logger logger;
        protected PositionSide positionSide;
        protected SystemParameters systemParameters;
        protected int lastBarNumber;
        protected Security security;
        protected ContractsManager contractsManager;
        protected Indicators indicators;

        public TradingSystem(List<Security> securities, SystemParameters systemParameters, ContractsManager contractsManager, Indicators indicators, Logger logger)
        {
            if (securities.Count == 0)            
                throw new ArgumentOutOfRangeException(nameof(securities));

            this.securities = securities;
            security = securities.First();
            lastBarNumber = security.GetBarsCountReal() - 1;
            this.systemParameters = systemParameters;
            this.contractsManager = contractsManager;
            this.indicators = indicators;
            this.logger = logger;
        }

        public abstract void CalculateIndicators();
        public abstract void CheckPositionCloseCase(int barNumber);
        public abstract void CheckPositionOpenLongCase(double lastPrice, int barNumber);
        public abstract void CheckPositionOpenShortCase(double lastPrice, int barNumber);
        public abstract bool CheckShortPositionCloseCase(IPosition se, int barNumber);
        public abstract bool HasOpenPosition();
        public abstract void Paint(Context context);        
        public abstract void SetParameters(SystemParameters systemParameters);
        public abstract void Update(int barNumber);
        public abstract void Initialize(IContext ctx);       
    }
}