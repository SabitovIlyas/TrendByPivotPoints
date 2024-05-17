using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TradingSystems
{
    public abstract class TradingSystem
    {
        protected List<Security> securities { get; set; }

        protected Logger logger;
        protected PositionSide positionSide;
        protected SystemParameters systemParameters;
        protected int lastBarNumber;
        protected Security security;
        protected ContractsManager contractsManager;
        protected Indicators indicators;
        protected int barNumber;
        protected string tradingSystemDescription;
        protected int limitOpenedPositions = 1;
        protected Converter converter;

        public TradingSystem(List<Security> securities, ContractsManager contractsManager, Indicators indicators, Logger logger)
        {
            if (securities.Count == 0)            
                throw new ArgumentOutOfRangeException(nameof(securities));

            this.securities = securities;
            security = securities.First();
            lastBarNumber = security.GetBarsCountReal() - 1;            
            this.contractsManager = contractsManager;
            this.indicators = indicators;
            this.logger = logger;            
        }

        public abstract void CalculateIndicators();
        public abstract void CheckPositionCloseCase(int barNumber);
        protected abstract void CheckPositionOpenShortCase(int positionNumber);
        public abstract bool CheckShortPositionCloseCase(IPosition se, int barNumber);
        public abstract void Paint(Context context);               
        public virtual void Update(int barNumber)
        {
            try
            {
                this.barNumber = barNumber;

                if (security.IsRealTimeActualBar(barNumber))
                    logger.SwitchOn();
                else
                    logger.SwitchOff();

                CheckPositionOpenLongCase();
            }

            catch (Exception e)
            {
                Log("Исключение в методе Update(): " + e.ToString());
            }
        }

        public void CheckPositionOpenLongCase()
        {
            for (var i = 0; i < limitOpenedPositions; i++)
                CheckPositionOpenLongCase(i);
        }

        protected abstract void CheckPositionOpenLongCase(int positionNumber);        

        protected void Log(string text)
        {
            logger.Log("{0}: {1}", tradingSystemDescription, text);
        }

        protected void Log(string text, params object[] args)
        {
            text = string.Format(text, args);
            Log(text);
        }
        public abstract void Initialize(IContext ctx);

        protected bool IsPositionOpen(string notes = "")
        {
            var position = security.GetLastActiveForSignal(signalNameForOpenPosition + notes, barNumber);
            return position != null;
        }
    }
}