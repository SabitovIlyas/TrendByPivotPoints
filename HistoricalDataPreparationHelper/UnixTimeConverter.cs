using System;

namespace HistoricalDataPreparationHelper
{
    public partial class Program
    {
        public class UnixTimeConverter
        {
            private readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);            
            public DateTime FromUnixTimeSeconds(long unixTimestamp)
            {
                return UnixEpoch.AddSeconds(unixTimestamp);
            }
        }
    }
}
