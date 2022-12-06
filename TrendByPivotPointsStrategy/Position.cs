using TSLab.Script;

namespace TrendByPivotPointsStrategy
{
    public class Position
    {
        //TODO: Позакрывать свойства от изменения

        public double EntryPrice { get; set; }
        public double Profit { get; set; }
        public int BarNumber { get; set; }
        public Security Security { get; set; }
        //TODO: iPosition спрятать за "адаптером"
        public IPosition iPosition { get; }

        public static Position Create(IPosition iPosition)
        {
            return new Position(iPosition);
        }

        //TODO: Сделать конструкторы приватными и довести их до ума
        public Position()
        {
        }

        public Position(IPosition iPosition)
        {
            EntryPrice = iPosition.EntryPrice;
            this.iPosition = iPosition;
        }
        
    }
}
