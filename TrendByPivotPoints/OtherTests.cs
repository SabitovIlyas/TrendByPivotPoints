using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using TrendByPivotPoints;
using TSLab.Utils;

namespace TrendByPivotPointsStrategy.Tests
{
    [TestClass()]
    public class OtherTests
    {
        [TestMethod()]
        public void MethodWithKeyWordRefForStructTest()
        {
            var expected = 2;            

            var actual = 1;
            var classForTestKeyWords = new ClassForTestKeyWords();
            classForTestKeyWords.MethodWithKeyWordRefForStruct(ref actual);
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MethodWithoutKeyWordRefForStructTest()
        {
            var expected = 1;

            var actual = 1;
            var classForTestKeyWords = new ClassForTestKeyWords();
            classForTestKeyWords.MethodWithoutKeyWordRefForStruct(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MethodWithKeyWordRefForClassTest()
        {
            var expected = 2;            

            var param = new HelperForClassForTestKeyWords(property: 1);            
            var classForTestKeyWords = new ClassForTestKeyWords();            
            classForTestKeyWords.MethodWithKeyWordRefForClass(ref param);
            var actual = param.Property;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MethodWithoutKeyWordRefForClassTest()
        {
            var expected = 1;

            var param = new HelperForClassForTestKeyWords(property: 1);
            var classForTestKeyWords = new ClassForTestKeyWords();
            classForTestKeyWords.MethodWithoutKeyWordRefForClass(param);
            var actual = param.Property;

            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod()]
        [DataRow(0,false)]
        [DataRow(1, true)]
        [DataRow(-1, true)]
        [DataRow(2, true)]
        [DataRow(-2, true)]
        public void ConvertIntToBoolTest(int parameterForConvertion, bool expected)
        {
            var actual = Convert.ToBoolean(parameterForConvertion);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ConvertDoubleNullableToDouble()
        {
            var expected = 0;
            double? a = 0;
            var actual = (double) a;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MethodWithKeyWordOutForClassTest()
        {
            var expected = 1;
            var param = 0;
            var classForTestKeyWords = new ClassForTestKeyWords();
            classForTestKeyWords.MethodWithKeyWordOutForClass(out param);

            var actual = param;

            Assert.AreEqual(expected, actual);
        }
    }

    public class ClassForTestKeyWords
    {
        public void MethodWithKeyWordRefForStruct(ref int param)
        {
            param++;
        }

        public void MethodWithoutKeyWordRefForStruct(int param)
        {
            param++;
        }

        public void MethodWithKeyWordRefForClass(ref HelperForClassForTestKeyWords param)
        {
            param = new HelperForClassForTestKeyWords(2);
        }

        public void MethodWithoutKeyWordRefForClass(HelperForClassForTestKeyWords param)
        {
            param = new HelperForClassForTestKeyWords(2);
        }

        public void MethodWithKeyWordOutForClass(out int param)
        {
            param = 1;
        }

        public void MethodWithoutKeyWordOutForClass(int param)
        {
            param++;
        }
    }

    public class HelperForClassForTestKeyWords
    {
        public int Property { get; }

        public HelperForClassForTestKeyWords(int property)
        {
            Property = property;
        }
    }
}