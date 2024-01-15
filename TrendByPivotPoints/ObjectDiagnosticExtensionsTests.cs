using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;
using TrendByPivotPoints;
using TSLab.Utils;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class ObjectDiagnosticExtensionsTests
    {
        [TestMethod()]
        public void GetPropertiesStateTest()
        {
            var expected = "Property1: 7\r\nProperty2: 13\r\n";
            var myClass = new ClassForTestReflection();
            var actual = myClass.GetPropertiesState();
            Assert.AreEqual(expected, actual);
        }
    }
}