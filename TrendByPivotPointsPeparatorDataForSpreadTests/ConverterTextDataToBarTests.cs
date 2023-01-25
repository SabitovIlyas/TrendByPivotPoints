using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPointsPeparatorDataForSpread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using System.IO;

namespace TrendByPivotPointsPeparatorDataForSpread.Tests
{
    [TestClass()]
    public class ConverterTextDataToBarTests
    {
        [TestMethod()]
        public void ConvertTest()
        {
            //< TICKER >,< PER >,< DATE >,< TIME >,< OPEN >,< HIGH >,< LOW >,< CLOSE >,< VOL >
            //SPFB.BR - 3.23,1,20230103,090000,90.5900000,90.5900000,90.5800000,90.5800000,7
            var expected = Bar.Create(new DateTime(2023, 01, 03, 9, 0, 0), 90.59, 90.59, 90.58, 90.58, 
                7, "SPFB.BR - 3.23", "1");
            var converter = ConverterTextDataToBar.Create();
            var actual = converter.Convert("SPFB.BR - 3.23,1,20230103,090000,90.5900000,90.5900000,90.5800000,90.5800000,7");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ConvertFileWithBarsToListOfBarsTest() 
        {
            var expected = 7201;
            var converter = ConverterTextDataToBar.Create("SPFB.BR-3.23_230101_230131.txt");
            
            var bars = converter.ConvertFileWithBarsToListOfBars();
            var actual = bars.Count();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void SpreadCreateTest()
        {
            var bars1 = new List<Bar>
            {
                Bar.Create(new DateTime(2023, 01, 03, 9, 0, 0), 88.84, 88.85, 88.46, 88.49),
                Bar.Create(new DateTime(2023, 01, 03, 9, 1, 0), 88.49, 88.49, 88.35, 88.44),
                Bar.Create(new DateTime(2023, 01, 03, 9, 2, 0), 88.44, 88.53, 88.44, 88.47)
            };

            var bars2 = new List<Bar>
            {
                Bar.Create(new DateTime(2023, 01, 03, 9, 0, 0), 90.59, 90.59, 90.58, 90.58),
                Bar.Create(new DateTime(2023, 01, 03, 9, 1, 0), 90.26, 90.26, 90.25, 90.25),
                Bar.Create(new DateTime(2023, 01, 03, 9, 3, 0), 90.34, 90.34, 90.33, 90.33)
            };

            var expected = new List<Bar>
            {
                Bar.Create(new DateTime(2023, 01, 03, 9, 0, 0), -1.75, -1.75, -2.09, -2.09),
                Bar.Create(new DateTime(2023, 01, 03, 9, 1, 0), -1.77, -1.77, -1.81, -1.81)
            };

            var spread = Spread.Create(bars1, bars2);
            var actual = spread.Bars;

            if (expected.Count != actual.Count)
                Assert.Fail();

            for (var i = 0; i < expected.Count; i++)
            {                
                if (expected[i] != actual[i])
                    Assert.Fail();
            }
        }
        [TestMethod]
        public void BarToStringTest()
        {
            var expected = "SPFB.BR - 3.23,1,20230103,090000,90.5900000,90.5900000,90.5800000,90.5800000,7";
            var bar = Bar.Create(new DateTime(2023, 01, 03, 9, 0, 0), 90.59, 90.59, 90.58, 90.58,
                7, "SPFB.BR - 3.23", "1");
            var actual = bar.ToString();
            Assert.AreEqual(expected, actual);
        }
    }
}