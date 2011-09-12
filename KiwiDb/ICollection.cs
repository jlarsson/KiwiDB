using System;
using System.Collections.Generic;
using Kiwi.Json.Untyped;

namespace KiwiDb
{
    public interface ICollection
    {
        ICollectionIndices Indices { get; }
        T ExecuteRead<T>(Func<ICollection, T> action);
        T ExecuteWrite<T>(Func<ICollection, T> action);
        IList<KeyValuePair<string, IJsonValue>> Find(IJsonValue filter);
        IJsonValue Get(string key);
        IJsonValue Update(string key, IJsonValue value);
        IJsonValue Remove(string key);
    }
}