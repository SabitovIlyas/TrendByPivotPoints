using System;
using TradingSystems;
using TSLab.DataSource;
using Security = TradingSystems.Security;

namespace TrendByPivotPointsOptimizator
{
    public class ChromosomeDonchianChannel
    {
        public bool FitnessPassed { get; set; }
        public double FitnessValue { get; set; }
        public Ticker Ticker { get; set; }
        public Interval TimeFrame { get; set; }
        public PositionSide Side { get; set; }
        public int FastDonchian { get; set; }
        public int SlowDonchian { get; set; }
        public int AtrPeriod { get; set; }
        public int LimitOpenedPositions { get; set; }
        public double KAtrForOpenPosition { get; set; }
        public double KAtrForStopLoss { get; set; }

        public ChromosomeDonchianChannel(Ticker ticker, Interval timeFrame, PositionSide side, int fastDonchian, int slowDonchian, int atrPeriod, int limitOpenedPositions, double kAtrForOpenPosition, double kAtrForStopLoss)
        {
            Ticker = ticker;
            TimeFrame = timeFrame;
            Side = side;
            FastDonchian = fastDonchian;
            SlowDonchian = slowDonchian;
            AtrPeriod = atrPeriod;
            LimitOpenedPositions = limitOpenedPositions;
            KAtrForOpenPosition = kAtrForOpenPosition;
            KAtrForStopLoss = kAtrForStopLoss;
        }
    }

    public class FitnessDonchianChannel
    {
        private Security security;
        private ChromosomeDonchianChannel chromosome;
        private Optimizator optimizator;

        public FitnessDonchianChannel(Security security, ChromosomeDonchianChannel chromosome, 
            Optimizator optimizator)
        {
            this.security = security;
            this.chromosome = chromosome;
            this.optimizator = optimizator;
            Calc();
        }
        
        private void Calc()
        {
            //Я здесь
            //Параметры для оптимизации: быстрая доунчиан, медленная доунчиан, период АТР,
            //лимит открытых позиций, коэф. АТР для открытия позиции, коэф. АТР для стоп-лосса
            optimizator.GetOptimalParametersPercent(
                points:null,
                dimension: 6, // Количество параметров  
                radiusNeighbourInPercent:new int[3] { 5, 5, 5 },
                barrier: 1.0, // Пороговое значение
                isCheckedPass: true); // Проверка на прохождение барьера            

            chromosome.FitnessPassed = CalcIsPassed();
            chromosome.FitnessValue = CalcValue();
        }

        private bool CalcIsPassed()
        {
            throw new NotImplementedException();
        }

        private double CalcValue()
        {
            throw new NotImplementedException();
        }
    }
}