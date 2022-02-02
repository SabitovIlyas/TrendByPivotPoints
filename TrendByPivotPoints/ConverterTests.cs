using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrendByPivotPointsStrategy.Tests
{
    [TestClass()]
    public class ConverterTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            
        }

        [TestMethod()]
        public void IsGreaterTest_IsConvertedFalse_ReturnedTrue()
        {
            //arrange            
            var expected = true;

            //act
            var convertable = new Converter(false);            
            var actual = convertable.IsGreater(4.5, 2.0); ;

            //assert            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void IsGreaterTest_IsConvertedTrue_ReturnedFalse()
        {
            //arrange            
            var expected = false;

            //act
            var convertable = new Converter(true);
            var actual = convertable.IsGreater(4.5, 2.0);

            //assert            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void IsLessTest_IsConvertedFalse_ReturnedFalse()
        {
            //arrange            
            var expected = false;

            //act
            var convertable = new Converter(false);
            var actual = convertable.IsLess(4.5, 2.0); ;

            //assert            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void IsLessTest_IsConvertedTrue_ReturnedFalse()
        {
            //arrange            
            var expected = true;

            //act
            var convertable = new Converter(true);
            var actual = convertable.IsLess(4.5, 2.0); ;

            //assert            
            Assert.AreEqual(expected, actual);
        }
    }
}