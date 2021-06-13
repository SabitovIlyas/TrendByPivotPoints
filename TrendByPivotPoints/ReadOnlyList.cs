using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;
using TSLab.Script;
using TSLab.DataSource;
using TSLab.Utils;
using TSLab.Script.Handlers;


namespace TrendByPivotPointsStrategy.Tests
{
    public class ReadOnlyList<IDataBar> : IReadOnlyList<IDataBar>, IEnumerator<IDataBar>
    {
        private List<IDataBar> bars = new List<IDataBar>();
        public IDataBar this[int index]
        {
            get
            {
                if (index < bars.Count)
                    return bars[index];
                throw new ArgumentOutOfRangeException(nameof(index), "Индекс находится за пределами списка");
            }
        }

        public int Count => bars.Count;

        public object Current => this[position];

        IDataBar IEnumerator<IDataBar>.Current => (IDataBar)Current;

        public void Dispose()
        {
            Reset();
        }

        public IEnumerator<IDataBar> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            if (position < Count - 1)
            {
                position++;
                return true;
            }

            return false;
        }

        private int position = -1;

        public void Reset()
        {
            position = -1;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IDataBar bar)
        {
            bars.Add(bar);
        }
    }
}
