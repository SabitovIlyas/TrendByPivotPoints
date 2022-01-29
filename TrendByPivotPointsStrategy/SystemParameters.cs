using System.Collections.Generic;
using TSLab.Script.Optimization;

namespace TrendByPivotPointsStrategy
{
    public class SystemParameters
    {
        private readonly Dictionary<string, OptimProperty> parameters = new Dictionary<string, OptimProperty>();
        public void Add(string key, OptimProperty value)
        {
            if (!parameters.ContainsKey(key))
                parameters.Add(key, value);
        }

        private OptimProperty GetValue(string key)
        {
            OptimProperty value;
            if (parameters.TryGetValue(key, out value))
                return value;
            else
                throw new KeyNotFoundException("Не найден параметр " + key);
        }

        public double GetDouble(string key)
        {
            return GetValue(key);
        }

        public int GetInt(string key)
        {
            return GetValue(key);
        }
    }
}