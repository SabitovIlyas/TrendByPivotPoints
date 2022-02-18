using System;
using TSLab.Script.Handlers;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class RealTimeTrading
    {
        public static RealTimeTrading Create(PositionSide positionSide, string tradingSystemDescription, IContext ctx)
        {
            return new RealTimeTrading(positionSide, tradingSystemDescription, ctx);
        }
        public Logger Logger { get; set; } = new NullLogger();

        //private void Log(string text)
        //{
        //    Logger.Log("{0}: {1}", stopLossDescription, text);
        //}

        private PositionSide positionSide;
        private string tradingSystemDescription;
        private IContext ctx;

        private void Log(string text, params object[] args)
        {
            text = string.Format(text, args);
            Log(text);
        }

        private RealTimeTrading(PositionSide positionSide, string tradingSystemDescription, IContext ctx)
        {
            this.positionSide = positionSide;
            this.tradingSystemDescription = tradingSystemDescription;
            this.ctx = ctx;
        }
        public bool WasNewPositionOpened()
        {
            return LoadFlagNewPositionOpened();
        }

        private bool LoadFlagNewPositionOpened()
        {
            if (positionSide == PositionSide.Long || positionSide == PositionSide.Short)
            {
                var containerName = "Новая позиция " + positionSide;
                Logger.Log("Название контейнера: " + containerName);
                var value = (bool)LoadObjectFromContainer(containerName);
                return value;
            }
            else
            {
                throw new Exception("Вызываю исключение, так как значение positionSide ожидалось Long или Short, а оно " + positionSide);
            }
        }

        private object LoadObjectFromContainer(string key)
        {
            var containerName = tradingSystemDescription + key;
            var container = ctx.LoadObject(containerName) as NotClearableContainer<object>;
            object value;
            if (container != null)
                value = container.Content;
            else
                throw new NullReferenceException("container равно null");

            return value;
        }
    }
}