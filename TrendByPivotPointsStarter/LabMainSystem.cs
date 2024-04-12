using System;
using System.Collections.Generic;
using TradingSystems;
using TSLab.Script;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStarter
{
    public class LabMainSystem : MainSystem
    {        
        public LabMainSystem(List<Security> securities, Logger logger, SystemParameters system)        
        {
            this.logger = logger;
            this.securities = securities;
            SetParameters(system);
            Initialize();
        }
        
        private void Initialize()
        {
            Initialize(securities);
        }

        public override void Initialize(List<Security> securities)
        {
            InitializeBase();            
        }

        public override void Paint()
        {
            throw new NotImplementedException();
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