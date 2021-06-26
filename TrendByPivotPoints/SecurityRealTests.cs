using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy.Tests
{
    [TestClass()]
    public class SecurityRealTests
    {
        Security security;

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
            var securityBaseAccessAdding = (SecurityISecurityFake)securityBase;
            securityBaseAccessAdding.Bars = barsBaseAccessAdding;

            IReadOnlyList<IDataBar> barsCompressed = new ReadOnlyList<IDataBar>();
            var barsCompressedAccessAdding = (ReadOnlyList<IDataBar>)barsCompressed;
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 5, 0));
            barsCompressedAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 15, 0));
            barsCompressedAccessAdding.Add(bar);
            bar = new DataBarFake(new DateTime(2021, 6, 18, 14, 30, 0));
            barsCompressedAccessAdding.Add(bar);

            ISecurity securityCompressed = new SecurityISecurityFake();
            var securityCompressedAccessAdding = (SecurityISecurityFake)securityCompressed;
            securityCompressedAccessAdding.Bars = barsCompressedAccessAdding;

            security = new SecurityReal(securityCompressed, securityBase);
        }

        [TestMethod()]
        public void GetBarsBaseFromBarCompressedTest_barNumber0_returned0_1()
        {
            //arrange            
            var expected = new List<int>() { 0, 1 };

            //act
            var actual = security.GetBarsBaseFromBarCompressed(barNumber: 0);

            //assert
            var countIsEqual = actual.Count == expected.Count;
            var counter = 0;
            for (var i = 0; i < expected.Count; i++)
            {
                if (expected[i] == actual[i])
                    counter++;
            }
            var elementsIsEqual = counter == expected.Count;
            var result = countIsEqual && elementsIsEqual;
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void GetBarsBaseFromBarCompressedTest_barNumber1_returned2_3_4()
        {
            //arrange            
            var expected = new List<int>() { 2, 3, 4 };

            //act
            var actual = security.GetBarsBaseFromBarCompressed(barNumber: 1);

            //assert
            var countIsEqual = actual.Count == expected.Count;
            var counter = 0;
            for (var i = 0; i < expected.Count; i++)
            {
                if (expected[i] == actual[i])
                    counter++;
            }
            var elementsIsEqual = counter == expected.Count;
            var result = countIsEqual && elementsIsEqual;
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void GetBarsBaseFromBarCompressedTest_barNumber2_returned5()
        {
            //arrange            
            var expected = new List<int>() { 5 };

            //act
            var actual = security.GetBarsBaseFromBarCompressed(barNumber: 2);

            //assert
            var countIsEqual = actual.Count == expected.Count;
            var counter = 0;
            for (var i = 0; i < expected.Count; i++)
            {
                if (expected[i] == actual[i])
                    counter++;
            }
            var elementsIsEqual = counter == expected.Count;
            var result = countIsEqual && elementsIsEqual;
            Assert.IsTrue(result);
        }
    }
}