using System.Collections.Generic;

namespace TradingSystems
{
    public class CurrencyConverter
    {
        public Currency BaseCurrency { get; private set; }

        private readonly Dictionary<Currency, double> currencyRates = new Dictionary<Currency, double>();

        public CurrencyConverter(Currency baseCurrency)
        {
            BaseCurrency = baseCurrency;
            currencyRates.Add(baseCurrency, 1);
        }

        public void AddCurrencyRate(Currency currency, double rate)
        {
            if (!currencyRates.ContainsKey(currency))
                currencyRates.Add(currency, rate);
        }

        public double GetCurrencyRate(Currency currency)
        {
            if (currencyRates.TryGetValue(currency, out double rate))
                return rate;
            else
            {
                throw new KeyNotFoundException("Не найдена валюта " + currency.ToString());
            }
        }
    }
}
