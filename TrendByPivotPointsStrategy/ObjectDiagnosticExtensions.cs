using System;
using System.Text;

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
}