using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using TSLab.Utils;
using TSLab.Script.Handlers;

namespace TrendByPivotPointsStrategy.Tests
{

    [TestClass()]
    public class SecurityRealTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            IReadOnlyList<IDataBar> barsBase = new ReadOnlyList<IDataBar>();
            var barsBaseAccessAdding = (ReadOnlyList<IDataBar>)barsBase;
            
            IDataBar bar;
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 5, 0));
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 10, 0));
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 15, 0));
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 20, 0));
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 25, 0));
            barsBaseAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 30, 0));
            barsBaseAccessAdding.Add(bar);

            ISecurity securityBase = new SecurityISecurityFake();
            securityBase.Bars

            IReadOnlyList<IDataBar> barsCompressed = new ReadOnlyList<IDataBar>();
            var barsCompressedAccessAdding = (ReadOnlyList<IDataBar>)barsCompressed;
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 5, 0));
            barsCompressedAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 15, 0));
            barsCompressedAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 30, 0));
            barsCompressedAccessAdding.Add(bar);




            //foreach (var bar in bars)
            //    double c = bar.Close;

            //ISecurity sec = new 
            //Security security = new SecurityReal()
        }

        [TestMethod()]
        public void GetMoneyTest_deposit1000_riskValuePrcnt5_freeBalance100_returned50()
        {
            IReadOnlyList<IDataBar> bars = new ReadOnlyList<IDataBar>();
            ((ReadOnlyList<IDataBar>)bars).Add(new NullDataBar());

            List<int> list = new List<int>();
            foreach (var l in list)
                Console.WriteLine(l);

            foreach (var bar in bars)
                Console.WriteLine(bar.High);
                



            //arrange            
            //var expected = 50;
            //account.FreeBalance = 100;

            ////act
            //var actual = globalMoneyManager.GetMoneyForDeal();
            ////assert
            //Assert.AreEqual(expected, actual);
        }
    }
}