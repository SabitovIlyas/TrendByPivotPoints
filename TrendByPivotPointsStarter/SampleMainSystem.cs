﻿using System.Collections.Generic;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStarter
{
    public class SampleMainSystem : MainSystem
    {
        public override void Initialize(List<Security> securities)
        {
            InitializeBase();            
        }

        public override void Paint()
        {

        }

        public override void Run()
        {

        }

        public override void SetParameters(SystemParameters systemParameters)
        {
            SetBaseParameters(systemParameters);
            var sma = systemParameters.GetInt("SMA");
        }
    }
}