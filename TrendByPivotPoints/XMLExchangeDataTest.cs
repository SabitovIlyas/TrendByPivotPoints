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
            var tradingSystem = new XElement("TradingSystem", new XAttribute("Description", "TradingSystemDonchian5520"),
                new XElement("Security", "BRH4"),
                new XElement("PositionSide", "Long"),
                new XElement("Units", "4")
                );

            var doc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"), tradingSystem);
            doc.Save("test.xml");            

            Assert.AreEqual(true, true);
        }
    }
}