﻿using System.Collections.Generic;
using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    internal class SecurityLab : Security
    {
        private ISecurity sec;

        public SecurityLab(ISecurity sec)
        {
            this.sec = sec;
        }

        public int BarNumber { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public double? SellDeposit => 4500;

        public double? StepPrice => 1;

        public double? BuyDeposit => 4400;

        public Bar LastBar => throw new System.NotImplementedException();

        public double GetBarClose(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public double GetBarHigh(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public double GetBarLow(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public double GetBarOpen(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public List<Bar> GetBars(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public int GetSecurityCount()
        {
            throw new System.NotImplementedException();
        }
    }
}