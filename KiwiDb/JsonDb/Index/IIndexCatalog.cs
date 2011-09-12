using System.Collections.Generic;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Index
{
    public interface IIndexCatalog
    {
        IEnumerable<KeyValuePair<string, IndexOptions>> EnumerateIndices { get; }
        IIndex GetIndex(string memberPath);
        bool EnsureIndex(IndexDefinition indexDefinition);
        bool DropIndex(string memberPath);
        void UpdateIndex(string key, IJsonValue oldValue, IJsonValue newValue);
        IEnumerable<KeyValuePair<string, IJsonValue>> FindIndexedObjects(IJsonValue filter);
        void SaveChanges();
    }
}