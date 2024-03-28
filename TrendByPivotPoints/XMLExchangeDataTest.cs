using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Xml.Linq;

namespace TradingSystems.Tests
{
    [TestClass()]
    public class XMLExchangeDataTest
    {
        [DataTestMethod]
        public void XMLSaveLoadLinqTest()
        {
            var expectedLong = 7;
            var expectedShort = 1;

            var tradingSystems = new XElement
            (
                new XElement
                (
                    "TradingSystems",
                        new XElement("TradingSystem",
                            new XAttribute("Description", "TradingSystemDonchianBr"),
                            new XAttribute("Security", "BRH4"),
                            new XAttribute("PositionSide", "Long"),
                            new XElement("Units", "4")
                        ),
                        new XElement("TradingSystem",
                            new XAttribute("Description", "TradingSystemDonchianEu"),
                            new XAttribute("Security", "EuH4"),
                            new XAttribute("PositionSide", "Short"),
                            new XElement("Units", "1")
                        ),
                        new XElement("TradingSystem",
                            new XAttribute("Description", "TradingSystemDonchianSV"),
                            new XAttribute("Security", "SVH4"),
                            new XAttribute("PositionSide", "Long"),
                            new XElement("Units", "3")
                        )
                )
            );

            var docOut = new XDocument(new XDeclaration("1.0", "utf-16", "yes"), tradingSystems);
            var fileName = "test.xml";
            docOut.Save(fileName);

            ReadXMLForTotalPostiions(out int actualLong, out int actualShort);

            Assert.AreEqual(expectedLong, actualLong);
            Assert.AreEqual(expectedShort, actualShort);
        }

        private void ReadXMLForTotalPostiions(out int totalLongPosition, out int totalShortPosition)
        {
            var fileName = "test.xml";
            var docIn = XDocument.Load(fileName);
            var root = docIn.Elements();
            var tradingSystems = root.Elements();

            var longPositions = from ts in tradingSystems
                                where (string)ts.Attribute("PositionSide") == "Long"
                                select ts;

            var shortPositions = from ts in tradingSystems
                                where (string)ts.Attribute("PositionSide") == "Short"
                                select ts;

            var longValues = from ts in longPositions
                             select ts.Element("Units");

            var shortValues = from ts in shortPositions
                             select ts.Element("Units");
            
            totalLongPosition = longValues.Sum(l => int.Parse(l.Value));
            totalShortPosition = shortValues.Sum(l => int.Parse(l.Value));
        }
    }
}