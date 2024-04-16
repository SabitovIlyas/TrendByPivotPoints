﻿using System.Collections.Generic;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStarter
{
    internal class TradingSystemSMA : TradingSystem
    {
        public TradingSystemSMA(List<Security> securities, SystemParameters systemParameters, Logger logger)
        {
            this.securities = securities;
            this.systemParameters = systemParameters;
            this.logger = logger;
        }

        public void CalculateIndicators()
        {
            throw new System.NotImplementedException();
        }

        public void CheckPositionCloseCase(int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public void CheckPositionOpenLongCase(double lastPrice, int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public void CheckPositionOpenShortCase(double lastPrice, int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckShortPositionCloseCase(IPosition se, int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public bool HasOpenPosition()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(IContext ctx)
        {
            throw new System.NotImplementedException();
        }

        public void Paint(Context context)
        {
            throw new System.NotImplementedException();
        }

        public void SetParameters(double leftLocalSide, double rightLocalSide, double pivotPointBreakDownSide, double EmaPeriodSide)
        {
            throw new System.NotImplementedException();
        }

        public void SetParameters(SystemParameters systemParameters)
        {
            throw new System.NotImplementedException();
        }

        public void Update(int barNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}