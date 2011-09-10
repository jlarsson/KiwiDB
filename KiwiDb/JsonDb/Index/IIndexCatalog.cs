using System.Collections.Generic;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Index
{
    public interface IIndexCatalog
    {
        IEnumerable<IIndex> EnumerateIndices { get; }
        void EnsureIndex(IndexDefinition indexDefinition);
        void UpdateIndex(string key, IJsonValue oldValue, IJsonValue newValue);
        IEnumerable<KeyValuePair<string, IJsonValue>> FindIndexedObjects(IJsonValue filter);
        void SaveChanges();
    }
}