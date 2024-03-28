using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TradingSystems.Tests
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
        [DataRow(0, false)]
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
            var actual = (double)a;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MethodWithKeyWordOutForClassTest()
        {
            var expected = 1;
            var classForTestKeyWords = new ClassForTestKeyWords();
            classForTestKeyWords.MethodWithKeyWordOutForClass(out int param);

            var actual = param;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetLotsBasedOnStartLotTest()
        {
            var expected = 8;
            var positionShares = 7;
            var startLorts = 4;
            var coef = 2;

            var actual = GetLotsBasedOnStartLot(positionShares, startLorts, coef);
            Assert.AreEqual(expected, actual);
        }

        private int GetLotsBasedOnStartLot(int positionShares, int startLots, int coef)
        {
            if (coef <= 1)
                throw new Exception("Коэффициент должен быть больше 1");
            var lots = startLots;
            while (positionShares > lots)
                lots *= coef;

            return lots;
        }

        [TestMethod()]
        public void IsCurrentPositionSharesCorrectTest_False()
        {
            var expected = false;
            var positionShares = 7;
            var startLorts = 4;
            var coef = 2;

            var actual = IsCurrentPositionSharesCorrect(positionShares, startLorts, coef);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void IsCurrentPositionSharesCorrectTest_True()
        {
            var expected = true;
            var positionShares = 8;
            var startLorts = 4;
            var coef = 2;

            var actual = IsCurrentPositionSharesCorrect(positionShares, startLorts, coef);
            Assert.AreEqual(expected, actual);
        }

        private bool IsCurrentPositionSharesCorrect(int positionShares, int startLots, int coef)
        {
            if (coef <= 1)
                throw new Exception("Коэффициент должен быть больше 1");
            var lots = startLots;
            while (positionShares > lots)
                lots *= coef;

            return positionShares == lots;
        }
    }
}