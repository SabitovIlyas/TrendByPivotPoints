namespace TrendByPivotPoints
{
    public class PositionOpenCasePatternPivotPoints
    {
        private Pattern patternPivotPoints;

        public bool NeedOpenPosition()
        {
            var signal = patternPivotPoints.Check();
            var signalIsActual = false;

            return signal && signalIsActual;
        }
    }
}
