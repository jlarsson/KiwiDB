using System;
using System.Collections.Generic;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb
{
    public interface ICollection
    {
        ICollectionIndices Indices { get; }
        T ExecuteRead<T>(Func<ICollection, T> action);
        T ExecuteWrite<T>(Func<ICollection, T> action);
        IJsonValue Get(string key);
        IJsonValue Remove(string key);
        IList<KeyValuePair<string, IJsonValue>> Find(IJsonValue filter);
        IJsonValue Update(string key, IJsonValue value);
    }
}