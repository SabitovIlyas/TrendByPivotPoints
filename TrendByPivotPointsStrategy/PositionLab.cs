namespace TradingSystems
{
    public class PositionLab : Position
    {
        public int BarNumber { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public double EntryPrice { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public double Profit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Security Security { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public string SignalNameForOpenPosition => throw new System.NotImplementedException();

        public string SignalNameForClosePosition => throw new System.NotImplementedException();

        public void CloseAtMarket(int barNumber, string signalNameForClosePosition)
        {
            throw new System.NotImplementedException();
        }

        public void CloseAtStop(int barNumber, double stopPrice, string signalNameForClosePosition)
        {
            throw new System.NotImplementedException();
        }
    }
}
