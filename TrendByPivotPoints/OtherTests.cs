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
        public void GetLotsBasedOnStartLotTest()
        {
            var expected = 8;
            var positionShares = 7;
            var startLorts = 4;
            var coef = 2;

            var actual = GetLotsBasedOnStartLot(positionShares, startLorts, coef);
            
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

        [TestMethod()]
        public void DateTimeWithdrawTest1()
        {            
            var endDate = new DateTime(2023, 11, 30);
            var tS = TimeSpan.FromDays(30);
            var expected = new DateTime(2023, 10, 31);

            var actual = endDate - tS;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest2()
        {
            var endDate = new DateTime(2023, 11, 30);
            var expected = new DateTime(2023, 11, 01);

            var actual = (endDate - TimeSpan.FromDays(30-1)).Date;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest3()
        {
            var endDate = new DateTime(2023, 11, 30, 10, 0, 0);
            var expected = new DateTime(2021, 12, 1, 10, 0, 0);
            var diff = 24;

            var actual = GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }
      
        private DateTime GetStartDateTime(DateTime endDate, int diff)
        {
            int actualYear = endDate.Year;
            int actualMonth = endDate.Month - diff + 1;

            //if (endDate.Month == diff)
            //{
            //    actualYear = endDate.Year - 1;
            //    actualMonth = 12;
            //}
            //else 
            if (endDate.Month < diff)
            {
                int n = diff / 12;
                actualMonth = endDate.Month - diff + 1 + n * 12;
                actualYear = endDate.Year - n;
            }

            return new DateTime(actualYear, actualMonth, 1, 10, 0, 0);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest4()
        {
            var endDate = new DateTime(2023, 11, 30, 10, 0, 0);
            var expected = new DateTime(2022, 11, 1, 10, 0, 0);
            var diff = 13;

            var actual = GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest5()
        {
            var endDate = new DateTime(2023, 11, 30, 10, 0, 0);//1-11 2-10 3-9 4-8 5-7 6-6 7-5 8-4 9-3 10-2 11-1 12-12 13-11 14-10 15 -9 16-8 17-7 18-6 19-5 20-4 21-3 22-2 23-1 24-12 
            var expected = new DateTime(2022, 12, 1, 10, 0, 0);
            var diff = 12;

            var actual = GetStartDateTime(endDate, diff);
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest6()
        {
            var endDate = new DateTime(2023, 11, 30, 10, 0, 0);
            var expected = new DateTime(2023, 11, 1, 10, 0, 0);
            var diff = 1;

            var actual = GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest7()
        {
            var endDate = new DateTime(2023, 12, 29, 10, 0, 0);
            var expected = new DateTime(2023, 01, 1, 10, 0, 0);
            var diff = 12;

            var actual = GetStartDateTime(endDate, diff);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void DateTimeWithdrawTest8()
        {
            var endDate = new DateTime(2023, 12, 31, 10, 0, 0);
            var expected = new DateTime(2023, 01, 1, 10, 0, 0);
            var diff = 12;

            var actual = GetStartDateTime(endDate, diff);

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