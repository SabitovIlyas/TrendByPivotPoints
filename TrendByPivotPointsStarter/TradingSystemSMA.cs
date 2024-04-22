﻿using System;
using System.Collections.Generic;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;

namespace TrendByPivotPointsStarter
{

    public class TradingSystemSMA : TradingSystem
    {
        public TradingSystemSMA(List<Security> securities, SystemParameters systemParameters, ContractsManager contractsManager, Logger logger):
            base(securities, systemParameters, contractsManager, logger)
        {
          
        }               

        public override void CalculateIndicators()
        {
            //Series.SMA
        }        

        public override void CheckPositionCloseCase(int barNumber)
        {
            throw new System.NotImplementedException();
        }        

        public override void CheckPositionOpenLongCase(double lastPrice, int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public override void CheckPositionOpenShortCase(double lastPrice, int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public override bool CheckShortPositionCloseCase(IPosition se, int barNumber)
        {
            throw new System.NotImplementedException();
        }

        public override bool HasOpenPosition()
        {
            throw new System.NotImplementedException();
        }        

        public override void Initialize(IContext ctx)
        {
            throw new System.NotImplementedException();
        }

        public override void Paint(Context context)
        {
            throw new System.NotImplementedException();
        }        

        public override void SetParameters(SystemParameters systemParameters)
        {
            throw new System.NotImplementedException();
        }

        public override void Update(int barNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}