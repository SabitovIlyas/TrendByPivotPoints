using System;
using System.Collections.Generic;
using TSLab.DataSource;
using TSLab.Script;
using TSLab.Script.CanvasPane;
using TSLab.Script.Control;
using TSLab.Script.DataGridPane;
using TSLab.Script.Handlers;
using TSLab.Utils;

namespace TrendByPivotPointsStarter
{
    public class CustomContext : IContext
    {

        public static CustomContext Create()
        {
            return new CustomContext();
        }

        private CustomContext() { }
        public bool IsFixedBarsCount => throw new NotImplementedException();

        public bool IsOptimization => throw new NotImplementedException();

        public int TradeFromBar => throw new NotImplementedException();

        public bool IsLastBarUsed => throw new NotImplementedException();

        public bool IsLastBarClosed => throw new NotImplementedException();

        public double ScriptResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Dictionary<string, double> ScriptResults => throw new NotImplementedException();

        public IRuntime Runtime => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Title => throw new NotImplementedException();

        public IGraphPane First => throw new NotImplementedException();

        public IList<IBasePane> Panes => throw new NotImplementedException();

        public int BarsCount => throw new NotImplementedException();

        public IContext Context => throw new NotImplementedException();

        public DisposeState DisposeState => throw new NotImplementedException();

        public bool IsDisposed => throw new NotImplementedException();

        public bool IsDisposedOrDisposing => throw new NotImplementedException();

        public void AddUnremovableInteractiveObjectId(string id)
        {
            throw new NotImplementedException();
        }

        public IWindow AddWindow(string name, string title)
        {
            throw new NotImplementedException();
        }

        public void Attach()
        {
            throw new NotImplementedException();
        }

        public void ClearLog()
        {
            throw new NotImplementedException();
        }

        public bool ContainsGhostInteractiveObjectId(string id)
        {
            throw new NotImplementedException();
        }

        public bool ContainsUnremovableInteractiveObjectId(string id)
        {
            throw new NotImplementedException();
        }

        public ICanvasPane CreateCanvasPane(string name, string title, bool addToTop = false)
        {
            throw new NotImplementedException();
        }

        public IControlPane CreateControlPane(string id, string name, string title, bool addToTop = false)
        {
            throw new NotImplementedException();
        }

        public IDataGridPane CreateDataGridPane(string name, string title, int displayIndexValueX, string formatValueX, string headerValueX, bool isVisibleValueX, TextAlignment textAlignmentValueX, int? widthValueX, int displayIndexDateTime, string formatDateTime, string headerDateTime, bool isVisibleDateTime, TextAlignment textAlignmentDateTime, int? widthDateTime, bool addToTop = false)
        {
            throw new NotImplementedException();
        }

        public IGraphPane CreateGraphPane(string name, string title, bool addToTop = false)
        {
            throw new NotImplementedException();
        }

        public IGraphPane CreatePane(string title, double sizePct, bool hideLegend, bool addToTop = false)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public T[] GetArray<T>(int count)
        {
            throw new NotImplementedException();
        }

        public IList<Double2> GetData(string handlerName, string[] parameters, CacheObjectMaker<IList<Double2>> maker)
        {
            throw new NotImplementedException();
        }

        public IList<double> GetData(string handlerName, string[] parameters, CacheObjectMaker<IList<double>> maker)
        {
            throw new NotImplementedException();
        }

        public IList<bool> GetData(string handlerName, string[] parameters, CacheObjectMaker<IList<bool>> maker)
        {
            throw new NotImplementedException();
        }

        public IList<int> GetData(string handlerName, string[] parameters, CacheObjectMaker<IList<int>> maker)
        {
            throw new NotImplementedException();
        }

        public double? GetGraphPaneSize(string graphPaneName)
        {
            throw new NotImplementedException();
        }

        public ILastContractsTradeStatistics GetLastContractsTradeStatistics(string id, Func<ILastContractsTradeStatistics> maker)
        {
            throw new NotImplementedException();
        }

        public ITradesCache GetTradesCache(ISecurity security)
        {
            throw new NotImplementedException();
        }

        public ITradeStatistics GetTradeStatistics(string id, Func<ITradeStatistics> maker)
        {
            throw new NotImplementedException();
        }

        public object LoadGlobalObject(string key, bool fromStorage = false)
        {
            throw new NotImplementedException();
        }

        public T LoadGlobalObject<T>(string key, CacheObjectMaker<T> maker, bool fromStorage = false) where T : class
        {
            throw new NotImplementedException();
        }

        public object LoadObject(string key, bool fromStorage = false)
        {
            throw new NotImplementedException();
        }

        public T LoadObject<T>(string key, CacheObjectMaker<T> maker, bool fromStorage = false) where T : class
        {
            throw new NotImplementedException();
        }

        public IDisposable Lock()
        {
            throw new NotImplementedException();
        }

        public void Log(string text, Color color)
        {
            throw new NotImplementedException();
        }

        public void Log(string text, Color color, bool toMessageWindow)
        {
            throw new NotImplementedException();
        }

        public void Log(string text, Color color, bool toMessageWindow, IDictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        public void Log(string text, MessageType type = MessageType.Info, bool toMessageWindow = false, IDictionary<string, object> context = null)
        {
            //throw new NotImplementedException();
        }

        public void Recalc()
        {
            throw new NotImplementedException();
        }

        public void Recalc(string recalcReason, IDataSourceSecurity dataSourceSecurity)
        {
            throw new NotImplementedException();
        }

        public void ReleaseArray(Array array)
        {
            throw new NotImplementedException();
        }

        public bool RemoveGhostInteractiveObjectId(string id)
        {
            throw new NotImplementedException();
        }

        public void StoreGlobalObject(string key, object data, bool toStorage = false)
        {
            throw new NotImplementedException();
        }

        public void StoreObject(string key, object data, bool toStorage = false)
        {
            throw new NotImplementedException();
        }

        public ISecurity GetData(string handlerName, string[] parameters, CacheObjectMaker<ISecurity> maker)
        {
            throw new NotImplementedException();
        }
    }
}
