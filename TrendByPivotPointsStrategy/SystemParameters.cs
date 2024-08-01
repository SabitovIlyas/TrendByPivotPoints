using System.Collections.Generic;
using TSLab.Script.Optimization;

namespace TradingSystems
{
    public class SystemParameters
    {
        private readonly Dictionary<string, object> parameters = new Dictionary<string, object>();

        public void Add(string key, object value)
        {
            if (!parameters.ContainsKey(key))
                parameters.Add(key, (OptimProperty)value);
        }

        public object GetValue(string key)
        {            
            if (parameters.TryGetValue(key, out object value))
                return value;
            else
            {
                throw new KeyNotFoundException("Не найден параметр " + key);
            }
        }
    }
}