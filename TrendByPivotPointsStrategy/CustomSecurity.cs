using System;
using System.Collections.Generic;
using TSLab.DataSource;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Utils;

namespace TradingSystems
{
    public class CustomSecurity : ISecurity
    {
        FinInfo finInfo;
        IReadOnlyList<IDataBar> bars;
        
        public static CustomSecurity Create(IReadOnlyList<IDataBar> bars)
        {
            return new CustomSecurity(bars);
        }

        public static CustomSecurity Create(float initDeposit, FinInfo finInfo, IReadOnlyList<IDataBar> bars)
        {
            return new CustomSecurity(initDeposit, finInfo, bars);
        }

        private CustomSecurity(IReadOnlyList<IDataBar> bars)
        {       
            this.bars = bars;
        }

        private CustomSecurity(float initDeposit, FinInfo finInfo, IReadOnlyList<IDataBar> bars)
        {
            InitDeposit = initDeposit;
            this.finInfo = finInfo;
            this.bars = bars;
        }        

        public string Symbol => throw new NotImplementedException();

        public IDataSourceSecurity SecurityDescription => throw new NotImplementedException();

        public FinInfo FinInfo => finInfo;

        public IReadOnlyList<IDataBar> Bars => bars;

        public bool IsBarsLoaded => throw new NotImplementedException();

        public IList<double> OpenPrices => throw new NotImplementedException();

        public IList<double> ClosePrices => throw new NotImplementedException();

        public IList<double> HighPrices => throw new NotImplementedException();

        public IList<double> LowPrices => throw new NotImplementedException();

        public IList<double> Volumes => throw new NotImplementedException();

        public Interval IntervalInstance => throw new NotImplementedException();

        public int Interval => throw new NotImplementedException();

        public DataIntervals IntervalBase => throw new NotImplementedException();

        public double LotSize => throw new NotImplementedException();

        public double LotTick => throw new NotImplementedException();

        public double Margin => throw new NotImplementedException();

        public double Tick => throw new NotImplementedException();

        public int Decimals => throw new NotImplementedException();

        public IPositionsList Positions => throw new NotImplementedException();

        public CommissionDelegate Commission { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string CacheName => throw new NotImplementedException();

        public double InitDeposit { get; set; }

        public bool IsRealtime => throw new NotImplementedException();

        public bool IsPortfolioReady => throw new NotImplementedException();

        public bool SimulatePositionOrdering => throw new NotImplementedException();

        public bool IsAligned => throw new NotImplementedException();

        public DisposeState DisposeState => throw new NotImplementedException();

        public bool IsDisposed => throw new NotImplementedException();

        public bool IsDisposedOrDisposing => throw new NotImplementedException();

        public void Attach()
        {
            throw new NotImplementedException();
        }

        public ISecurity CloneAndReplaceBars(IEnumerable<IDataBar> newCandles)
        {
            throw new NotImplementedException();
        }

        public ISecurity CloneAndReplaceBars(IReadOnlyList<IDataBar> newcandles)
        {
            throw new NotImplementedException();
        }

        public ISecurity CompressTo(int interval)
        {
            throw new NotImplementedException();
        }

        public ISecurity CompressTo(Interval interval)
        {
            throw new NotImplementedException();
        }

        public ISecurity CompressTo(Interval interval, int shift)
        {
            throw new NotImplementedException();
        }

        public ISecurity CompressTo(Interval interval, int shift, int adjustment, int adjShift)
        {
            throw new NotImplementedException();
        }

        public ISecurity CompressToPriceRange(Interval interval)
        {
            throw new NotImplementedException();
        }

        public ISecurity CompressToVolume(Interval interval)
        {
            throw new NotImplementedException();
        }

        public void ConnectDoubleList(IGraphListBase list, IDoubleHandlerWithUpdate handler)
        {
            throw new NotImplementedException();
        }

        public void ConnectSecurityList(IGraphListBase list)
        {
            throw new NotImplementedException();
        }

        public IList<double> Decompress(IList<double> candles)
        {
            throw new NotImplementedException();
        }

        public IList<TK> Decompress<TK>(IList<TK> candles, DecompressMethodWithDef method) where TK : struct
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IQueueData> GetBuyQueue(int barNum)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IQueueData> GetSellQueue(int barNum)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ITrade> GetTrades(int barNum)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ITrade> GetTrades(int firstBarIndex, int lastBarIndex)
        {
            throw new NotImplementedException();
        }

        public int GetTradesCount(int firstBarIndex, int lastBarIndex)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IReadOnlyList<ITrade>> GetTradesPerBar(int firstBarIndex, int lastBarIndex)
        {
            throw new NotImplementedException();
        }

        public IDisposable Lock()
        {
            throw new NotImplementedException();
        }

        public double RoundPrice(double price)
        {
            throw new NotImplementedException();
        }

        public double RoundShares(double shares)
        {
            throw new NotImplementedException();
        }

        public void UpdateQueueData()
        {
            throw new NotImplementedException();
        }
    }
}
