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

        private bool WasFlagNewPositionOpenedSavedToContainer(bool flag)
        {
            var flagSaved = LoadFlagNewPositionOpened();
            var message = string.Format("Сохранённый флаг = {0}, текущий флаг = {1}", flagSaved, flag);
            Logger.Log(message);
            return flag == flagSaved;
        }

        private DateTime LoadBarFromContainer(string key)
        {
            Logger.Log("LoadBarFromContainer(key = " + key + ")");
            DateTime barDate = DateTime.MinValue;
            try
            {
                barDate = (DateTime)LoadObjectFromContainer(key);
            }
            catch (NullReferenceException e)
            {
                Logger.Log(e.Message + ". Возвращаем константу DateTime.MinValue");
            }

            return barDate;
        }

        private bool WasBarSavedToContainer(DateTime dateBar)
        {
            var dateBarLoaded = LoadBarFromContainer("Последний бар");
            return dateBar == dateBarLoaded;
        }

        public void SaveBarToContainer(DateTime dateBar)
        {
            Logger.Log("Сохраняем бар в контейнер");
            SaveObjectToContainer("Последний бар", dateBar);
            Logger.Log("Проверим, сохранился ли бар в контейнере");

            if (WasBarSavedToContainer(dateBar))
                Logger.Log("Бар сохранился в контейнере");
            else
            {
                Logger.Log("Бар не сохранился в контейнере");
                throw new Exception("Вызываю исключение, так как бар не сохранился в контейнере!");
            }
        }

        private void SaveObjectToContainer(string key, object value)
        {
            var containerName = this.tradingSystemDescription + key;
            var container = new NotClearableContainer<object>(value);
            ctx.StoreObject(containerName, container);
        }

        private void SaveFlagNewPositionOpened(bool flag)
        {
            if (positionSide == PositionSide.Long || positionSide == PositionSide.Short)
            {
                Logger.Log("Взводим флаг открытия новой позиции в контейнере. Флаг: " + flag);
                SaveObjectToContainer("Новая позиция " + positionSide, flag);
                Logger.Log("Проверим, сохранился ли флаг в контейнере");

                if (WasFlagNewPositionOpenedSavedToContainer(flag))
                    Logger.Log("Флаг сохранился в контейнере");
                else
                {
                    Logger.Log("Флаг не сохранился в контейнере");
                    throw new Exception("Вызываю исключение, так как флаг не сохранился в контейнере!");
                }
            }
            else
            {
                throw new Exception("Вызываю исключение, так как значение positionSide ожидалось Long или Short, а оно " + positionSide);
            }
        }

        public void SetFlagNewPositionOpened()
        {
            SaveFlagNewPositionOpened(true);
        }

        public void ResetFlagNewPositionOpened()
        {
            SaveFlagNewPositionOpened(false);
        }
    }
}