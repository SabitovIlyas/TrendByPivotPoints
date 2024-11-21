﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TradingSystems.Tests
{
    public class TradeingSecurityLabTests
    {
        List<Bar> bars;
        SecurityLab security;

        [TestInitialize]
        public void TestInitialize()
        {
            bars = new List<Bar>()
            {
                Bar.Create(new DateTime(2025,11,27,10,00,00),90000,90000,90000,90000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),80000,80000,80000,80000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),81000,81000,81000,81000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),82000,82000,82000,82000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),83000,83000,83000,83000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),84000,84000,84000,84000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),85000,85000,85000,85000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),86000,86000,86000,86000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),87000,87000,87000,87000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),88000,88000,88000,88000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),89000,89000,89000,89000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),90000,90000,90000,90000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),91000,91000,91000,91000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),92000,92000,92000,92000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),93000,93000,93000,93000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),94000,94000,94000,94000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),95000,95000,95000,95000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),94000,94000,94000,94000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),93000,93000,93000,93000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),92000,92000,92000,92000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),91000,91000,91000,91000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),90000,90000,90000,90000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),89000,89000,89000,89000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),88000,88000,88000,88000,1, "SPFB.TEST", "1",0),
                Bar.Create(new DateTime(2025,11,27,10,00,00),87000,87000,87000,87000,1, "SPFB.TEST", "1",0)
            };

            security = new SecurityLab(Currency.Ruble, shares: 1, bars);
        }
    }
}