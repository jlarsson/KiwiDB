using System.Collections.Generic;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Index
{
    public interface IIndexCatalog
    {
        IEnumerable<KeyValuePair<string,IIndex>> EnumerateIndices { get; }
        IIndex GetIndex(string memberPath);
        void EnsureIndex(IndexDefinition indexDefinition);
        bool DropIndex(string memberPath);
        void UpdateIndex(string key, IJsonValue oldValue, IJsonValue newValue);
        IEnumerable<KeyValuePair<string, IJsonValue>> FindIndexedObjects(IJsonValue filter);
        void SaveChanges();
    }
}