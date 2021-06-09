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
    public class ReadOnlyList : IReadOnlyList<IDataBar>
    {
        public IDataBar this[int index] => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public IEnumerator<IDataBar> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
