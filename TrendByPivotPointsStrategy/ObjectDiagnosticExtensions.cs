using System;
using System.Collections.Generic;
using System.Text;
using TradingSystems;
using TSLab.DataSource;

namespace TrendByPivotPoints
{
    public static class ObjectDiagnosticExtensions
    {
        public static string GetPropertiesState(this object o)
        {
            Type myType = o.GetType();
            var properties = myType.GetProperties();
            var objectDiagnostic = new StringBuilder();

            foreach (var property in properties)
            {
                var info = string.Format("{0}: {1}", property.Name, property.GetValue(o));
                objectDiagnostic.AppendLine(info);
            }

            return objectDiagnostic.ToString();
        }
    }

    public static class ListIDataBarExtensions
    {
        public static bool IsListBarEquals(this IReadOnlyList<IDataBar> expectedBars, IReadOnlyList<IDataBar> actualBars)
        {
            var checkBars = false;
            if (expectedBars.Count == actualBars.Count)
            {
                int counter;
                for (counter = 0; counter < expectedBars.Count; counter++)
                {
                    if (expectedBars[counter].Date != actualBars[counter].Date ||
                        expectedBars[counter].Open != actualBars[counter].Open ||
                        expectedBars[counter].High != actualBars[counter].High ||
                        expectedBars[counter].Low != actualBars[counter].Low ||
                        expectedBars[counter].Close != actualBars[counter].Close)
                        break;
                }
                if (counter == expectedBars.Count)
                    checkBars = true;
            }

            return checkBars;
        }
    }
}