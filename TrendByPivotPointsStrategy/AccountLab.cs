using System;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class AccountLab : Account
    {
        private ISecurity sec;
        private double equity;
        private IPosition lastClosedPosition;
        Logger logger = new NullLogger();

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

        public double InitDeposit
        {
            get
            {
                if (sec == null)
                    return 0;
                return sec.InitDeposit;
            }
        }

        public double Equity
        {
            get
            {
                if (sec == null)
                    return 0;
                return equity;
            }
        }        

        public double GObying => 4500;

        public double GOselling => 4500;

        public double Rate { get => 1; set => throw new NotImplementedException(); }

        public ISecurity Security
        {
            get
            {
                return sec;
            }
            set
            {
                sec = value;
            }
        }

        public AccountLab(ISecurity sec)
        {
            this.sec = sec;
            equity = sec.InitDeposit;            
        }

        public void Update(int barNumber)
        {
            var positions = sec.Positions;
            var lastPosition = positions.GetLastPosition(barNumber);

            if (lastClosedPosition != lastPosition)
            {
                var message = string.Format("AccountLab.Update: barNumber = {0}; lastClosedPosition != lastPosition; lastClosedPosition = {1}, lastPosition = {2}, equity = {3}",
                    barNumber, lastClosedPosition, lastPosition, equity);
                logger.Log(message);
                if (!lastPosition.IsActiveForBar(barNumber))
                {
                    lastClosedPosition = lastPosition;                    
                    logger.Log("Активная позиция закрылась");
                    equity = equity + lastClosedPosition.Profit();

                    message = string.Format("AccountLab.Update: barNumber = {0}; !lastPosition.IsActiveForBar(barNumber); lastClosedPosition = {1}, lastPosition = {2}, equity = {3}",
                    barNumber, lastClosedPosition, lastPosition, equity);
                    logger.Log(message);
                }
            }                     
        }
    }
}