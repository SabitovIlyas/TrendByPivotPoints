using System;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy
{
    public class AccountLab : Account
    {
        private ISecurity sec;
        Logger logger = new NullLogger();

        public double Deposit
        {
            get
            {
                if (sec == null)
                    return 0;
                return sec.InitDeposit;
            }
        }

        public double FreeBalance
        {
            get
            {
                if (sec == null)
                    return 0;
                return sec.InitDeposit;
            }
        }

        public double Depo
        {
            get
            {
                //var l = sec.Positions.GetLastPosition(5);
                //l.Profit();
                return 0;
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

        public AccountLab(ISecurity sec, IContext context)
        {
            this.sec = sec;
            equity = sec.InitDeposit;
            logger = new LoggerSystem(context);
        }

        public void Update(int barNumber)
        {
            var positions = sec.Positions;
            var lastPosition = positions.GetLastPosition(barNumber);

            if (lastClosedPosition != lastPosition)
            {
                var message = string.Format("AccountLab.Update: barNumber = {0}; lastClosedPosition != lastPosition; lastClosedPosition = {1}, lastPosition = {2}",
                    barNumber, lastClosedPosition, lastPosition);
                logger.Log(message);
                if (!lastPosition.IsActiveForBar(barNumber))
                {
                    lastClosedPosition = lastPosition;                    
                    logger.Log("Активная позиция закрылась");

                    equity = equity + lastClosedPosition.Profit();
                }
            }                     
        }

        private double equity;
        private IPosition lastClosedPosition;
    }
}
