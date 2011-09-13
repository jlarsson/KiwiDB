using System.Collections.Generic;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Index
{
    public interface IIndexValueFactory
    {
        IndexValue GetIndexValue(object o);
        IEnumerable<IndexValue> GetIndexValues(IJsonValue value, string memberPath);
    }
}