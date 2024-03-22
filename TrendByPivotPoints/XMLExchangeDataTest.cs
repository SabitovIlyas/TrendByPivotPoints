using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class XMLExchangeDataTest
    {
        //[DataTestMethod]
        //public void XMLCreateTest()
        //{
        //    var tradingSystems = new XElement
        //    (
        //        new XElement
        //        (
        //            "TradingSystems",
        //                new XElement("TradingSystem",
        //                    new XAttribute("Description", "TradingSystemDonchianBr"),
        //                    new XAttribute("Security", "BRH4"),
        //                    new XAttribute("PositionSide", "Long"),
        //                    new XElement("Units", "4")
        //                ),
        //                new XElement("TradingSystem",
        //                    new XAttribute("Description", "TradingSystemDonchianEu"),
        //                    new XAttribute("Security", "EuH4"),
        //                    new XAttribute("PositionSide", "Short"),
        //                    new XElement("Units", "1")
        //                )
        //        )
        //    );

        //    var docOut = new XDocument(new XDeclaration("1.0", "utf-16", "yes"), tradingSystems);
        //    var fileName = "test.xml";
        //    docOut.Save(fileName);

        //    var docIn = XDocument.Load(fileName);

        //    var els = docIn.Elements();



        //    Assert.AreEqual(true, true);
        //}
    }
}