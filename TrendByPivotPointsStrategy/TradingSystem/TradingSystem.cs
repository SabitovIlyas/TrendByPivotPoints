using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystems
{
    public abstract class TradingSystem
    {
        protected List<Security> securities { get; set; }

        protected Logger logger;
        protected PositionSide positionSide;
        protected int lastBarNumber;
        protected Security security;
        protected ContractsManager contractsManager;
        protected Indicators indicators;
        protected int barNumber;
        protected string tradingSystemDescription;
        protected string signalNameForOpenPosition;
        protected string signalNameForClosePosition;
        protected int limitOpenedPositions = 1;
        protected Converter converter;
        protected string parametersCombination = string.Empty;
        protected string name = string.Empty;
        protected double entryPricePlanned;
        protected double currentPrice;
        protected Context context;
        protected int nonTradingPeriod;        

        public TradingSystem(List<Security> securities, ContractsManager contractsManager, Indicators indicators, Context context, Logger logger)
        {
            if (securities.Count == 0)            
                throw new ArgumentOutOfRangeException(nameof(securities));

            this.securities = securities;
            security = securities.First();
            lastBarNumber = security.GetBarsCountReal() - 1;            
            this.contractsManager = contractsManager;
            this.indicators = indicators;
            this.context = context;
            this.logger = logger;            
        }

        public abstract void CalculateIndicators();
        public abstract void CheckPositionCloseCase(Position position, string signalNameForClosePosition, out bool isPositionClosing);                       
        public virtual void Update(int barNumber)
        {
            if (barNumber < nonTradingPeriod)
                return;

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

        protected bool IsPositionOpen(string notes = "")
        {
            var position = security.GetLastActiveForSignal(signalNameForOpenPosition + notes, barNumber);
            return position != null;
        }

        public virtual void Initialize()
        {            
            tradingSystemDescription = string.Format("{0}/{1}/{2}/{3}/", name, parametersCombination, security.Name, positionSide);

            switch (positionSide)
            {
                case PositionSide.Long:
                    {
                        signalNameForOpenPosition = "LE";
                        signalNameForClosePosition = "LXS";
                        converter = new Converter(isConverted: false);
                        break;
                    }
                case PositionSide.Short:
                    {
                        signalNameForOpenPosition = "SE";
                        signalNameForClosePosition = "SXS";
                        converter = new Converter(isConverted: true);
                        break;
                    }
            }
        }

        protected Position GetOpenedPosition(string notes)
        {            
            var position = security.GetLastActiveForSignal(signalNameForOpenPosition + notes, barNumber);
            return position;
        }

        protected virtual double GetStopPrice(string notes = "")
        {
            throw new NotImplementedException();
        }

        public virtual void Paint(Context context)
        {
            throw new NotImplementedException();

        }
    }
}