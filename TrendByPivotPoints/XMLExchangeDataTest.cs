using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class XMLExchangeDataTest
    {
        [DataTestMethod]
        public void FooTest()
        {
            var l = new XElement("Long");
            var s = new XElement("Short");
            Assert.AreEqual(true, true);
        }
    }
}